using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RMA.Shared.DTOs;
using System;
using System.Collections.Generic;

namespace RMA.Server.Services;

public class PtscMcPdfTemplate : IDocument
{
    private readonly RmaTicketDto _ticket;
    private readonly TicketType _ticketType;
    private readonly List<HandoverItemDto> _items;

    public PtscMcPdfTemplate(RmaTicketDto ticket, TicketType ticketType, List<HandoverItemDto> items)
    {
        _ticket = ticket;
        _ticketType = ticketType;
        _items = items;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.PageColor(Colors.White);
            
            // Use Arial for Vietnamese diacritics
            page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10f).FontColor(Colors.Grey.Darken4));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                // Left Header: Song Linh Company
                row.RelativeItem(1).Column(c =>
                {
                    c.Item().Text("SONG LINH RMA SYSTEM").Bold().FontSize(10).FontColor(Colors.Blue.Darken3);
                    c.Item().Text("CÔNG TY TNHH DỊCH VỤ VÀ THƯƠNG MẠI SONG LINH").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text("Số 12 Ba Cu, Phường 1, TP. Vũng Tàu").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                });

                // Right Header: PTSC M&C
                row.RelativeItem(1).AlignRight().Column(c =>
                {
                    c.Item().Text("PTSC M&C").Bold().FontSize(10).FontColor(Colors.Blue.Darken4);
                    c.Item().Text("CÔNG TY CỔ PHẦN DỊCH VỤ LẮP ĐẶT, VẬN HÀNH").Bold().FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                    c.Item().Text("VÀ SỬA CHỮA CÔNG TRÌNH DẦU KHÍ BIỂN PTSC").Bold().FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                });
            });

            col.Item().PaddingTop(8).LineHorizontal(1f).LineColor(Colors.Grey.Lighten1);
            
            // Title
            col.Item().PaddingTop(15).AlignCenter().Column(c =>
            {
                c.Item().Text("BIÊN BẢN BÀN GIAO THIẾT BỊ").Bold().FontSize(15).FontColor(Colors.Blue.Darken4);
                c.Item().Text($"(Mã RMA: #{_ticket.Id.Substring(0, Math.Min(8, _ticket.Id.Length)).ToUpper()})").Italic().FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(15).Column(col =>
        {
            // Section 1: Customer Information
            col.Item().Text("I. THÔNG TIN KHÁCH HÀNG & BÀN GIAO").Bold().FontSize(11).FontColor(Colors.Blue.Darken3);
            col.Item().PaddingTop(3).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });

                table.Cell().Text("Đơn vị nhận:").Bold();
                table.Cell().ColumnSpan(3).Text(_ticket.CustomerName ?? "-");

                table.Cell().Text("Đại diện:").Bold();
                table.Cell().Text(_ticket.CustomerContactPerson ?? "-");
                table.Cell().Text("Số ĐT:").Bold();
                table.Cell().Text(_ticket.CustomerPhone ?? "-");

                table.Cell().Text("Bộ phận/User:").Bold();
                table.Cell().ColumnSpan(3).Text(_ticket.EndUserName ?? "-");

                table.Cell().Text("Hình thức dịch vụ:").Bold();
                table.Cell().Text(_ticket.ServiceMode ?? "-");
                table.Cell().Text("Ngày bàn giao:").Bold();
                table.Cell().Text(DateTime.Now.ToString("dd/MM/yyyy"));
            });

            col.Item().PaddingTop(15);

            // Section 2: Equipment list
            col.Item().Text("II. DANH SÁCH THIẾT BỊ BÀN GIAO").Bold().FontSize(11).FontColor(Colors.Blue.Darken3);
            col.Item().PaddingTop(3).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40); // STT
                    columns.RelativeColumn(4);  // Tên thiết bị & S/N
                    columns.ConstantColumn(60); // Số lượng
                    columns.ConstantColumn(60); // Đơn vị tính
                    columns.RelativeColumn(2);  // Ghi chú (Bảo hành / Sửa chữa)
                });

                // Table Header
                table.Cell().Background(Colors.Blue.Darken2).Padding(5).Border(0.5f).BorderColor(Colors.Grey.Lighten1).AlignCenter().Text("STT").Bold().FontColor(Colors.White).FontSize(9);
                table.Cell().Background(Colors.Blue.Darken2).Padding(5).Border(0.5f).BorderColor(Colors.Grey.Lighten1).Text("Tên thiết bị / Model").Bold().FontColor(Colors.White).FontSize(9);
                table.Cell().Background(Colors.Blue.Darken2).Padding(5).Border(0.5f).BorderColor(Colors.Grey.Lighten1).AlignCenter().Text("SL").Bold().FontColor(Colors.White).FontSize(9);
                table.Cell().Background(Colors.Blue.Darken2).Padding(5).Border(0.5f).BorderColor(Colors.Grey.Lighten1).AlignCenter().Text("ĐVT").Bold().FontColor(Colors.White).FontSize(9);
                table.Cell().Background(Colors.Blue.Darken2).Padding(5).Border(0.5f).BorderColor(Colors.Grey.Lighten1).AlignCenter().Text("Ghi chú (Dịch vụ)").Bold().FontColor(Colors.White).FontSize(9);

                int index = 1;
                foreach (var item in _items)
                {
                    var bgColor = index % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                    
                    table.Cell().Background(bgColor).Padding(5).Border(0.5f).BorderColor(Colors.Grey.Lighten2).AlignCenter().Text(index.ToString()).FontSize(9);
                    
                    // Column Tên thiết bị ở trên, S/N ở dưới (font size 9, in nghiêng)
                    table.Cell().Background(bgColor).Padding(5).Border(0.5f).BorderColor(Colors.Grey.Lighten2).Column(cellCol =>
                    {
                        cellCol.Item().Text(item.DeviceName).Bold().FontSize(9);
                        cellCol.Item().Text($"S/N: {item.SerialNumber}").Italic().FontSize(9).FontColor(Colors.Grey.Darken1);
                    });

                    table.Cell().Background(bgColor).Padding(5).Border(0.5f).BorderColor(Colors.Grey.Lighten2).AlignCenter().Text(item.Quantity.ToString()).FontSize(9);
                    table.Cell().Background(bgColor).Padding(5).Border(0.5f).BorderColor(Colors.Grey.Lighten2).AlignCenter().Text(item.Unit).FontSize(9);

                    // Note column based on TicketType
                    string noteText = _ticketType == TicketType.BaoHanh ? "Bảo hành" : "Sửa chữa";
                    table.Cell().Background(bgColor).Padding(5).Border(0.5f).BorderColor(Colors.Grey.Lighten2).AlignCenter().Text(noteText).FontSize(9);

                    index++;
                }
            });

            col.Item().PaddingTop(25);

            // Section 3: Signature Blocks
            col.Item().Row(row =>
            {
                row.RelativeItem().AlignCenter().Column(sigCol =>
                {
                    sigCol.Item().Text("ĐẠI DIỆN KHÁCH HÀNG (PTSC M&C)").Bold().FontSize(9.5f);
                    sigCol.Item().Text("(Ký và ghi rõ họ tên)").Italic().FontSize(8f).FontColor(Colors.Grey.Darken1);
                    sigCol.Item().PaddingTop(50).Text("....................................................").FontColor(Colors.Grey.Lighten1);
                });

                row.RelativeItem().AlignCenter().Column(sigCol =>
                {
                    sigCol.Item().Text("ĐẠI DIỆN SONG LINH RMA").Bold().FontSize(9.5f);
                    sigCol.Item().Text("(Ký và ghi rõ họ tên)").Italic().FontSize(8f).FontColor(Colors.Grey.Darken1);
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
            col.Item().PaddingTop(5).AlignCenter().Text("Biên bản được lập thành 02 bản, mỗi bên giữ 01 bản có giá trị pháp lý như nhau.").Italic().FontSize(8).FontColor(Colors.Grey.Darken1);
        });
    }
}
