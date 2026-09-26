using System;
using System.Collections.Generic;
using System.Linq;
using DarV2.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DarV2.Service.Export
{
    public static class SchedulePdfGenerator
    {
        static SchedulePdfGenerator()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static byte[] GenerateWeeklySchedulePdf(IEnumerable<WeeklyScheduleItemDTO> items, string? filterTeacherName = null)
        {
            var dayNames = new Dictionary<int, string>
            {
                { 6, "ÙŠÙˆÙ… Ø§Ù„Ø³Ø¨Øª" },
                { 0, "ÙŠÙˆÙ… Ø§Ù„Ø£Ø­Ø¯" },
                { 1, "ÙŠÙˆÙ… Ø§Ù„Ø¥Ø«Ù†ÙŠÙ†" },
                { 2, "ÙŠÙˆÙ… Ø§Ù„Ø«Ù„Ø§Ø«Ø§Ø¡" },
                { 3, "ÙŠÙˆÙ… Ø§Ù„Ø£Ø±Ø¨Ø¹Ø§Ø¡" },
                { 4, "ÙŠÙˆÙ… Ø§Ù„Ø®Ù…ÙŠØ³" },
                { 5, "ÙŠÙˆÙ… Ø§Ù„Ø¬Ù…Ø¹Ø©" }
            };

            var orderedDays = new[] { 6, 0, 1, 2, 3, 4, 5 };

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.2f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));
                    page.ContentFromRightToLeft();

                    page.Header().Element(header =>
                    {
                        header.PaddingBottom(12).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Ø¯Ø§Ø± Ø£ÙˆÙ„Ø§Ø¯ Ø³Ù†Ø¯ Ø§Ù„ØªØ¹Ù„ÙŠÙ…ÙŠØ©").FontSize(18).Bold().FontColor("#0F172A");
                                col.Item().Text("Ø¬Ø¯ÙˆÙ„ Ø§Ù„Ø­ØµØµ ÙˆØ§Ù„Ù…ÙˆØ§Ø¹ÙŠØ¯ Ø§Ù„Ø£Ø³Ø¨ÙˆØ¹ÙŠØ©").FontSize(13).SemiBold().FontColor("#334155");
                                if (!string.IsNullOrWhiteSpace(filterTeacherName))
                                {
                                    col.Item().PaddingTop(2).Text($"Ø§Ù„Ù…Ø¹Ù„Ù…: {filterTeacherName}").FontSize(11).Bold().FontColor("#2563EB");
                                }
                            });

                            row.ConstantItem(200).AlignLeft().Column(col =>
                            {
                                col.Item().Text($"ØªØ§Ø±ÙŠØ® Ø§Ù„ØªØµØ¯ÙŠØ±: {DateTime.Now:yyyy/MM/dd}").FontSize(9).FontColor("#64748B");
                                col.Item().Text($"Ø¥Ø¬Ù…Ø§Ù„ÙŠ Ø§Ù„Ù…ÙˆØ§Ø¹ÙŠØ¯: {items.Count()}").FontSize(9).FontColor("#64748B");
                            });
                        });
                    });

                    page.Content().Column(column =>
                    {
                        column.Spacing(14);

                        bool hasAny = false;

                        foreach (var dayVal in orderedDays)
                        {
                            var dayItems = items.Where(x => x.DayOfWeek == dayVal).OrderBy(x => x.StartTime).ToList();
                            if (!dayItems.Any()) continue;

                            hasAny = true;

                            column.Item().Column(dayCol =>
                            {
                                dayCol.Spacing(4);

                                // Day Header Badge
                                dayCol.Item().Background("#1E293B").Padding(6).Row(r =>
                                {
                                    r.RelativeItem().Text(dayNames[dayVal]).FontSize(11).Bold().FontColor("#FFFFFF");
                                    r.ConstantItem(100).AlignLeft().Text($"{dayItems.Count} Ø­ØµØµ").FontSize(10).FontColor("#94A3B8");
                                });

                                dayCol.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);   // Ø§Ø³Ù… Ø§Ù„Ù…Ø¬Ù…ÙˆØ¹Ø©
                                        columns.RelativeColumn(3);   // Ø§Ù„Ù…Ø¹Ù„Ù…
                                        columns.RelativeColumn(2.5f); // Ø§Ù„ÙˆÙ‚Øª
                                        columns.RelativeColumn(2);   // Ø§Ù„Ù‚Ø§Ø¹Ø© / Ø£ÙˆÙ†Ù„Ø§ÙŠÙ†
                                    });

                                    table.Header(h =>
                                    {
                                        h.Cell().Background("#F1F5F9").Padding(6).Text("Ø§Ø³Ù… Ø§Ù„Ù…Ø¬Ù…ÙˆØ¹Ø©").Bold().FontColor("#334155").FontSize(9);
                                        h.Cell().Background("#F1F5F9").Padding(6).Text("Ø§Ù„Ù…Ø¹Ù„Ù…").Bold().FontColor("#334155").FontSize(9);
                                        h.Cell().Background("#F1F5F9").Padding(6).Text("Ø§Ù„ÙˆÙ‚Øª").Bold().FontColor("#334155").FontSize(9);
                                        h.Cell().Background("#F1F5F9").Padding(6).Text("Ø§Ù„Ù…ÙƒØ§Ù† / Ø§Ù„Ù†Ø¸Ø§Ù…").Bold().FontColor("#334155").FontSize(9);
                                    });

                                    bool isEven = false;
                                    foreach (var item in dayItems)
                                    {
                                        var bg = isEven ? "#F8FAFC" : "#FFFFFF";
                                        table.Cell().Background(bg).BorderBottom(1).BorderColor("#E2E8F0").Padding(6).Text(item.GroupName ?? "-").FontSize(9);
                                        table.Cell().Background(bg).BorderBottom(1).BorderColor("#E2E8F0").Padding(6).Text(item.TeacherName ?? "ØºÙŠØ± Ù…Ø­Ø¯Ø¯").FontSize(9);
                                        table.Cell().Background(bg).BorderBottom(1).BorderColor("#E2E8F0").Padding(6).Text($"{FormatTimeSpan(item.StartTime)} - {FormatTimeSpan(item.EndTime)}").FontSize(9);
                                        table.Cell().Background(bg).BorderBottom(1).BorderColor("#E2E8F0").Padding(6).Text(item.IsOnline ? "Ø£ÙˆÙ†Ù„Ø§ÙŠÙ†" : (string.IsNullOrWhiteSpace(item.RoomName) ? "Ø­Ø¶ÙˆØ±ÙŠ" : item.RoomName)).FontSize(9);
                                        isEven = !isEven;
                                    }
                                });
                            });
                        }

                        if (!hasAny)
                        {
                            column.Item().Padding(30).AlignCenter().Text("Ù„Ø§ ØªÙˆØ¬Ø¯ Ù…ÙˆØ§Ø¹ÙŠØ¯ Ù…Ø¶Ø§ÙØ© ÙÙŠ Ø§Ù„Ø¬Ø¯ÙˆÙ„.").FontSize(12).FontColor("#64748B");
                        }
                    });

                    page.Footer().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Text("Ø¯Ø§Ø± Ø£ÙˆÙ„Ø§Ø¯ Ø³Ù†Ø¯ Ø§Ù„ØªØ¹Ù„ÙŠÙ…ÙŠØ© - Ù†Ø¸Ø§Ù… Ø¥Ø¯Ø§Ø±Ø© Ø§Ù„Ù…ÙˆØ§Ø¹ÙŠØ¯").FontSize(8).FontColor("#94A3B8");
                        row.ConstantItem(100).AlignLeft().Text(x =>
                        {
                            x.Span("ØµÙØ­Ø© ");
                            x.CurrentPageNumber();
                            x.Span(" Ù…Ù† ");
                            x.TotalPages();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static string FormatTimeSpan(TimeSpan ts)
        {
            int hours = ts.Hours;
            int minutes = ts.Minutes;
            string period = hours >= 12 ? "Ù…" : "Øµ";
            int displayHour = hours % 12;
            if (displayHour == 0) displayHour = 12;
            return $"{displayHour:D2}:{minutes:D2} {period}";
        }
    }
}
