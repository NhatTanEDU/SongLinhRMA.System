using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using RMA.Server.Controllers;
using RMA.Server.Entities;
using RMA.Server.Services;
using RMA.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RMA.Server.Tests
{
    public class RmaTicketsControllerTests
    {
        private readonly Mock<FirestoreRepository<RmaTicket>> _mockTicketRepo;
        private readonly Mock<FirestoreRepository<Device>> _mockDeviceRepo;
        private readonly Mock<FirestoreRepository<Customer>> _mockCustomerRepo;
        private readonly Mock<FirestoreRepository<StatusMaster>> _mockStatusRepo;
        private readonly Mock<FirestoreRepository<Vendor>> _mockVendorRepo;
        private readonly Mock<FirestoreRepository<Model>> _mockModelRepo;
        private readonly Mock<FirestoreRepository<Attachment>> _mockAttachmentRepo;
        private readonly Mock<FirestoreRepository<StatusHistory>> _mockStatusHistoryRepo;
        private readonly Mock<FirestoreRepository<Location>> _mockLocationRepo;
        private readonly Mock<IPdfService> _mockPdfService;
        private readonly RmaTicketsController _controller;

        public RmaTicketsControllerTests()
        {
            _mockTicketRepo = new Mock<FirestoreRepository<RmaTicket>>(null!, "rma_tickets", null);
            _mockDeviceRepo = new Mock<FirestoreRepository<Device>>(null!, "devices", null);
            _mockCustomerRepo = new Mock<FirestoreRepository<Customer>>(null!, "customers", null);
            _mockStatusRepo = new Mock<FirestoreRepository<StatusMaster>>(null!, "status_masters", null);
            _mockVendorRepo = new Mock<FirestoreRepository<Vendor>>(null!, "vendors", null);
            _mockModelRepo = new Mock<FirestoreRepository<Model>>(null!, "models", null);
            _mockAttachmentRepo = new Mock<FirestoreRepository<Attachment>>(null!, "attachments", null);
            _mockStatusHistoryRepo = new Mock<FirestoreRepository<StatusHistory>>(null!, "status_histories", null);
            _mockLocationRepo = new Mock<FirestoreRepository<Location>>(null!, "locations", null);
            _mockPdfService = new Mock<IPdfService>();

            SetupEmptyRepos();

            _controller = new RmaTicketsController(
                _mockTicketRepo.Object,
                _mockDeviceRepo.Object,
                _mockCustomerRepo.Object,
                _mockStatusRepo.Object,
                _mockVendorRepo.Object,
                _mockModelRepo.Object,
                _mockAttachmentRepo.Object,
                _mockStatusHistoryRepo.Object,
                _mockLocationRepo.Object,
                _mockPdfService.Object
            );
        }

        private void SetupEmptyRepos()
        {
            _mockTicketRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RmaTicket>());
            _mockDeviceRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Device>());
            _mockCustomerRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Customer>());
            _mockStatusRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<StatusMaster>());
            _mockVendorRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Vendor>());
            _mockModelRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Model>());
            _mockAttachmentRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Attachment>());
            _mockStatusHistoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<StatusHistory>());
            _mockLocationRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Location>());
        }

        [Fact]
        public async Task Get_ReturnsMappedDtos_WithInMemoryJoins()
        {
            // Arrange
            var ticket = new RmaTicket
            {
                Id = "ticket-1",
                CustomerId = "customer-1",
                DeviceId = "device-1",
                StatusId = "status-1",
                VendorId = "vendor-1",
                ProblemDescription = "Display broken",
                ReceivedDate = DateTime.UtcNow
            };

            var customer = new Customer { Id = "customer-1", Name = "Cong ty Song Linh" };
            var device = new Device { Id = "device-1", SerialNumber = "SN12345", ModelId = "model-1" };
            var model = new Model { Id = "model-1", ModelName = "Dell XPS 13" };
            var status = new StatusMaster { Id = "status-1", StatusName = "In Progress" };
            var vendor = new Vendor { Id = "vendor-1", Name = "Dell Asia" };

            _mockTicketRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<RmaTicket> { ticket });
            _mockCustomerRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Customer> { customer });
            _mockDeviceRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Device> { device });
            _mockModelRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Model> { model });
            _mockStatusRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<StatusMaster> { status });
            _mockVendorRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Vendor> { vendor });

            // Act
            var actionResult = await _controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<RmaTicketDto>>(okResult.Value).ToList();

            Assert.Single(dtos);
            var dto = dtos.First();
            Assert.Equal("ticket-1", dto.Id);
            Assert.Equal("Cong ty Song Linh", dto.CustomerName);
            Assert.Equal("SN12345", dto.DeviceSerialNumber);
            Assert.Equal("Dell XPS 13", dto.DeviceModelName);
            Assert.Equal("In Progress", dto.StatusName);
            Assert.Equal("Dell Asia", dto.VendorName);
        }

        [Fact]
        public async Task GetPaged_AppliesFiltersAndPagination_Correctly()
        {
            // Arrange
            // Create 15 tickets:
            // - 12 are Red in June (Month = 6)
            // - 1 is Green in June
            // - 2 are Red in July (Month = 7)
            var tickets = new List<RmaTicket>();
            
            // 12 Red tickets in June
            for (int i = 1; i <= 12; i++)
            {
                tickets.Add(new RmaTicket
                {
                    Id = $"t-june-red-{i}",
                    CustomerId = "c-1",
                    DeviceId = "d-1",
                    StatusId = "s-1",
                    ReceivedDate = new DateTime(2026, 6, i, 12, 0, 0, DateTimeKind.Utc),
                    WarningColor = "Red"
                });
            }

            // 1 Green ticket in June
            tickets.Add(new RmaTicket
            {
                Id = "t-june-green",
                CustomerId = "c-1",
                DeviceId = "d-1",
                StatusId = "s-1",
                ReceivedDate = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc),
                WarningColor = "Green"
            });

            // 2 Red tickets in July
            tickets.Add(new RmaTicket
            {
                Id = "t-july-red-1",
                CustomerId = "c-1",
                DeviceId = "d-1",
                StatusId = "s-1",
                ReceivedDate = new DateTime(2026, 7, 5, 12, 0, 0, DateTimeKind.Utc),
                WarningColor = "Red"
            });
            tickets.Add(new RmaTicket
            {
                Id = "t-july-red-2",
                CustomerId = "c-1",
                DeviceId = "d-1",
                StatusId = "s-1",
                ReceivedDate = new DateTime(2026, 7, 10, 12, 0, 0, DateTimeKind.Utc),
                WarningColor = "Red"
            });

            _mockTicketRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(tickets);

            var request = new TicketPagedRequestDto
            {
                Month = 6,
                WarningColor = "Red",
                PageSize = 10,
                PageNumber = 1
            };

            // Act
            var actionResult = await _controller.GetPaged(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<RmaTicketDto>>(okResult.Value).ToList();

            // Should return maximum of PageSize (10)
            Assert.Equal(10, dtos.Count);

            // All returned items must match filter criteria (June, Red)
            foreach (var dto in dtos)
            {
                Assert.Equal(6, dto.ReceivedDate.Month);
                Assert.Equal("Red", dto.WarningColor);
            }
        }
    }
}
