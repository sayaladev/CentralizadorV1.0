using System;
using System.Data;
using System.Xml;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class DBSendSii
    {
        public byte EnviadoSII { get; set; }
        public byte AceptadoSII { get; set; }
        public byte EnviadoCliente { get; set; }
        public byte AceptadoCliente { get; set; }
        public string Motivo { get; set; }
        public string IdSetDTESII { get; set; }
        public ulong TrackID { get; set; }
        public uint XMLEnviado { get; set; }
        public string FileEnviado { get; set; }
        public string Archivo { get; set; }
        public DateTime FechaEnvioSII { get; set; }
        public DateTime FechaEnvioCliente { get; set; }
        public uint XMLBasico { get; set; }
        public string FileBasico { get; set; }
        public string OrigenDestino { get; set; }


        public static DBSendSii GetSendSiiByFolio(ResultParticipant participante, ResultInstruction instruction)
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

            DBSendSii dte = new DBSendSii();
            DBConn con = new DBConn
            {
                Cnn = $"Data Source=DEVELOPER;Initial Catalog={DataBaseName};Persist Security Info=True;User ID=sa;Password=123456"
            };
            con.Query += "Select DDC.EnviadoSII, DDC.AceptadoSII,ddc.EnviadoCliente,ddc.AceptadoCliente, DDC.Motivo, DDC.idSetDTESII, ";
            con.Query += "(CASE When IsNull(DDC.TrackID, '') = '' Then DLog.TrackID Else DDC.TrackID END) as TrackID, DLog.XMLSET 'XMLEnviado', ";
            con.Query += "(select Archivo from softland.dte_archivos WHERE ID_Archivo = DLog.XMLSET) 'FileEnviado', ";
            con.Query += "DDC.Archivo, ddc.FechaEnvioSII, ddc.FechaEnvioCliente, DDC.IDXMLDoc 'XMLBasico', ";
            con.Query += "(select Archivo from softland.dte_archivos WHERE ID_Archivo = DDC.IDXMLDoc) 'FileBasico', DLog.OrigenDestino ";
            con.Query += "From softland.DTE_DocCab DDC  ";
            con.Query += "inner join softland.Dte_SIITDOC DSII  ";
            con.Query += "on DDC.TipoDTE = DSII.DocCod  ";
            con.Query += "left JOIN softland.DTE_LogRecEnv DLog  ";
            con.Query += "ON DDC.RutEmisor = DLog.RutEmisor AND DDC.IDSetDTESII = DLog.IDSetDTE  ";
            con.Query += $"Where DSII.Tipo = 'F' and DSII.SubTipoDocto = 'T' AND DDC.Folio = {instruction.ResultDteMapping.Folio} and ";
            con.Query += $"DDC.RutEmisor = '{participante.Rut}-{participante.VerificationCode}' ";


            DataTable dataTable = new DataTable();
            dataTable = DBConn.ConexionBdQuery(con);
            if (dataTable != null && dataTable.Rows.Count == 1)
            {
                dte.EnviadoSII = Convert.ToByte(dataTable.Rows[0].ItemArray[0]);
                dte.AceptadoSII = Convert.ToByte(dataTable.Rows[0].ItemArray[1]);
                dte.EnviadoCliente = Convert.ToByte(dataTable.Rows[0].ItemArray[2]);
                dte.AceptadoCliente = Convert.ToByte(dataTable.Rows[0].ItemArray[3]);
                dte.Motivo = dataTable.Rows[0].ItemArray[4].ToString();
                dte.IdSetDTESII = dataTable.Rows[0].ItemArray[5].ToString();
                dte.TrackID = Convert.ToUInt64(dataTable.Rows[0].ItemArray[6]);
                dte.XMLEnviado = Convert.ToUInt32(dataTable.Rows[0].ItemArray[7]);
                dte.FileEnviado = dataTable.Rows[0].ItemArray[8].ToString();
                dte.Archivo = dataTable.Rows[0].ItemArray[9].ToString();
                dte.FechaEnvioSII = Convert.ToDateTime(dataTable.Rows[0].ItemArray[10]);
                dte.FechaEnvioCliente = Convert.ToDateTime(dataTable.Rows[0].ItemArray[11]);
                dte.XMLBasico = Convert.ToUInt32(dataTable.Rows[0].ItemArray[12]);
                dte.FileBasico = dataTable.Rows[0].ItemArray[13].ToString();
                dte.OrigenDestino = dataTable.Rows[0].ItemArray[14].ToString();
                return dte;
            }
            return null;
        }
    }
}
