using ClientiX.Application.Interfaces;
using ClientiX.Domain.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ClientiX.Infrastructure.Services
{
    public class PdfReportService : IPdfReportService
    {
        public byte[] GenerateMastersReportAsync(IEnumerable<MasterUser> masters)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);

                    page.Header().Text("📊 ClientiX - Отчет по мастерам")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        column.Spacing(5);

                        column.Item().Text($"Отчет на {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .Italic().FontSize(10).AlignCenter();

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(150);
                                columns.ConstantColumn(120);
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Telegram ID").Bold();
                                header.Cell().Text("Статус").Bold();
                                header.Cell().Text("Подписка").Bold();
                                header.Cell().Text("Бот").Bold();
                            });

                            foreach (var master in masters)
                            {
                                table.Cell().Text(master.TelegramUserId.ToString());
                                table.Cell().Text(master.IsAdmin ? "👑 Админ" : "👤 Мастер");
                                table.Cell().Text(master.Subscription?.IsActive == true
                                    ? $"До {master.Subscription.ExpiresAt:dd.MM}" : "❌ Нет");
                                table.Cell().Text(master.ClientBotId?.ToString() ?? "❌");
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
