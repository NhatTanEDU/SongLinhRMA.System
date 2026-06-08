using RMA.Server.Services;
using RMA.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace RMA.Server.Tests;

public class RmaReceiptPdfServiceTests
{
    [Fact]
    public void GenerateRmaReceiptPdf_ReturnsValidPdfBytes()
    {
        // Arrange
        var service = new RmaReceiptPdfService();
        var ticket = new RmaTicketDto
        {
            Id = "test-ticket-id-12345678",
            CustomerName = "Công ty Cổ phần PTSC",
            CustomerContactPerson = "Nguyễn Văn A",
            CustomerPhone = "0909123456",
            EndUserName = "Phòng Kỹ thuật",
            DeviceModelName = "Dell Latitude 5420",
            DeviceSerialNumber = "SNDELL12345",
            ServiceMode = "Sửa chữa",
            ProblemDescription = "Mất nguồn, không lên hình",
            IsUrgent = true,
            StaffNote = "[Vị trí: Tại Cty Song Linh] [BH: Còn BH] [Phụ kiện: Adapter/Sạc, Dây nguồn]"
        };

        // Act
        var pdfBytes = service.GenerateRmaReceiptPdf(ticket);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 0, "PDF bytes should not be empty");
        
        // A valid PDF file must start with "%PDF" header
        var pdfHeader = Encoding.UTF8.GetString(pdfBytes, 0, 4);
        Assert.Equal("%PDF", pdfHeader);
        
        // Check that the parsed checklist is populated in the DTO after execution
        Assert.NotNull(ticket.Checklists);
        Assert.NotEmpty(ticket.Checklists);
        var adapterCheck = ticket.Checklists.Find(c => c.ItemName == "Adapter/Sạc");
        Assert.NotNull(adapterCheck);
        Assert.True(adapterCheck.IsChecked);
    }
}