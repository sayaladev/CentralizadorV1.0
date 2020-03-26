using System;
using System.Data;
using System.Xml;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class DBReference
    {



        public static DBReference GetReferenceByFolio(uint folio, uint codAux, ResultParticipant participante)
        {
            XmlDocument document = Properties.Settings.Default.DBSoftland;
            string DataBaseName = "";
            foreach (XmlNode item in document.ChildNodes[0])
            {
                if (item.Attributes["id"].Value == participante.Id.ToString())
                {
                    DataBaseName = item.FirstChild.InnerText;
                }
            }

            if (DataBaseName == null)
            {
                return null;
            }

            DBReference reference = new DBReference();
            DBConn con = new DBConn
            {
                Cnn = $"Data Source=DEVELOPER;Initial Catalog={DataBaseName};Persist Security Info=True;User ID=sa;Password=123456"
            };
            con.Query += "select r.NroInt, r.FolioRef, r.FechaRef, r.Glosa, g.FmaPago, g.NetoAfecto, m.DetProd ";
            con.Query += "from softland.IW_GSaEn_RefDTE r inner join softland.iw_gsaen g on r.NroInt = g.NroInt and r.CodRefSII = 'SEN' and r.Tipo = 'F' ";
            con.Query += $"left join softland.iw_gmovi m on g.NroInt = m.NroInt and g.Tipo = r.Tipo where g.Folio = {folio} and g.CodAux = '{codAux}' ";

            DataTable dataTable = new DataTable();
            dataTable = DBConn.ConexionBdQuery(con);
            if (dataTable != null)
            {
                foreach (DataRow item in dataTable.Rows)
                {
                    //reference.NroInt = Convert.ToUInt32(item["NroInt"]);
                    //reference.FolioRef = item["FolioRef"].ToString();
                    //reference.FechaRef = Convert.ToDateTime(item["FechaRef"]);
                    //reference.Glosa = item["Glosa"].ToString();
                    //reference.FmaPago = Convert.ToByte(item["FmaPago"]);
                    //reference.NetoAfecto = Convert.ToUInt32(item["NetoAfecto"]);
                    //reference.DetProd = item["DetProd"].ToString();
                }
                return reference;
            }
            return null;
        }

        public static ResultDte GetReferenceByGlosa(ResultInstruction instruction)
        {
            XmlDocument document = Properties.Settings.Default.DBSoftland;
            string DataBaseName = "";
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

            ResultDte dte = new ResultDte();

            DBConn con = new DBConn
            {
                Cnn = $"Data Source=DEVELOPER;Initial Catalog={DataBaseName};Persist Security Info=True;User ID=sa;Password=123456"
            };
            con.Query += "select r.NroInt, g.Folio,g.Fecha,r.FolioRef, r.FechaRef, r.Glosa, g.FmaPago, g.NetoAfecto, m.DetProd ";
            con.Query += "from softland.IW_GSaEn_RefDTE r inner join softland.iw_gsaen g on r.NroInt = g.NroInt and r.CodRefSII = 'SEN' and r.Tipo = 'F' ";
            con.Query += $"left join softland.iw_gmovi m on g.NroInt = m.NroInt and g.Tipo = r.Tipo where g.NetoAfecto = {instruction.Amount} and ";
            con.Query += $"g.CodAux = '{instruction.ResultParticipantMapping.Rut}' and r.Glosa = '{instruction.ResultPaymentMatrixMapping.NaturalKey}' ";
            con.Query += $"and r.FolioRef = '{instruction.ResultPaymentMatrixMapping.ReferenceCode}'";

            DataTable dataTable = new DataTable();
            dataTable = DBConn.ConexionBdQuery(con);
            if (dataTable != null && dataTable.Rows.Count == 1)
            {
                dte.NroInt = Convert.ToUInt32(dataTable.Rows[0].ItemArray[0]);
                dte.Folio = Convert.ToUInt32(dataTable.Rows[0].ItemArray[1]);
                dte.EmissionDt = Convert.ToDateTime(dataTable.Rows[0].ItemArray[2]);
                return dte;
            }
            return null;
        }

    }

}


