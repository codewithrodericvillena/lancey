using System;
using System.IO;
using Lancy.Data;
using Xunit;

namespace Lancy.Tests;

public class LegacyImportTests
{
    [Fact]
    public void ImportLegacyData_CopiesDbAndAttachments_AndNeverOverwritesExistingLancyDb()
    {
        var root = Path.Combine(Path.GetTempPath(), "lancy-legacy-" + Guid.NewGuid());
        var oldDir = Path.Combine(root, "FcmsPro");
        var newDir = Path.Combine(root, "Lancy");
        Directory.CreateDirectory(Path.Combine(oldDir, "attachments"));
        Directory.CreateDirectory(newDir);
        try
        {
            File.WriteAllText(Path.Combine(oldDir, "fcms.db"), "old-data");
            File.WriteAllText(Path.Combine(oldDir, "attachments", "a.txt"), "att");

            LancyPaths.ImportLegacyData(oldDir, newDir);

            Assert.Equal("old-data", File.ReadAllText(Path.Combine(newDir, "lancy.db")));
            Assert.Equal("att", File.ReadAllText(Path.Combine(newDir, "attachments", "a.txt")));
            Assert.True(File.Exists(Path.Combine(oldDir, "fcms.db"))); // copy, not move

            File.WriteAllText(Path.Combine(newDir, "lancy.db"), "new-data");
            LancyPaths.ImportLegacyData(oldDir, newDir);
            Assert.Equal("new-data", File.ReadAllText(Path.Combine(newDir, "lancy.db")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
