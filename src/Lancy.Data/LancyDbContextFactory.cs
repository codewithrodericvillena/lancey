using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lancy.Data;

public static class LancyPaths
{
    /// <summary>
    /// Resolves the per-OS app data directory:
    ///   Windows: %APPDATA%\Lancy
    ///   macOS:   ~/Library/Application Support/Lancy
    ///   Linux:   ~/.local/share/Lancy (XDG_DATA_HOME if set)
    /// Created if it doesn't exist.
    /// </summary>
    public static string GetAppDataDirectory()
    {
        var customDir = Environment.GetEnvironmentVariable("LANCY_DATA_DIR");
        if (!string.IsNullOrWhiteSpace(customDir))
        {
            Directory.CreateDirectory(customDir);
            return customDir;
        }

        var baseDir = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData,
            Environment.SpecialFolderOption.Create);

        // On Linux, ApplicationData maps to ~/.config by default under .NET;
        // XDG_DATA_HOME (or ~/.local/share) is more appropriate for app data.
        if (OperatingSystem.IsLinux())
        {
            var xdgData = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            baseDir = !string.IsNullOrWhiteSpace(xdgData)
                ? xdgData
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
        }

        var dir = Path.Combine(baseDir, "Lancy");
        Directory.CreateDirectory(dir);
        ImportLegacyData(Path.Combine(baseDir, "FcmsPro"), dir);
        return dir;
    }

    /// <summary>
    /// One-time import for users coming from FCMS Pro (this app's former name): copies the
    /// old data folder (database, attachments, backups) when the new database doesn't exist
    /// yet. Copies, never moves, so the old install keeps working. Never throws.
    /// </summary>
    public static void ImportLegacyData(string legacyDir, string newDir)
    {
        try
        {
            var newDb = Path.Combine(newDir, "lancy.db");
            var oldDb = Path.Combine(legacyDir, "fcms.db");
            if (File.Exists(newDb) || !File.Exists(oldDb)) return;

            foreach (var src in Directory.EnumerateFiles(legacyDir, "*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(legacyDir, src);
                if (rel.StartsWith("logs", StringComparison.OrdinalIgnoreCase)) continue;
                if (rel.StartsWith("fcms.db-", StringComparison.OrdinalIgnoreCase)) continue; // -wal/-shm
                if (rel.Equals("fcms.db", StringComparison.OrdinalIgnoreCase)) rel = "lancy.db";
                var dest = Path.Combine(newDir, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                File.Copy(src, dest, overwrite: false);
            }
        }
        catch
        {
            // Best effort: a failed import just means the user starts with an empty Lancy.
        }
    }

    public static string GetDatabasePath() => Path.Combine(GetAppDataDirectory(), "lancy.db");

    public static string GetLogsDirectory()
    {
        var dir = Path.Combine(GetAppDataDirectory(), "logs");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public static string GetBackupsDirectory()
    {
        var dir = Path.Combine(GetAppDataDirectory(), "backups");
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>
    /// Per-commission subfolder for CommissionAttachment files (reference
    /// images, delivered artwork, etc.) - one folder per commission so
    /// deleting a commission's attachments is a single directory delete
    /// rather than hunting individual files by Id.
    /// </summary>
    public static string GetAttachmentsDirectory(Guid commissionId)
    {
        var dir = Path.Combine(GetAppDataDirectory(), "attachments", commissionId.ToString());
        Directory.CreateDirectory(dir);
        return dir;
    }

    /// <summary>
    /// Same one-folder-per-entity pattern as GetAttachmentsDirectory above,
    /// generalized to Client/Invoice/Quote attachments (round 8 originally
    /// only covered Commission). Nested under "attachments/clients|invoices/quotes/"
    /// rather than reusing the flat "attachments/{id}" layout Commission uses -
    /// that flat layout is left exactly as-is for backward compatibility with
    /// any already-uploaded commission attachments, and namespacing the new
    /// ones by entity type also rules out a same-Guid collision between e.g.
    /// a commission and an unrelated invoice sharing an id (astronomically
    /// unlikely with GUIDs, but free to rule out entirely here).
    /// </summary>
    public static string GetClientAttachmentsDirectory(Guid clientId) =>
        GetNamespacedAttachmentsDirectory("clients", clientId);

    public static string GetInvoiceAttachmentsDirectory(Guid invoiceId) =>
        GetNamespacedAttachmentsDirectory("invoices", invoiceId);

    public static string GetQuoteAttachmentsDirectory(Guid quoteId) =>
        GetNamespacedAttachmentsDirectory("quotes", quoteId);

    private static string GetNamespacedAttachmentsDirectory(string entityFolder, Guid entityId)
    {
        var dir = Path.Combine(GetAppDataDirectory(), "attachments", entityFolder, entityId.ToString());
        Directory.CreateDirectory(dir);
        return dir;
    }
}

public static class LancyDbContextOptionsFactory
{
    // Plain, unencrypted SQLite - no Password keyword, no custom cipher
    // provider. An encrypted-database attempt (login password doubling as
    // the SQLCipher key) went through several rounds here and never
    // reached a working state - every attempt failed with the same
    // "file is not a database" error even after two independent,
    // documentation-backed fixes, which means there was a deeper mismatch
    // somewhere between how the key was set and how it was later read that
    // couldn't be pinned down without actually being able to run the app.
    // Removed entirely rather than left half-working.
    public static string BuildConnectionString(string? dbPathOverride = null)
    {
        var dbPath = dbPathOverride ?? LancyPaths.GetDatabasePath();
        return $"Data Source={dbPath}";
    }

    public static DbContextOptions<LancyDbContext> Create(string? dbPathOverride = null)
    {
        var builder = new DbContextOptionsBuilder<LancyDbContext>();
        // WAL mode set via a raw pragma the first time the connection opens -
        // see LancyDbContext startup call in App composition root (Phase 3 Avalonia).
        builder.UseSqlite(BuildConnectionString(dbPathOverride));
        return builder.Options;
    }
}

/// <summary>
/// Used only by `dotnet ef migrations add ...` at design time - not referenced
/// by the running app, which uses LancyDbContextOptionsFactory + DI instead.
/// </summary>
public class LancyDbContextDesignTimeFactory : IDesignTimeDbContextFactory<LancyDbContext>
{
    public LancyDbContext CreateDbContext(string[] args)
    {
        var options = LancyDbContextOptionsFactory.Create(dbPathOverride: "design_time.db");
        return new LancyDbContext(options);
    }
}
