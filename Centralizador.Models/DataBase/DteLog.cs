using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class DteLog
    {
        public string IDSetDTE { get; set; }
        public string RutEnvia { get; set; }
        public string OrigenDestino { get; set; }
        public DateTime Fecha { get; set; }
        public string Archivo { get; set; }
        public int Enviado { get; set; }
        public int Aceptado { get; set; }
        public string TipoArchivo { get; set; }
        public string Motivo { get; set; }
        public string TrackId { get; set; }
        public int XMLSET { get; set; }


        public static IList<DteLog> GetDteLogs(string IDSetDTESII, string IDSetDTECliente, ResultParticipant participant, Conexion conexion)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                IList<DteLog> lista = new List<DteLog>();
                DataTable dataTable;

                query.Append("SELECT IDSetDTE ");
                query.Append("        ,RutEnvia ");
                query.Append("        ,OrigenDestino ");
                query.Append("        ,Fecha ");
                query.Append("        ,Archivo ");
                query.Append("        ,Enviado ");
                query.Append("        ,Aceptado ");
                query.Append("        ,TipoArchivo ");
                query.Append("        ,Motivo ");
                query.Append("        ,TrackId ");
                query.Append("        ,XMLSET ");
                query.Append("FROM softland.dte_logrecenv ");
                query.Append("WHERE IDSetDTE IN ( ");
                query.Append($"                '{IDSetDTESII}' ");
                query.Append($"                ,'{IDSetDTECliente}' ");
                query.Append("                ) ");
                query.Append($"        AND RUTEmisor = '{participant.Rut}-{participant.VerificationCode}' "); //76807947-1
                conexion.Query = query.ToString();
                dataTable = Conexion.ExecuteReaderAsync(conexion).Result;
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    foreach (DataRow item in dataTable.Rows)
                    {
                        {
                            DteLog log = new DteLog();
                            if (item[0] != DBNull.Value) { log.IDSetDTE = Convert.ToString(item[0]); }
                            if (item[1] != DBNull.Value) { log.RutEnvia = Convert.ToString(item[1]); }
                            if (item[2] != DBNull.Value) { log.OrigenDestino = Convert.ToString(item[2]); }
                            if (item[3] != DBNull.Value) { log.Fecha = Convert.ToDateTime(item[3]); }
                            if (item[4] != DBNull.Value) { log.Archivo = Convert.ToString(item[4]); }
                            if (item[5] != DBNull.Value) { log.Enviado = Convert.ToInt32(item[5]); }
                            if (item[6] != DBNull.Value) { log.Aceptado = Convert.ToInt32(item[6]); }
                            if (item[7] != DBNull.Value) { log.TipoArchivo = Convert.ToString(item[7]); }
                            if (item[8] != DBNull.Value) { log.Motivo = Convert.ToString(item[8]); }
                            if (item[9] != DBNull.Value) { log.TrackId = Convert.ToString(item[9]); }
                            if (item[10] != DBNull.Value) { log.XMLSET = Convert.ToInt32(item[10]); }
                            lista.Add(log);
                        }
                    }
                    return lista;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
    }
}
