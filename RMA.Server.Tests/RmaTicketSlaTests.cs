using RMA.Server.Services;
using System;
using Xunit;

namespace RMA.Server.Tests
{
    public class RmaTicketSlaTests
    {
        [Fact]
        public void CalculateSla_Sent8DaysAgo_ReturnsGreenAndNotUrgent()
        {
            // Arrange
            var utcNow = DateTime.UtcNow;
            var sentDate = utcNow.AddDays(-8);

            // Act
            var (warningColor, shouldSetUrgent) = SlaCalculator.Calculate(sentDate, utcNow);

            // Assert
            Assert.Equal("Green", warningColor);
            Assert.False(shouldSetUrgent);
        }

        [Fact]
        public void CalculateSla_Sent12DaysAgo_ReturnsYellowAndNotUrgent()
        {
            // Arrange
            var utcNow = DateTime.UtcNow;
            var sentDate = utcNow.AddDays(-12);

            // Act
            var (warningColor, shouldSetUrgent) = SlaCalculator.Calculate(sentDate, utcNow);

            // Assert
            Assert.Equal("Yellow", warningColor);
            Assert.False(shouldSetUrgent);
        }

        [Fact]
        public void CalculateSla_Sent15DaysAgo_ReturnsRedAndUrgent()
        {
            // Arrange
            var utcNow = DateTime.UtcNow;
            var sentDate = utcNow.AddDays(-15);

            // Act
            var (warningColor, shouldSetUrgent) = SlaCalculator.Calculate(sentDate, utcNow);

            // Assert
            Assert.Equal("Red", warningColor);
            Assert.True(shouldSetUrgent);
        }
    }
}
