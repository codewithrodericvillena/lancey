using System;
using System.IO;
using System.Text;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using Xunit;

namespace Lancy.Tests;

public class Ed25519Tests
{
    private const string PublicKeyBase64 = "V6h4U+KSlWQQ9oaWYpcsj1uYyVH4r+du4fYfrGsUMEk=";

    [Fact]
    public void VerifyEd25519Checker_WithValidSignature_Succeeds()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var data = Encoding.UTF8.GetBytes("Hello NetSparkle Ed25519 verification!");
            File.WriteAllBytes(tempFile, data);

            var checker = new Ed25519Checker(SecurityMode.Strict, PublicKeyBase64);
            var sig = "cbX76uz0pSw5sdmsk0+I6lM46WgBGfGl0jLWW4koH/kJwgBySDuh90rJ3X8NOYTzo7Iswea76wvVKFV3E8nZBQ==";

            var res = checker.VerifySignatureOfFile(sig, tempFile);
            Assert.Equal(ValidationResult.Valid, res);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void VerifyEd25519Checker_WithTamperedFile_Fails()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var data = Encoding.UTF8.GetBytes("Tampered content!");
            File.WriteAllBytes(tempFile, data);

            var checker = new Ed25519Checker(SecurityMode.Strict, PublicKeyBase64);
            var sig = "cbX76uz0pSw5sdmsk0+I6lM46WgBGfGl0jLWW4koH/kJwgBySDuh90rJ3X8NOYTzo7Iswea76wvVKFV3E8nZBQ==";

            var res = checker.VerifySignatureOfFile(sig, tempFile);
            Assert.Equal(ValidationResult.Invalid, res);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void VerifyEd25519Checker_WithInvalidSignature_Fails()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var data = Encoding.UTF8.GetBytes("Hello NetSparkle Ed25519 verification!");
            File.WriteAllBytes(tempFile, data);

            var checker = new Ed25519Checker(SecurityMode.Strict, PublicKeyBase64);
            var invalidSig = Convert.ToBase64String(new byte[64]);

            var res = checker.VerifySignatureOfFile(invalidSig, tempFile);
            Assert.Equal(ValidationResult.Invalid, res);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
