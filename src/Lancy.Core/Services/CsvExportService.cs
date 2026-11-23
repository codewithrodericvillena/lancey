using System.Globalization;
using System.Text;
using Lancy.Core.Entities;
using Lancy.Core.Interfaces;

namespace Lancy.Core.Services;

/// <summary>
/// Exports clients, commissions, payments and expenses as UTF-8 CSV (with BOM, so Excel
/// reads accents and currency symbols correctly) for accountants and spreadsheets.
/// </summary>
public class CsvExportService
{
    private readonly IUnitOfWork _uow;

    public CsvExportService(IUnitOfWork uow) => _uow = uow;

    /// <summary>Writes clients.csv, commissions.csv, payments.csv and expenses.csv into <paramref name="directory"/>; returns the files written.</summary>
    public async Task<IReadOnlyList<string>> ExportAllAsync(string directory, CancellationToken ct = default)
    {
        Directory.CreateDirectory(directory);
        var clients = await _uow.Clients.GetAllAsync(ct);
        var commissions = await _uow.Commissions.GetAllAsync(ct);
        var payments = await _uow.Payments.GetAllAsync(ct);
        var expenses = await _uow.Expenses.GetAllAsync(ct);
        var names = clients.ToDictionary(c => c.Id, c => c.Name);
        var titles = commissions.ToDictionary(c => c.Id, c => c.Title);

        var files = new List<(string Name, string Content)>
        {
            ("clients.csv", Build(
                new[] { "Name", "Type", "Email", "Phone", "Social", "Notes", "Added" },
                clients.Select(c => new[] { c.Name, c.ClientType.ToString(), c.Email, c.Phone, c.Social, c.Notes, Date(c.DateAdded) }))),
            ("commissions.csv", Build(
                new[] { "Title", "Client", "Service", "Status", "Priority", "Price", "Down payment", "Remaining", "Deadline", "Added" },
                commissions.Select(c => new[] { c.Title, names.GetValueOrDefault(c.ClientId), c.ServiceType, c.Status.ToString(), c.Priority.ToString(),
                    Num(c.Price), Num(c.DownPayment), Num(c.Remaining), c.Deadline?.ToString("yyyy-MM-dd"), Date(c.DateAdded) }))),
            ("payments.csv", Build(
                new[] { "Date", "Client", "Commission", "Amount", "Method", "Reference", "Notes" },
                payments.Select(p => new[] { p.Date.ToString("yyyy-MM-dd"), names.GetValueOrDefault(p.ClientId), titles.GetValueOrDefault(p.CommissionId),
                    Num(p.Amount), p.Method.ToString(), p.ReferenceNumber, p.Notes }))),
            ("expenses.csv", Build(
                new[] { "Date", "Description", "Category", "Amount", "Notes" },
                expenses.Select(e => new[] { e.Date.ToString("yyyy-MM-dd"), e.Description, e.Category.ToString(), Num(e.Amount), e.Notes }))),
        };

        var written = new List<string>();
        foreach (var (name, content) in files)
        {
            var path = Path.Combine(directory, name);
            await File.WriteAllTextAsync(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), ct);
            written.Add(path);
        }
        return written;
    }

    public static string Build(string[] header, IEnumerable<string?[]> rows)
    {
        var sb = new StringBuilder();
        sb.Append(string.Join(",", header.Select(Escape))).Append("\r\n");
        foreach (var row in rows)
            sb.Append(string.Join(",", row.Select(Escape))).Append("\r\n");
        return sb.ToString();
    }

    /// <summary>
    /// RFC 4180 quoting, plus a leading apostrophe on cells that start with = + - @ so a client
    /// named "=HYPERLINK(...)" can't run as a formula when the file is opened in Excel/Sheets.
    /// </summary>
    public static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if ("=+-@\t\r".Contains(value[0])) value = "'" + value;
        return value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
    }

    private static string Num(decimal v) => v.ToString("0.00", CultureInfo.InvariantCulture);
    private static string Date(DateTimeOffset d) => d.ToLocalTime().ToString("yyyy-MM-dd");
}
