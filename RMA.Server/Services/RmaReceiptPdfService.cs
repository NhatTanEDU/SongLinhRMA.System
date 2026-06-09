using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RMA.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RMA.Server.Services;

public class RmaReceiptPdfService : IPdfService
{
    static RmaReceiptPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateRmaReceiptPdf(RmaTicketDto ticket)
    {
        // Parse checklists from StaffNote if empty (as the system serializes accessories into StaffNote)
        PopulateChecklistsFromStaffNote(ticket);

        using var stream = new MemoryStream();
        
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                
                // Use a standard font like Arial which handles Vietnamese diacritics well
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10f).FontColor(Colors.Grey.Darken4));

                page.Header().Element(headerContainer => ComposeHeader(headerContainer, ticket));
                page.Content().Element(contentContainer => ComposeContent(contentContainer, ticket));
                page.Footer().Element(footerContainer => ComposeFooter(footerContainer));
            });
        }).GeneratePdf(stream);

        return stream.ToArray();
    }

    private void PopulateChecklistsFromStaffNote(RmaTicketDto ticket)
    {
        ticket.PopulateChecklistsFromStaffNote();
    }

    private void ComposeHeader(IContainer container, RmaTicketDto ticket)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                // Left: Company Info
                row.RelativeItem(2).Column(c =>
                {
                    c.Item().Text("SONG LINH RMA SYSTEM").Bold().FontSize(13).FontColor(Colors.Blue.Darken3);
                    c.Item().Text("CÔNG TY TNHH DỊCH VỤ VÀ THƯƠNG MẠI SONG LINH").Bold().FontSize(9f).FontColor(Colors.Grey.Darken2);
                    c.Item().Text("Địa chỉ: Số 12 Ba Cu, Phường 1, TP. Vũng Tàu").FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                    c.Item().Text("Hotline: 0909.123.456 | Email: contact@songlinh.vn").FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                });

                // Right: Document Info
                row.RelativeItem(1).AlignRight().Column(c =>
                {
                    c.Item().Text("PHIẾU BIÊN NHẬN").Bold().FontSize(15).FontColor(Colors.Blue.Darken4);
                    c.Item().Text($"Mã RMA: #{ticket.Id.Substring(0, Math.Min(8, ticket.Id.Length)).ToUpper()}").Bold().FontSize(11).FontColor(Colors.Red.Medium);
                    c.Item().Text($"Ngày nhận: {ticket.ReceivedDate.AddHours(7).ToString("dd/MM/yyyy HH:mm")}").FontSize(9).FontColor(Colors.Grey.Darken2);
                });
            });

            col.Item().PaddingTop(10).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken3);
        });
    }

    private void ComposeContent(IContainer container, RmaTicketDto ticket)
    {
        container.PaddingTop(15).Column(col =>
        {
            // Section 1: Customer Information
            col.Item().Text("1. THÔNG TIN KHÁCH HÀNG").Bold().FontSize(11).FontColor(Colors.Blue.Darken3);
            col.Item().PaddingTop(3).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });

                // Row 1
                table.Cell().Text("Khách hàng:").Bold();
                table.Cell().ColumnSpan(3).Text(ticket.CustomerName ?? "-");

                // Row 2
                table.Cell().Text("Đại diện:").Bold();
                table.Cell().Text(ticket.CustomerContactPerson ?? "-");
                table.Cell().Text("Điện thoại:").Bold();
                table.Cell().Text(ticket.CustomerPhone ?? "-");

                // Row 3
                table.Cell().Text("P. Ban/User:").Bold();
                table.Cell().ColumnSpan(3).Text(ticket.EndUserName ?? "-");
            });

            col.Item().PaddingTop(15);

            // Section 2: Device & Problem Information
            col.Item().Text("2. THÔNG TIN THIẾT BỊ & TÌNH TRẠNG LỖI").Bold().FontSize(11).FontColor(Colors.Blue.Darken3);
            col.Item().PaddingTop(3).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });

                // Row 1
                table.Cell().Text("Thiết bị:").Bold();
                table.Cell().Text(ticket.DeviceModelName ?? "-");
                table.Cell().Text("Số Serial (S/N):").Bold();
                table.Cell().Text(ticket.DeviceSerialNumber ?? "-");

                // Row 2
                table.Cell().Text("Dịch vụ:").Bold();
                table.Cell().Text(ticket.ServiceMode ?? "-");
                table.Cell().Text("Ưu tiên:").Bold();
                table.Cell().Text(ticket.IsUrgent ? "GẤP (Hỏa tốc)" : "Thường");

                // Row 3
                table.Cell().Text("Mô tả lỗi:").Bold();
                table.Cell().ColumnSpan(3).Text(ticket.ProblemDescription ?? "-");
            });

            col.Item().PaddingTop(15);

            // Section 3: Accessory Checklist
            col.Item().Text("3. PHỤ KIỆN KÈM THEO").Bold().FontSize(11).FontColor(Colors.Blue.Darken3);
            col.Item().PaddingTop(3).Column(chkCol =>
            {
                if (ticket.Checklists != null && ticket.Checklists.Any())
                {
                    chkCol.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                        });

                        // Table Header
                        table.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("STT").Bold().FontColor(Colors.White);
                        table.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Tên phụ kiện / Thiết bị kèm theo").Bold().FontColor(Colors.White);
                        table.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("Trạng thái nhận").Bold().FontColor(Colors.White);

                        int index = 1;
                        foreach (var item in ticket.Checklists)
                        {
                            var bgColor = index % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                            
                            table.Cell().Background(bgColor).Padding(5).AlignCenter().Text(index.ToString());
                            table.Cell().Background(bgColor).Padding(5).Text(item.ItemName);
                            
                            var statusText = item.IsChecked ? "[ ✓ ] Có nhận" : "[   ] Không";
                            var statusColor = item.IsChecked ? Colors.Green.Darken2 : Colors.Grey.Darken1;
                            
                            var cellText = table.Cell().Background(bgColor).Padding(5).AlignCenter().Text(statusText).FontColor(statusColor);
                            if (item.IsChecked)
                            {
                                cellText.Bold();
                            }
                            index++;
                        }
                    });
                }
                else
                {
                    chkCol.Item().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(8).AlignCenter().Text("Không ghi nhận phụ kiện đi kèm.").Italic().FontColor(Colors.Grey.Darken1);
                }
            });

            col.Item().PaddingTop(25);

            // Section 4: Signature Blocks
            col.Item().Row(row =>
            {
                row.RelativeItem().AlignCenter().Column(sigCol =>
                {
                    sigCol.Item().Text("ĐẠI DIỆN KHÁCH HÀNG").Bold().FontSize(10);
                    sigCol.Item().Text("(Ký và ghi rõ họ tên)").Italic().FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                    sigCol.Item().PaddingTop(50).Text("....................................................").FontColor(Colors.Grey.Lighten1);
                });

                row.RelativeItem().AlignCenter().Column(sigCol =>
                {
                    sigCol.Item().Text("NHÂN VIÊN NHẬN MÁY").Bold().FontSize(10);
                    sigCol.Item().Text("(Ký và ghi rõ họ tên)").Italic().FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                    sigCol.Item().PaddingTop(50).Text("....................................................").FontColor(Colors.Grey.Lighten1);
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
            col.Item().PaddingTop(5).AlignCenter().Text("Vui lòng mang theo phiếu này khi đến nhận lại thiết bị. Cảm ơn quý khách!").Italic().FontSize(8.5f).FontColor(Colors.Grey.Darken1);
        });
    }

    public byte[] GenerateHandoverPdf(RmaTicketDto ticket, TicketType ticketType, List<HandoverItemDto> items)
    {
        var document = new PtscMcPdfTemplate(ticket, ticketType, items);
        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }
}
