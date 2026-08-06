using Microsoft.AspNetCore.Mvc;
using CertificadoTrazabilidad.Services;
using CertificadoTrazabilidad.Models;

namespace CertificadoTrazabilidad.Controllers
{
    public class CertificadoController : Controller
    {
        private readonly MySqlService _mySqlService;
        /*
        public CertificadoController(MySqlService mySqlService)
        {
            _mySqlService = mySqlService;
        }*/

        public CertificadoController(MySqlService mySqlService, PdfService pdfService)
        {
            _mySqlService = mySqlService;
            _pdfService = pdfService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new List<Documento>());
        }

        [HttpPost]
        public IActionResult Index(string fechaini, string fechafin)
        {
            if (string.IsNullOrWhiteSpace(fechaini) || string.IsNullOrWhiteSpace(fechafin))
            {
                TempData["Mensaje"] = "Debe seleccionar las fechas para buscar los documentos.";
                TempData["Tipo"] = "warning";
                return View(new List<Documento>());
            }

            // Validar rango de fechas
            DateTime fechaIni = Convert.ToDateTime(fechaini);
            DateTime fechaFin = Convert.ToDateTime(fechafin);

            if (fechaIni > fechaFin)
            {
                TempData["Mensaje"] = "La fecha final no puede ser menor que la fecha inicial.";
                TempData["Tipo"] = "warning";
                return View(new List<Documento>());
            }

            var documentos = _mySqlService.ConsultarDocumentos(
                "900080835",
                fechaini,
                fechafin);

            if (documentos == null || documentos.Count == 0)
            {
                TempData["Mensaje"] = "No se encontró ninguna información.";
                TempData["Tipo"] = "warning";
                return View(new List<Documento>());
            }

            return View(documentos);
        }

        private readonly PdfService _pdfService;

        

        [HttpPost]
        public async Task<IActionResult> GenerarPdf(int docid)
        {
            if (docid == 0)
            {
                TempData["Mensaje"] = "Debe seleccionar un documento.";
                TempData["Tipo"] = "warning";
                return RedirectToAction("Index");
            }

            var data = _mySqlService.ObtenerDatosDocumento("900080835", docid);

            if (data == null || data.Count == 0)
            {
                TempData["Mensaje"] = "No se encontró información para el documento seleccionado.";
                TempData["Tipo"] = "warning";
                return RedirectToAction("Index");
            }

            // Leer la plantilla HTML
            string rutaHtml = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Archivos",
                "certficadotrazabilidad.html");

            string html = System.IO.File.ReadAllText(rutaHtml);

            // Reemplazar los marcadores del HTML
            var valores = new Dictionary<string, string>
            {
                { "prefijo", data.GetValueOrDefault("prefijo")?.ToString() ?? "" },
                { "numero", data.GetValueOrDefault("numero_transaccion")?.ToString() ?? "" },
                { "tipo_documento", data.GetValueOrDefault("tipo_documento")?.ToString() ?? "" },
                { "cufe", data.GetValueOrDefault("cufe")?.ToString() ?? "" },
                { "empresa_emisor", data.GetValueOrDefault("empresa_emisor")?.ToString() ?? "" },
                { "nit_emisor", data.GetValueOrDefault("nit_emisor")?.ToString() ?? "" },
                { "razon_social_receptor", data.GetValueOrDefault("razon_social_receptor")?.ToString() ?? "" },
                { "nit_cliente", data.GetValueOrDefault("nit_cliente")?.ToString() ?? "" },
                { "estado_dian", data.GetValueOrDefault("estado_dian")?.ToString() ?? "" },
                { "correo_destino", data.GetValueOrDefault("correo_electronico")?.ToString() ?? "" },
                { "fecha_envio", data.GetValueOrDefault("fecha_envio")?.ToString() ?? "" },
                { "destinatario", data.GetValueOrDefault("razon_social_receptor")?.ToString() ?? "" },
                { "estado_envio", "Documento Entregado" }
            };

            foreach (var item in valores)
            {
                html = html.Replace("#" + item.Key + "#", item.Value);
            }

            // Generar el PDF desde el HTML
            byte[] pdf = await _pdfService.GenerarPdfDesdeHtml(html);

            return File(pdf, "application/pdf", $"certificado_{docid}.pdf");
        }
    }
}