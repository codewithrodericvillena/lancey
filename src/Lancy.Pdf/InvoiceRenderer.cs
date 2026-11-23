using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lancy.Core.Entities;
using Lancy.Core.Interfaces;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Lancy.Pdf;

/// <summary>
/// Letter-size invoice for one commission: business header, bill-to, the job as a
/// line item, payments already received, and the balance due. Same PdfSharpCore
/// approach as <see cref="TaxSummaryRenderer"/>.
/// </summary>
public class InvoiceRenderer : IInvoiceRenderer
{
    private const double PageWidthPt = 612;
    private const double PageHeightPt = 792;
    private static readonly XColor Accent = XColor.FromArgb(99, 102, 241);

    public Task<byte[]> RenderPdfAsync(
        Commission commission, Client? client, IReadOnlyList<Payment> payments,
        AppSettings business, CancellationToken ct = default)
    {
        using var document = new PdfDocument();
        document.Info.Title = $"Invoice {IInvoiceRenderer.InvoiceNumber(commission)}";
        var page = document.AddPage();
        page.Width = XUnit.FromPoint(PageWidthPt);
        page.Height = XUnit.FromPoint(PageHeightPt);

        using var gfx = XGraphics.FromPdfPage(page);
        var bigFont = new XFont("Arial", 26, XFontStyle.Bold);
        var nameFont = new XFont("Arial", 13, XFontStyle.Bold);
        var bold = new XFont("Arial", 10, XFontStyle.Bold);
        var normal = new XFont("Arial", 10, XFontStyle.Regular);
        var small = new XFont("Arial", 9, XFontStyle.Regular);
        var totalFont = new XFont("Arial", 14, XFontStyle.Bold);
        var gray = new XSolidBrush(XColor.FromArgb(110, 115, 130));
        var accent = new XSolidBrush(Accent);

        const double margin = 54;
        const double width = PageWidthPt - margin * 2;
        double y = margin;
        var currency = string.IsNullOrWhiteSpace(business.CurrencySymbol) ? "₱" : business.CurrencySymbol;
        string Money(decimal v) => $"{currency}{v:N2}";

        // Accent bar + title
        gfx.DrawRectangle(accent, 0, 0, PageWidthPt, 10);
        gfx.DrawString("INVOICE", bigFont, accent, new XPoint(margin, y + 14));

        // Business block (right aligned)
        var bizName = !string.IsNullOrWhiteSpace(business.BusinessName) ? business.BusinessName : business.FreelancerName;
        double by = y;
        void BizLine(string? text, XFont font, XBrush brush)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            gfx.DrawString(text, font, brush, new XRect(margin, by, width, 14), XStringFormats.TopRight);
            by += 14;
        }
        BizLine(bizName, nameFont, XBrushes.Black);
        BizLine(business.Address, small, gray);
        BizLine(business.Email, small, gray);
        BizLine(business.ContactNumber, small, gray);
        BizLine(business.Website, small, gray);
        if (!string.IsNullOrWhiteSpace(business.Tin)) BizLine($"TIN: {business.Tin}", small, gray);

        y = Math.Max(y + 52, by + 16);

        // Meta (number / date / due) + Bill to
        gfx.DrawString("BILL TO", small, gray, new XPoint(margin, y));
        gfx.DrawString("INVOICE NO.", small, gray, new XPoint(margin + 330, y));
        y += 14;
        gfx.DrawString(client?.Name ?? "-", nameFont, XBrushes.Black, new XPoint(margin, y));
        gfx.DrawString(IInvoiceRenderer.InvoiceNumber(commission), bold, XBrushes.Black, new XPoint(margin + 330, y));
        y += 16;
        gfx.DrawString(client?.Email ?? "", small, gray, new XPoint(margin, y));
        gfx.DrawString("DATE", small, gray, new XPoint(margin + 330, y));
        y += 14;
        gfx.DrawString(client?.Phone ?? "", small, gray, new XPoint(margin, y));
        gfx.DrawString(DateTime.Now.ToString("MMM d, yyyy"), normal, XBrushes.Black, new XPoint(margin + 330, y));
        if (commission.Deadline is { } due)
        {
            y += 16;
            gfx.DrawString("DUE", small, gray, new XPoint(margin + 330, y));
            y += 14;
            gfx.DrawString(due.ToString("MMM d, yyyy"), normal, XBrushes.Black, new XPoint(margin + 330, y));
        }
        y += 34;

        // Line item table
        gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(243, 244, 250)), margin, y - 12, width, 22);
        gfx.DrawString("DESCRIPTION", bold, XBrushes.Black, new XPoint(margin + 8, y + 2));
        gfx.DrawString("AMOUNT", bold, XBrushes.Black, new XRect(margin, y - 10, width - 8, 18), XStringFormats.TopRight);
        y += 24;
        gfx.DrawString(commission.Title, bold, XBrushes.Black, new XPoint(margin + 8, y));
        gfx.DrawString(Money(commission.Price), normal, XBrushes.Black, new XRect(margin, y - 10, width - 8, 18), XStringFormats.TopRight);
        y += 14;
        var detail = string.Join(" - ", new[] { commission.ServiceType, commission.Description }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
        if (detail.Length > 0)
        {
            var tf = new PdfSharpCore.Drawing.Layout.XTextFormatter(gfx);
            tf.DrawString(detail, small, gray, new XRect(margin + 8, y - 2, width - 120, 60), XStringFormats.TopLeft);
            y += 30;
        }
        y += 6;
        gfx.DrawLine(XPens.LightGray, margin, y, margin + width, y);
        y += 20;

        // Payments received
        var received = payments.Where(p => !p.IsDeleted).OrderBy(p => p.Date).ToList();
        if (received.Count > 0)
        {
            gfx.DrawString("PAYMENTS RECEIVED", small, gray, new XPoint(margin + 8, y));
            y += 16;
            foreach (var p in received)
            {
                gfx.DrawString($"{p.Date:MMM d, yyyy}  -  {p.Method}", normal, XBrushes.Black, new XPoint(margin + 8, y));
                gfx.DrawString($"-{Money(p.Amount)}", normal, XBrushes.Black, new XRect(margin, y - 10, width - 8, 18), XStringFormats.TopRight);
                y += 16;
                if (y > PageHeightPt - 180) break; // one page only; the balance below is always exact
            }
            y += 8;
        }

        // Totals (balance is the authoritative stored value, so it matches the app exactly)
        var paid = commission.Price - commission.Remaining;
        void Total(string label, string value, XFont font, XBrush brush)
        {
            gfx.DrawString(label, font, brush, new XPoint(margin + 300, y));
            gfx.DrawString(value, font, brush, new XRect(margin, y - 12, width - 8, 20), XStringFormats.TopRight);
            y += 20;
        }
        Total("Subtotal", Money(commission.Price), normal, XBrushes.Black);
        Total("Paid", Money(paid), normal, XBrushes.Black);
        gfx.DrawLine(XPens.Black, margin + 300, y - 6, margin + width, y - 6);
        y += 6;
        Total("Balance due", Money(commission.Remaining), totalFont, commission.Remaining > 0 ? accent : XBrushes.Black);
        if (commission.Remaining <= 0)
            gfx.DrawString("PAID IN FULL", bold, accent, new XPoint(margin + 8, y - 20));

        // Footer
        gfx.DrawLine(XPens.LightGray, margin, PageHeightPt - 70, margin + width, PageHeightPt - 70);
        gfx.DrawString("Thank you for your business.", small, gray,
            new XRect(margin, PageHeightPt - 60, width, 14), XStringFormats.TopCenter);
        gfx.DrawString("Generated with Lancy", small, gray,
            new XRect(margin, PageHeightPt - 46, width, 14), XStringFormats.TopCenter);

        using var ms = new MemoryStream();
        document.Save(ms, closeStream: false);
        return Task.FromResult(ms.ToArray());
    }
}
