using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using RMA.Server.Entities;
using RMA.Server.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RMA.Server.Tests
{
    public class RmaAlertBackgroundServiceTests
    {
        [Fact]
        public async Task ExecuteAsync_ProcessesTickets_CorrectlyIgnoringClosedAndUpdatingOpen()
        {
            // Arrange
            var mockFcmService = new Mock<IFcmService>();
            var mockLogger = new Mock<ILogger<RmaAlertBackgroundService>>();
            mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback(new InvocationAction(invocation =>
            {
                var logLevel = (LogLevel)invocation.Arguments[0];
                var state = invocation.Arguments[2];
                var exception = invocation.Arguments[3] as Exception;
                Console.WriteLine($"[LOG - {logLevel}]: {state} {(exception != null ? " - " + exception.ToString() : "")}");
            }));
            var mockConfig = new Mock<IConfiguration>();
            
            // Mock Configuration to return interval
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(s => s.Value).Returns("1");
            mockConfig.Setup(c => c.GetSection("Firebase:CheckIntervalSeconds")).Returns(mockSection.Object);

            var mockTicketRepo = new Mock<FirestoreRepository<RmaTicket>>(null!, "rma_tickets");
            var mockStatusRepo = new Mock<FirestoreRepository<StatusMaster>>(null!, "status_masters");
            var mockCustomerRepo = new Mock<FirestoreRepository<Customer>>(null!, "customers");

            // Mock Data
            var statuses = new List<StatusMaster>
            {
                new() { Id = "status-closed", StatusName = "Closed" },
                new() { Id = "status-open", StatusName = "In Progress" }
            };

            var customers = new List<Customer>
            {
                new() { Id = "cust-1", Name = "Cong ty Song Linh" }
            };

            // Test tickets
            var ticketClosedClean = new RmaTicket
            {
                Id = "t-closed-clean",
                StatusId = "status-closed",
                WarningColor = null,
                IsUrgent = false,
                SentDate = DateTime.UtcNow.AddDays(-20)
            };

            var ticketClosedStuck = new RmaTicket
            {
                Id = "t-closed-stuck",
                StatusId = "status-closed",
                WarningColor = "Red",
                IsUrgent = true,
                SentDate = DateTime.UtcNow.AddDays(-20)
            };

            var ticketOpenOverdue = new RmaTicket
            {
                Id = "t-open-overdue",
                StatusId = "WaitingVendor",
                CustomerId = "cust-1",
                WarningColor = null,
                IsUrgent = false,
                ReceivedDate = DateTime.UtcNow.AddDays(-16),
                SentDate = DateTime.UtcNow.AddDays(-15) // Over 14 days -> Red
            };

            var ticketOpenSafe = new RmaTicket
            {
                Id = "t-open-safe",
                StatusId = "WaitingVendor",
                CustomerId = "cust-1",
                WarningColor = "Green",
                IsUrgent = false,
                ReceivedDate = DateTime.UtcNow.AddDays(-6),
                SentDate = DateTime.UtcNow.AddDays(-5) // Under 10 days -> Green (unchanged)
            };

            var ticketsList = new List<RmaTicket>
            {
                ticketClosedClean,
                ticketClosedStuck,
                ticketOpenOverdue,
                ticketOpenSafe
            };

            // Setup repository calls
            mockStatusRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(statuses);
            mockCustomerRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);

            // Capture objects updated in UpdateAsync calls
            RmaTicket? capturedClosedStuckTicket = null;
            RmaTicket? capturedOpenOverdueTicket = null;
            int closedCleanUpdateCount = 0;
            int openSafeUpdateCount = 0;

            mockTicketRepo.Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<RmaTicket>()))
                .Callback<string, RmaTicket>((id, t) =>
                {
                    Console.WriteLine($"[CALLBACK] UpdateAsync called: id={id}, ticket.Id={t?.Id}, WarningColor={t?.WarningColor}, IsUrgent={t?.IsUrgent}");
                    if (id == "t-closed-clean") closedCleanUpdateCount++;
                    if (id == "t-open-safe") openSafeUpdateCount++;
                    if (id == "t-closed-stuck")
                    {
                        capturedClosedStuckTicket = new RmaTicket
                        {
                            Id = t.Id,
                            StatusId = t.StatusId,
                            WarningColor = t.WarningColor,
                            IsUrgent = t.IsUrgent
                        };
                    }
                    if (id == "t-open-overdue")
                    {
                        capturedOpenOverdueTicket = new RmaTicket
                        {
                            Id = t.Id,
                            StatusId = t.StatusId,
                            WarningColor = t.WarningColor,
                            IsUrgent = t.IsUrgent
                        };
                    }
                })
                .Returns(Task.CompletedTask);

            // Use CancellationTokenSource to stop background service loop after the first read
            var cts = new CancellationTokenSource();
            mockTicketRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(() =>
            {
                // Signal cancellation so the while-loop exits immediately after this iteration
                cts.Cancel();
                return ticketsList;
            });

            // Mocking IServiceScopeFactory hierarchy
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);
            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);

            var mockSettingRepo = new Mock<FirestoreRepository<SystemSetting>>(null!, "system_settings");
            mockSettingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<SystemSetting>());

            mockServiceProvider.Setup(p => p.GetService(typeof(FirestoreRepository<RmaTicket>))).Returns(mockTicketRepo.Object);
            mockServiceProvider.Setup(p => p.GetService(typeof(FirestoreRepository<StatusMaster>))).Returns(mockStatusRepo.Object);
            mockServiceProvider.Setup(p => p.GetService(typeof(FirestoreRepository<Customer>))).Returns(mockCustomerRepo.Object);
            mockServiceProvider.Setup(p => p.GetService(typeof(FirestoreRepository<SystemSetting>))).Returns(mockSettingRepo.Object);

            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var service = new RmaAlertBackgroundService(
                mockFcmService.Object,
                mockConfig.Object,
                mockLogger.Object,
                mockScopeFactory.Object,
                memoryCache
            );

            // Act
            // Run the background service StartAsync, which triggers ExecuteAsync
            await service.StartAsync(cts.Token);
            if (service.ExecuteTask != null)
            {
                try
                {
                    await service.ExecuteTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when token is cancelled
                }
            }

            // Assert
            // 1. ticketClosedClean should NOT trigger UpdateAsync
            Assert.Equal(0, closedCleanUpdateCount);

            // 2. ticketClosedStuck should trigger UpdateAsync to clean WarningColor to null and IsUrgent to false
            Assert.NotNull(capturedClosedStuckTicket);
            Assert.Null(capturedClosedStuckTicket.WarningColor);
            Assert.False(capturedClosedStuckTicket.IsUrgent);

            // 3. ticketOpenOverdue should trigger UpdateAsync to set WarningColor to Red and IsUrgent to true
            Assert.NotNull(capturedOpenOverdueTicket);
            Assert.Equal("Red", capturedOpenOverdueTicket.WarningColor);
            Assert.True(capturedOpenOverdueTicket.IsUrgent);

            // 4. ticketOpenOverdue should trigger FCM SendAlertAsync
            mockFcmService.Verify(f => f.SendAlertAsync("t-open-overdue", "Cong ty Song Linh", It.Is<string>(r => r.Contains("Quá hạn 14 ngày"))), Times.Once);

            // 5. ticketOpenSafe should NOT trigger UpdateAsync
            Assert.Equal(0, openSafeUpdateCount);
        }
    }
}
