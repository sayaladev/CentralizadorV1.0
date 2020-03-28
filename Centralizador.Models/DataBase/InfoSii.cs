using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class InfoSii
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


        public static IEnumerable<InfoSii> GetSendSiiByFolio(ResultInstruction instruction)
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
                yield break;
            }

            InfoSii dte = new InfoSii();
            DBaseConn con = new DBaseConn
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
            // con.Query += $"Where DSII.Tipo = 'F' and DSII.SubTipoDocto = 'T' AND DDC.Folio = {instruction.ResultDteM.Folio} and ";
            con.Query += $"DDC.RutEmisor = '{instruction.Participant.Rut}-{instruction.Participant.VerificationCode}' ";


            DataTable dataTable = new DataTable();
            dataTable = DBaseConn.ConexionBdQuery(con);
            if (dataTable != null)
            {
                foreach (DataRow item in dataTable.Rows)
                {
                    yield return new InfoSii
                    {
                        EnviadoSII = Convert.ToByte(dataTable.Rows[0].ItemArray[0]),
                        AceptadoSII = Convert.ToByte(dataTable.Rows[0].ItemArray[1]),
                        EnviadoCliente = Convert.ToByte(dataTable.Rows[0].ItemArray[2]),
                        AceptadoCliente = Convert.ToByte(dataTable.Rows[0].ItemArray[3]),
                        Motivo = dataTable.Rows[0].ItemArray[4].ToString(),
                        IdSetDTESII = dataTable.Rows[0].ItemArray[5].ToString(),
                        TrackID = Convert.ToUInt64(dataTable.Rows[0].ItemArray[6]),
                        XMLEnviado = Convert.ToUInt32(dataTable.Rows[0].ItemArray[7]),
                        FileEnviado = dataTable.Rows[0].ItemArray[8].ToString(),
                        Archivo = dataTable.Rows[0].ItemArray[9].ToString(),
                        FechaEnvioSII = Convert.ToDateTime(dataTable.Rows[0].ItemArray[10]),
                        FechaEnvioCliente = Convert.ToDateTime(dataTable.Rows[0].ItemArray[11]),
                        XMLBasico = Convert.ToUInt32(dataTable.Rows[0].ItemArray[12]),
                        FileBasico = dataTable.Rows[0].ItemArray[13].ToString(),
                        OrigenDestino = dataTable.Rows[0].ItemArray[14].ToString(),
                    };
                }
            }
        }
    }
}