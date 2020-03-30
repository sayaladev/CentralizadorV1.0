using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class Reference
    {
        public uint NroInt { get; set; }

        public uint Folio { get; set; }

        public DateTime FechaEmision { get; set; }

        public string FolioRef { get; set; }

        public DateTime FechaRef { get; set; }

        public string Glosa { get; set; }

        public byte FormaPago { get; set; }

        public string DetProduct { get; set; }

        public static IList<Reference> GetReferenceByGlosa(ResultInstruction instruction)
        {
            XmlDocument document = Properties.Settings.Default.DBSoftland;
            string DataBaseName = "";
            IList<Reference> references = new List<Reference>();

            foreach (XmlNode item in document.ChildNodes[0])
            {
                if (item.Attributes["id"].Value == instruction.Creditor.ToString())
                {
                    DataBaseName = item.FirstChild.InnerText;
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
            con.Query += "select r.NroInt, g.Folio, g.Fecha, r.FolioRef, r.FechaRef, r.Glosa, g.FmaPago, m.DetProd ";
            con.Query += "from softland.IW_GSaEn_RefDTE r inner join softland.iw_gsaen g on r.NroInt = g.NroInt and r.CodRefSII = 'SEN' and r.Tipo = 'F' ";
            con.Query += $"left join softland.iw_gmovi m on g.NroInt = m.NroInt and g.Tipo = r.Tipo where g.NetoAfecto = {instruction.Amount} and ";
            con.Query += $"g.CodAux = '{instruction.Participant.Rut}' and (r.Glosa = '{instruction.PaymentMatrix.NaturalKey}' or ";
            con.Query += $"r.FolioRef = '{instruction.PaymentMatrix.ReferenceCode}')";

            DataTable dataTable = new DataTable();
            dataTable = Conexion.ConexionBdQuery(con);
            if (dataTable != null)
            {
                foreach (DataRow item in dataTable.Rows)
                {
                    Reference r = new Reference
                    {
                        NroInt = Convert.ToUInt32(dataTable.Rows[0].ItemArray[0]),
                        Folio = Convert.ToUInt32(dataTable.Rows[0].ItemArray[1]),
                        FechaEmision = Convert.ToDateTime(dataTable.Rows[0].ItemArray[2]),
                        FolioRef = dataTable.Rows[0].ItemArray[3].ToString(),
                        FechaRef = Convert.ToDateTime(dataTable.Rows[0].ItemArray[4]),
                        Glosa = dataTable.Rows[0].ItemArray[5].ToString(),
                        FormaPago = Convert.ToByte(dataTable.Rows[0].ItemArray[6]),
                        DetProduct = dataTable.Rows[0].ItemArray[7].ToString(),
                        // Status 


                    };
                    references.Add(r);
                }
                return references;
            }
            return null;
        }
    }
}


