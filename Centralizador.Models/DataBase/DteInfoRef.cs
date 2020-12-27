using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class DteInfoRef
    {
        #region Properties

        public int NroInt { get; set; }
        public int Folio { get; set; }
        public DateTime Fecha { get; set; }
        public int AuxDocNum { get; set; }
        public DateTime AuxDocfec { get; set; }
        public int NetoAfecto { get; set; }
        public int NetoExento { get; set; }
        public int IVA { get; set; }
        public int Total { get; set; }
        public int Nvnumero { get; set; }
        public DateTime FecHoraCreacion { get; set; }
        public DateTime FechaGenDTE { get; set; }
        public int LineaRef { get; set; }
        public string CodRefSII { get; set; }
        public string FolioRef { get; set; }
        public DateTime FechaRef { get; set; }
        public string Glosa { get; set; }
        public int TipoDTE { get; set; }
        public string RUTRecep { get; set; }
        public string Archivo { get; set; }
        public int EnviadoSII { get; set; }
        public DateTime FechaEnvioSII { get; set; }
        public int AceptadoSII { get; set; }
        public int EnviadoCliente { get; set; }
        public DateTime FechaEnvioCliente { get; set; }
        public int AceptadoCliente { get; set; }
        public string Motivo { get; set; }
        public string IDSetDTECliente { get; set; }
        public string IDSetDTESII { get; set; }
        public string FirmaDTE { get; set; }
        public int IDXMLDoc { get; set; }
        public string TrackID { get; set; }
        public List<DteFiles> DteFiles { get; set; }

        #endregion Properties

        public static void InsertTriggerRefCen(Conexion conexion)
        {
            try
            {
                conexion.Query = $"USE [{conexion.DBName}] { Properties.Resources.sql_insert_Trigger}";
                Conexion.ExecuteNonQueryAsyncTG(conexion);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<List<DteInfoRef>> GetInfoRefAsync(ResultInstruction instruction, Conexion conexion, string tipo)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                List<DteInfoRef> lista = new List<DteInfoRef>();
                DataTable dataTable = new DataTable();
                int monto = 0;
                string date = null;
                if (Environment.MachineName == "DEVELOPER")
                {
                    // Developer
                    date = instruction.PaymentMatrix.PublishDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                }
                else
                {
                    date = instruction.PaymentMatrix.PublishDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }

                if (tipo == "NC")
                {
                    monto = instruction.Amount * -1;
                }
                else if (tipo == "F")
                {
                    monto = instruction.Amount;
                }
                query.AppendLine("SELECT    g.nroint, ");
                query.AppendLine("          g.folio, ");
                query.AppendLine("          g.fecha, ");
                query.AppendLine("          ( SELECT folio ");
                query.AppendLine("          FROM    softland.iw_gsaen ");
                query.AppendLine("          WHERE   tipo = 'N' ");
                query.AppendLine("                  AND auxdocnum = g.folio) AS auxdocnum, ");
                query.AppendLine("          ( SELECT fecha ");
                query.AppendLine("          FROM    softland.iw_gsaen ");
                query.AppendLine("          WHERE   tipo = 'N' ");
                query.AppendLine("                  AND auxdocnum = g.folio) AS auxdocfec, ");
                query.AppendLine("          g.netoafecto, ");
                query.AppendLine("          g.netoexento, ");
                query.AppendLine("          g.iva, ");
                query.AppendLine("          g.total, ");
                query.AppendLine("          g.nvnumero, ");
                query.AppendLine("          g.fechoracreacion, ");
                query.AppendLine("          g.fechagendte, ");
                query.AppendLine("          r.linearef, ");
                query.AppendLine("          r.codrefsii, ");
                query.AppendLine("          r.folioref, ");
                query.AppendLine("          r.fecharef, ");
                query.AppendLine("          r.glosa, ");
                query.AppendLine("          d.tipodte, ");
                query.AppendLine("          d.rutrecep, ");
                query.AppendLine("          d.archivo, ");
                query.AppendLine("          d.enviadosii, ");
                query.AppendLine("          d.fechaenviosii, ");
                query.AppendLine("          d.aceptadosii, ");
                query.AppendLine("          d.enviadocliente, ");
                query.AppendLine("          d.fechaenviocliente, ");
                query.AppendLine("          d.aceptadocliente, ");
                query.AppendLine("          d.motivo, ");
                query.AppendLine("          d.idsetdtecliente, ");
                query.AppendLine("          d.idsetdtesii, ");
                query.AppendLine("          d.firmadte, ");
                query.AppendLine("          d.idxmldoc, ");
                query.AppendLine("          d.trackid ");
                query.AppendLine("FROM      softland.iw_gsaen g ");
                query.AppendLine("LEFT JOIN softland.iw_gsaen_refdte r ");
                query.AppendLine("ON        g.nroint = r.nroint ");
                query.AppendLine("          AND g.tipo = r.tipo ");
                query.AppendLine("          AND r.codrefsii = 'SEN' ");
                query.AppendLine("LEFT JOIN softland.dte_doccab d ");
                query.AppendLine("ON        d.tipo = g.tipo ");
                query.AppendLine("          AND d.nroint = g.nroint ");
                query.AppendLine("          AND d.folio = g.folio ");
                query.AppendLine($"WHERE     g.netoafecto = {monto} ");
                if (instruction.ParticipantNew == null)
                {
                    query.AppendLine($"          AND g.codaux = '{instruction.ParticipantDebtor.Rut}' ");
                }
                else
                {
                    query.AppendLine($"          AND g.codaux IN ( '{instruction.ParticipantDebtor.Rut}', '{instruction.ParticipantNew.Rut}' )");
                }

                query.AppendLine("          AND g.estado = 'V' ");
                query.AppendLine($"          AND g.fechoracreacion >= '{date}' ");
                query.AppendLine($"          AND g.tipo = '{tipo}' ");
                query.AppendLine("ORDER BY  g.folio DESC");

                conexion.Query = query.ToString();
                dataTable = await Conexion.ExecuteReaderAsync(conexion);
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    foreach (DataRow item in dataTable.Rows)
                    {
                        DteInfoRef reference = new DteInfoRef();
                        if (item[0] != DBNull.Value) { reference.NroInt = Convert.ToInt32(item[0]); }
                        if (item[1] != DBNull.Value) { reference.Folio = Convert.ToInt32(item[1]); }
                        if (item[2] != DBNull.Value) { reference.Fecha = Convert.ToDateTime(item[2]); }
                        if (item[3] != DBNull.Value) { reference.AuxDocNum = Convert.ToInt32(item[3]); }
                        if (item[4] != DBNull.Value) { reference.AuxDocfec = Convert.ToDateTime(item[4]); }
                        if (item[5] != DBNull.Value) { reference.NetoAfecto = Convert.ToInt32(item[5]); }
                        if (item[6] != DBNull.Value) { reference.NetoExento = Convert.ToInt32(item[6]); }
                        if (item[7] != DBNull.Value) { reference.IVA = Convert.ToInt32(item[7]); }
                        if (item[8] != DBNull.Value) { reference.Total = Convert.ToInt32(item[8]); }
                        if (item[9] != DBNull.Value) { reference.Nvnumero = Convert.ToInt32(item[9]); }
                        if (item[10] != DBNull.Value) { reference.FecHoraCreacion = Convert.ToDateTime(item[10]); }
                        if (item[11] != DBNull.Value) { reference.FechaGenDTE = Convert.ToDateTime(item[11]); }
                        if (item[12] != DBNull.Value) { reference.LineaRef = Convert.ToInt32(item[12]); }
                        if (item[13] != DBNull.Value) { reference.CodRefSII = Convert.ToString(item[13]); }
                        if (item[14] != DBNull.Value) { reference.FolioRef = Convert.ToString(item[14]); }
                        if (item[15] != DBNull.Value) { reference.FechaRef = Convert.ToDateTime(item[15]); }
                        if (item[16] != DBNull.Value) { reference.Glosa = Convert.ToString(item[16]); }
                        if (item[17] != DBNull.Value) { reference.TipoDTE = Convert.ToInt32(item[17]); }
                        if (item[18] != DBNull.Value) { reference.RUTRecep = Convert.ToString(item[18]); }
                        if (item[19] != DBNull.Value) { reference.Archivo = Convert.ToString(item[19]); }
                        if (item[20] != DBNull.Value) { reference.EnviadoSII = Convert.ToInt32(item[20]); }
                        if (item[21] != DBNull.Value) { reference.FechaEnvioSII = Convert.ToDateTime(item[21]); }
                        if (item[22] != DBNull.Value) { reference.AceptadoSII = Convert.ToInt32(item[22]); }
                        if (item[23] != DBNull.Value) { reference.EnviadoCliente = Convert.ToInt32(item[23]); }
                        if (item[24] != DBNull.Value) { reference.FechaEnvioCliente = Convert.ToDateTime(item[24]); }
                        if (item[25] != DBNull.Value) { reference.AceptadoCliente = Convert.ToInt32(item[25]); }
                        if (item[26] != DBNull.Value) { reference.Motivo = Convert.ToString(item[26]); }
                        if (item[27] != DBNull.Value) { reference.IDSetDTECliente = Convert.ToString(item[27]); }
                        if (item[28] != DBNull.Value) { reference.IDSetDTESII = Convert.ToString(item[28]); }
                        if (item[29] != DBNull.Value) { reference.FirmaDTE = Convert.ToString(item[29]); }
                        if (item[30] != DBNull.Value) { reference.IDXMLDoc = Convert.ToInt32(item[30]); }
                        if (item[31] != DBNull.Value) { reference.TrackID = Convert.ToString(item[31]); }

                        // SEARCH & ATTACH FILES REF.
                        List<DteFiles> files = await DataBase.DteFiles.GetDteFilesAsync(conexion, reference.NroInt, reference.Folio);
                        if (files != null)
                        {
                            reference.DteFiles = files;
                        }
                        else
                        {
                            files = await DataBase.DteFiles.GetDteFilesAsync(conexion, reference.IDXMLDoc);
                            reference.DteFiles = files;
                        }

                        lista.Add(reference);
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