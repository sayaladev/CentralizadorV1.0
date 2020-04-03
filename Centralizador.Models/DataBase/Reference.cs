using System;
using System.Collections.Generic;
using System.Data;
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

        public DateTime FechaEmision { get; set; }

        public DateTime FechaRecepcionSii { get; set; }

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
                if (DataBaseName == null)
                {
                    return null;
                }
                Conexion con = new Conexion
                {
                    Cnn = $"Data Source=DEVELOPER;Initial Catalog={DataBaseName};Persist Security Info=True;User ID=sa;Password=123456"
                };
                StringBuilder query = new StringBuilder();
                query.Append("select g.Folio,r.NroInt, l.Fecha 'RecepcionSii', g.Fecha, ");
                query.Append("(select Archivo from softland.dte_archivos WHERE ID_Archivo = l.XMLSET) 'FileEnviado', ");
                query.Append("(select Archivo from softland.dte_archivos WHERE ID_Archivo = c.IDXMLDoc) 'FileBasico' ");
                query.Append("from softland.IW_GSaEn_RefDTE r ");
                query.Append("inner join softland.iw_gsaen g on r.NroInt = g.NroInt and r.CodRefSII = 'SEN' and r.Tipo = 'F' ");
                query.Append("left join softland.DTE_DocCab c on c.Folio = g.Folio and c.NroInt = g.NroInt ");
                query.Append("left join softland.dte_logrecenv l on l.IDSetDTE = c.IDSetDTESII ");
                query.Append($"where g.NetoAfecto = {instruction.Amount} and g.CodAux = '{instruction.ParticipantDebtor.Rut}' and r.Glosa = '{instruction.PaymentMatrix.NaturalKey}' ");
                query.Append($"and r.FolioRef = '{instruction.PaymentMatrix.ReferenceCode}' order by g.Folio desc ");

                con.Query = query.ToString();
                DataTable dataTable = new DataTable();
                dataTable = Conexion.ConexionBdQuery(con);
                if (dataTable != null)
                {
                    foreach (DataRow item in dataTable.Rows)
                    {

                        Reference r = new Reference();

                        if (!item.IsNull("Folio")) r.Folio = Convert.ToUInt32(item["Folio"]);                      
                        if (!item.IsNull("NroInt")) r.NroInt = Convert.ToUInt32(item["NroInt"]);         
                        if (!item.IsNull("RecepcionSii"))  r.FechaRecepcionSii = Convert.ToDateTime(item["RecepcionSii"]);
                        if (!item.IsNull("Fecha"))  r.FechaEmision = Convert.ToDateTime(item["Fecha"]);
                        if (!item.IsNull("FileEnviado"))  r.FileEnviado = item["FileEnviado"].ToString();
                        if (!item.IsNull("FileBasico"))  r.FileBasico = item["FileBasico"].ToString();


                        
                        softland.Add(r);

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
    }
}
