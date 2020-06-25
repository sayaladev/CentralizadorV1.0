using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class Reference
    {
        public int Folio { get; set; }
        public int NroInt { get; set; }
        public string FileEnviado { get; set; }
        public string FileBasico { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaRecepcionSii { get; set; }
        public int NetoAfecto { get; set; }
        public int Iva { get; set; }
        public int Total { get; set; }
        public string Rut { get; set; }
        public static IList<Reference> GetInfoFactura(ResultInstruction instruction, Conexion conexion)
        {

            try
            {
                StringBuilder query = new StringBuilder();

                query.Append("SELECT ");
                query.Append("  g.Folio, ");
                query.Append("  g.NroInt, ");
                query.Append("  l.Fecha 'RecepcionSii', ");
                query.Append("  g.Fecha, ");
                query.Append("  (SELECT Archivo FROM softland.dte_archivos WHERE ID_Archivo = l.XMLSET) 'FileEnviado', ");
                query.Append("  (SELECT Archivo FROM softland.dte_archivos WHERE ID_Archivo = c.IDXMLDoc) 'FileBasico', ");
                query.Append("  g.NetoAfecto, ");
                query.Append("  g.IVA, ");
                query.Append("  g.Total, ");
                query.Append("  g.CodAux ");
                query.Append("FROM softland.IW_GSaEn_RefDTE r ");
                query.Append("FULL OUTER JOIN softland.iw_gsaen g ");
                //query.Append("INNER JOIN softland.iw_gsaen g ");
                query.Append("  ON r.NroInt = g.NroInt ");
                query.Append("  AND r.Tipo = 'F' ");
                query.Append($"  AND (r.Glosa = '{instruction.PaymentMatrix.NaturalKey}' ");
                query.Append($"  OR r.FolioRef = '{instruction.PaymentMatrix.ReferenceCode}') ");
                query.Append("LEFT JOIN softland.DTE_DocCab c ");
                query.Append("  ON c.Folio = g.Folio ");
                query.Append("  AND c.NroInt = g.NroInt ");
                query.Append("LEFT JOIN softland.dte_logrecenv l ");
                query.Append("  ON l.IDSetDTE = c.IDSetDTESII ");
                query.Append($"WHERE g.NetoAfecto = {instruction.Amount} ");
                query.Append($"AND g.CodAux = '{instruction.ParticipantDebtor.Rut}' ");
                query.Append("ORDER BY g.Folio DESC ");

                IList<Reference> softland = new List<Reference>();
                conexion.Query = query.ToString();
                DataTable dataTable = new DataTable();
                dataTable = Conexion.ExecuteReaderAsync(conexion).Result;
                if (dataTable != null && dataTable.Rows.Count > 0)
                {

                    foreach (DataRow item in dataTable.Rows)
                    {
                        Reference reference = new Reference();
                        if (item["Folio"] != DBNull.Value)
                        {
                            reference.Folio = Convert.ToInt32(item["Folio"]);
                        }
                        if (item["NroInt"] != DBNull.Value)
                        {
                            reference.NroInt = Convert.ToInt32(item["NroInt"]);
                        }
                        if (item["RecepcionSii"] != DBNull.Value)
                        {
                            reference.FechaRecepcionSii = Convert.ToDateTime(item["RecepcionSii"]);
                        }
                        if (item["Fecha"] != DBNull.Value)
                        {
                            reference.FechaEmision = Convert.ToDateTime(item["Fecha"]);
                        }
                        if (item["FileEnviado"] != DBNull.Value)
                        {
                            reference.FileEnviado = item["FileEnviado"].ToString();
                        }
                        if (item["FileBasico"] != DBNull.Value)
                        {
                            reference.FileBasico = item["FileBasico"].ToString();
                        }
                        if (item["NetoAfecto"] != DBNull.Value)
                        {
                            reference.NetoAfecto = Convert.ToInt32(item["NetoAfecto"]);
                        }
                        if (item["IVA"] != DBNull.Value)
                        {
                            reference.Iva = Convert.ToInt32(item["IVA"]);
                        }
                        if (item["Total"] != DBNull.Value)
                        {
                            reference.Total = Convert.ToInt32(item["Total"]);
                        }
                        if (item["CodAux"] != DBNull.Value)
                        {
                            reference.Rut = item["CodAux"].ToString();
                        }
                        softland.Add(reference);
                    }
                    return softland;

                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }


        public static int InsertReference(ResultInstruction instruction, int nroInt, Conexion conexion)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                CultureInfo cultureInfo = CultureInfo.GetCultureInfo("es-CL");
                string time = string.Format(cultureInfo, "{0:yyyy-MM-dd HH:mm:ss}", instruction.PaymentMatrix.PublishDate);
                query.Append($"IF NOT EXISTS (SELECT * FROM softland.IW_GSaEn_RefDTE WHERE NroInt = {nroInt} ");
                query.Append("  AND Tipo = 'F' ");
                query.Append("  AND CodRefSII = 'SEN') ");
                query.Append("BEGIN ");
                query.Append("  INSERT INTO softland.IW_GSaEn_RefDTE (Tipo, NroInt, LineaRef, CodRefSII, FolioRef, FechaRef, Glosa) ");
                query.Append($"  VALUES ('F', {nroInt}, 2, 'SEN', '{instruction.PaymentMatrix.ReferenceCode}', '{time}', '{instruction.PaymentMatrix.NaturalKey}') ");
                query.Append("  UPDATE softland.iw_gmovi ");
                query.Append($" SET DetProd = '{instruction.PaymentMatrix.NaturalKey}' ");
                query.Append($" WHERE NroInt = {nroInt} ");
                query.Append("  AND Tipo = 'F' ");
                query.Append("END ");

                conexion.Query = query.ToString();
                return Convert.ToInt32(Conexion.ExecuteNonQueryAsync(conexion).Result); // Return 1 if ok!
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
