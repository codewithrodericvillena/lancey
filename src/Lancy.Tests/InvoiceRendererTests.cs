using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lancy.Core.Entities;
using Lancy.Core.Interfaces;
using Lancy.Pdf;
using Xunit;

namespace Lancy.Tests;

public class InvoiceRendererTests
{
    [Fact]
    public async Task RenderPdf_ProducesPdfBytes_AndInvoiceNumberIsStable()
    {
        var c = new Commission { Title = "Logo design", Price = 5000, DownPayment = 2000, Remaining = 3000, Deadline = new DateOnly(2026, 12, 1) };
        var payments = new List<Payment> { new() { Amount = 2000, Date = new DateOnly(2026, 11, 2) } };

        var bytes = await new InvoiceRenderer().RenderPdfAsync(c, new Client { Name = "Ana" }, payments, new AppSettings { BusinessName = "Studio" });

        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
        Assert.Equal(IInvoiceRenderer.InvoiceNumber(c), IInvoiceRenderer.InvoiceNumber(c));
        Assert.StartsWith("INV-", IInvoiceRenderer.InvoiceNumber(c));
    }
}
