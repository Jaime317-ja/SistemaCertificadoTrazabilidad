namespace CertificadoTrazabilidad.Models
{
    public class Documento
    {
        public int DocId { get; set; }

        public string TipoDocumento { get; set; } = string.Empty;

        public string Numero { get; set; } = string.Empty;

        public DateTime FechaDocumento { get; set; }

        public string Prefijo { get; set; } = string.Empty;

        public string Cufe { get; set; } = string.Empty;
    }
}
