using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Xml;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class Reference
    {
        public uint Folio { get; set; }
        public uint NroInt { get; set; }

        public string FileEnviado { get; set; }

        public string FileBasico { get; set; }

        public DateTime? FechaEmision { get; set; }

        public DateTime? FechaRecepcionSii { get; set; }

        public static IList<Reference> GetInfoFactura(ResultInstruction instruction)
        {
            try
            {
                XmlDocument document = Properties.Settings.Default.DBSoftland;
                string DataBaseName = "";
                IList<Reference> softland = new List<Reference>();
                foreach (XmlNode item in document.ChildNodes[0])
                {
                    if (item.Attributes["id"].Value == instruction.Creditor.ToString())
                    {
                        DataBaseName = item.FirstChild.InnerText;
                        break;
                    }
                }
                Conexion con = new Conexion(DataBaseName);
                StringBuilder query = new StringBuilder();

                query.Append("select g.Folio,g.NroInt, l.Fecha 'RecepcionSii', g.Fecha, ");
                query.Append("(select Archivo from softland.dte_archivos WHERE ID_Archivo = l.XMLSET) 'FileEnviado', ");
                query.Append("(select Archivo from softland.dte_archivos WHERE ID_Archivo = c.IDXMLDoc) 'FileBasico' ");
                query.Append("from softland.IW_GSaEn_RefDTE r ");
                query.Append("inner join softland.iw_gsaen g on r.NroInt = g.NroInt and r.Tipo = 'F' and ");
                query.Append($"(r.Glosa = '{instruction.PaymentMatrix.NaturalKey}' or r.FolioRef = '{instruction.PaymentMatrix.ReferenceCode}') ");
                query.Append("left join softland.DTE_DocCab c on c.Folio = g.Folio and c.NroInt = g.NroInt ");
                query.Append("left join softland.dte_logrecenv l on l.IDSetDTE = c.IDSetDTESII ");
                query.Append($"where g.NetoAfecto = {instruction.Amount} and g.CodAux = '{instruction.ParticipantDebtor.Rut}' ");
                query.Append($"order by g.Folio desc ");

                con.Query = query.ToString();
                DataTable dataTable = new DataTable();
                dataTable = Conexion.ExecuteReader(con);
                if (dataTable != null)
                {
                    foreach (DataRow item in dataTable.Rows)
                    {

                        Reference reference = new Reference();
                        if (item["Folio"] != DBNull.Value)
                        {
                            reference.Folio = Convert.ToUInt32(item["Folio"]);
                        }
                        if (item["NroInt"] != DBNull.Value)
                        {
                            reference.NroInt = Convert.ToUInt32(item["NroInt"]);
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
                        softland.Add(reference);
                    }
                    return softland;
                }
                return null;

            }
            catch (Exception)
            {

                throw;
            }
        }


        public static int InsertReference(ResultInstruction instruction, int nroInt)
        {

            try
            {
                XmlDocument document = Properties.Settings.Default.DBSoftland;
                string DataBaseName = "";
                foreach (XmlNode item in document.ChildNodes[0])
                {
                    if (item.Attributes["id"].Value == instruction.Creditor.ToString())
                    {
                        DataBaseName = item.FirstChild.InnerText;
                        break;
                    }
                }
                Conexion con = new Conexion(DataBaseName);
                CultureInfo cultureInfo = CultureInfo.GetCultureInfo("es-CL");
                StringBuilder query = new StringBuilder();
                string time = string.Format(cultureInfo, "{0:g}", Convert.ToDateTime(instruction.PaymentMatrix.PublishDate));

                query.Append("Insert Into softland.IW_GSaEn_RefDTE (Tipo, NroInt, LineaRef, CodRefSII, FolioRef, FechaRef, Glosa) values ");
                query.Append($"('F', {nroInt},2, 'SEN', '{instruction.PaymentMatrix.ReferenceCode}', '{time}', '{instruction.PaymentMatrix.NaturalKey}')");
                con.Query = query.ToString();
                if (Convert.ToInt32(Conexion.ExecuteNonQuery(con)) == 1) // 
                {
                    return Convert.ToInt32(Conexion.ExecuteNonQuery(con)); // Return 1 if ok!

                }
                return 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static int GetLastRef(ResultInstruction instruction)
        {
            try
            {
                XmlDocument document = Properties.Settings.Default.DBSoftland;
                string DataBaseName = "";
                foreach (XmlNode item in document.ChildNodes[0])
                {
                    if (item.Attributes["id"].Value == instruction.Creditor.ToString())
                    {
                        DataBaseName = item.FirstChild.InnerText;
                        break;
                    }
                }
                Conexion con = new Conexion(DataBaseName)
                {
                    Query = "select MAX(NroInt) from softland.iw_gsaen where Tipo = 'F'"
                };
                return Convert.ToInt32(Conexion.ExecuteScalar(con));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
