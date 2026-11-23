using Lancy.Core.Entities;
using Lancy.Core.Metrics;

namespace Lancy.Core.Interfaces;

/// <summary>
/// Implemented in Lancy.Pdf (PdfSharpCore). Kept as an interface here so
/// Lancy.Avalonia depends only on abstractions, and the PDF library choice
/// can change later without touching ViewModels.
/// </summary>

public interface ITaxSummaryRenderer
{
    Task<byte[]> RenderPdfAsync(TaxSummary summary, AppSettings businessSettings, CancellationToken ct = default);
}

/// <summary>One-page client-facing invoice for a single commission.</summary>
public interface IInvoiceRenderer
{
    /// <summary>Stable, human-readable invoice number, e.g. INV-202611-3F2A. Same commission always gives the same number.</summary>
    static string InvoiceNumber(Commission commission) =>
        $"INV-{commission.DateAdded:yyyyMM}-{commission.Id.ToString("N")[..4].ToUpperInvariant()}";

    Task<byte[]> RenderPdfAsync(
        Commission commission, Client? client, IReadOnlyList<Payment> payments,
        AppSettings businessSettings, CancellationToken ct = default);
}
