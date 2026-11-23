using Lancy.Core.Services;
using Xunit;

namespace Lancy.Tests;

public class CsvExportServiceTests
{
    [Theory]
    [InlineData("plain", "plain")]
    [InlineData("a,b", "\"a,b\"")]
    [InlineData("say \"hi\"", "\"say \"\"hi\"\"\"")]
    [InlineData("=HYPERLINK(\"x\")", "\"'=HYPERLINK(\"\"x\"\")\"")]
    [InlineData("-5", "'-5")]
    [InlineData(null, "")]
    public void Escape_QuotesAndNeutralisesFormulas(string? input, string expected) =>
        Assert.Equal(expected, CsvExportService.Escape(input));

    [Fact]
    public void Build_WritesHeaderAndRowsWithCrLf() =>
        Assert.Equal("A,B\r\n1,\"x,y\"\r\n",
            CsvExportService.Build(new[] { "A", "B" }, new[] { new string?[] { "1", "x,y" } }));
}
