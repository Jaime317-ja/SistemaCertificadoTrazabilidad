using CertificadoTrazabilidad.Models;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.Json;

namespace CertificadoTrazabilidad.Services
{
    public class MySqlService
    {
        private readonly IConfiguration _configuration;

        public MySqlService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public MySqlConnection ObtenerConexion()
        {
            string conexion = _configuration.GetConnectionString("MySqlConnection");

            return new MySqlConnection(conexion);
        }

        public List<Documento> ConsultarDocumentos(string ndi, string fechaini, string fechafin)
        {
            List<Documento> lista = new();

            using var conn = ObtenerConexion();
            conn.Open();

            using var cmd = new MySqlCommand("spw_ge_get_certificadotrazabilidad", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("iemprnit", ndi);
            cmd.Parameters.AddWithValue("ifechaini", fechaini);
            cmd.Parameters.AddWithValue("ifechafin", fechafin);
            cmd.Parameters.AddWithValue("idocid", 0);
            cmd.Parameters.AddWithValue("i_sp_opcion", "CON");

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Documento
                {
                    DocId = Convert.ToInt32(reader["docid"]),
                    TipoDocumento = reader["tipo_documento"].ToString() ?? "",
                    Numero = reader["numero"].ToString() ?? "",
                    FechaDocumento = Convert.ToDateTime(reader["fecha_documento"]),
                    Prefijo = reader["prefijo"].ToString() ?? "",
                    Cufe = reader["cufe"].ToString() ?? ""
                });
            }

            return lista;
        }

        public Dictionary<string, object> ObtenerDatosDocumento(string ndi, int docid)
        {
            using var conn = ObtenerConexion();
            conn.Open();

            using var cmd = new MySqlCommand("spw_ge_get_certificadotrazabilidad", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("iemprnit", ndi);
            cmd.Parameters.AddWithValue("ifechaini", DBNull.Value);
            cmd.Parameters.AddWithValue("ifechafin", DBNull.Value);
            cmd.Parameters.AddWithValue("idocid", docid);
            cmd.Parameters.AddWithValue("i_sp_opcion", "PDF");

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return new Dictionary<string, object>();

            string json = reader["documento_pdf"]?.ToString() ?? "[]";

            var lista = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);

            return lista?.FirstOrDefault() ?? new Dictionary<string, object>();
        }
    }
}
