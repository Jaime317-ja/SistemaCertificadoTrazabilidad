using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace CertificadoTrazabilidad.Services
{
    public class PdfService
    {
        public async Task<byte[]> GenerarPdfDesdeHtml(string html)
        {
            string edgePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = edgePath
            });

            await using var page = await browser.NewPageAsync();

            await page.SetContentAsync(html);

            return await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "10mm",
                    Bottom = "10mm",
                    Left = "10mm",
                    Right = "10mm"
                }
            });
        }
    }
}