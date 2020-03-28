using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class InfoSii
    {
        public uint NroInt { get; set; }
        public uint Folio { get; set; }
        public byte EnviadoSII { get; set; }
        public DateTime FechaEnvioSII { get; set; }
        public uint IdxmlDoc { get; set; }
        public ulong TrackID { get; set; }
        public string OrigenDestino { get; set; }
        public uint XmlSet { get; set; }
        public string FileEnviado { get; set; }
        public string FileBasico { get; set; }

        public static InfoSii GetSendSiiByFolio(ResultInstruction instruction, uint folio)
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

            if (DataBaseName == null)
            {
                return null;
            }

            InfoSii infoSii = new InfoSii();
            DBaseConn con = new DBaseConn
            {
                Cnn = $"Data Source=DEVELOPER;Initial Catalog={DataBaseName};Persist Security Info=True;User ID=sa;Password=123456"
            };
            con.Query += "select d.NroInt, d.Folio,d.EnviadoSII, d.FechaEnvioSII, d.IDXMLDoc,d.TrackID,l.OrigenDestino, l.XMLSET, ";
            con.Query += "(select Archivo from softland.dte_archivos WHERE ID_Archivo = l.XMLSET) 'FileEnviado', ";
            con.Query += "(select Archivo from softland.dte_archivos WHERE ID_Archivo = d.IDXMLDoc) 'FileBasico' ";
            con.Query += "from softland.dte_doccab d ";
            con.Query += "left join softland.dte_logrecenv l ";
            con.Query += "on d.IDSetDTESII = l.IDSetDTE ";
            con.Query += $"where d.Tipo = 'F' and d.TipoDTE = 33 and d.RUTRecep = '{instruction.Participant.Rut}-{instruction.Participant.VerificationCode}' and d.Folio = {folio} ";

            DataTable dataTable = new DataTable();
            dataTable = DBaseConn.ConexionBdQuery(con);
            if (dataTable != null)
            {
                infoSii.NroInt = Convert.ToUInt32(dataTable.Rows[0].ItemArray[0]);
                infoSii.Folio = Convert.ToUInt32(dataTable.Rows[0].ItemArray[1]);
                infoSii.EnviadoSII = Convert.ToByte(dataTable.Rows[0].ItemArray[2]);
                infoSii.FechaEnvioSII = Convert.ToDateTime(dataTable.Rows[0].ItemArray[3]);
                infoSii.IdxmlDoc = Convert.ToUInt32( dataTable.Rows[0].ItemArray[4].ToString());
                infoSii.TrackID = Convert.ToUInt64(dataTable.Rows[0].ItemArray[5].ToString());
                infoSii.OrigenDestino = dataTable.Rows[0].ItemArray[6].ToString();
                infoSii.XmlSet = Convert.ToUInt32(dataTable.Rows[0].ItemArray[7].ToString());
                infoSii.FileEnviado = dataTable.Rows[0].ItemArray[8].ToString();
                infoSii.FileBasico = dataTable.Rows[0].ItemArray[9].ToString();

                return infoSii;                
            }
            return null;
        }
    }    
}