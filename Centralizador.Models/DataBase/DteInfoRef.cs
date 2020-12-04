﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.AppFunctions;

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
        public IList<DteFiles> DteFiles { get; set; }

        #endregion

        public static IList<DteInfoRef> GetInfoRef(ResultInstruction instruction, Conexion conexion, string tipo)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                IList<DteInfoRef> lista = new List<DteInfoRef>();
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

                query.Append("SELECT G.NroInt ");
                query.Append("        ,G.Folio ");
                query.Append("        ,G.Fecha ");
                query.Append("        ,G.AuxDocNum ");
                query.Append("        ,G.AuxDocfec ");
                query.Append("        ,G.NetoAfecto ");
                query.Append("        ,G.NetoExento ");
                query.Append("        ,G.IVA ");
                query.Append("        ,G.Total ");
                query.Append("        ,G.nvnumero ");
                query.Append("        ,G.FecHoraCreacion ");
                query.Append("        ,G.FechaGenDTE ");
                query.Append("        ,R.LineaRef ");
                query.Append("        ,R.CodRefSII ");
                query.Append("        ,R.FolioRef ");
                query.Append("        ,R.FechaRef ");
                query.Append("        ,R.Glosa ");
                query.Append("        ,D.TipoDTE ");
                query.Append("        ,D.RUTRecep ");
                query.Append("        ,D.Archivo ");
                query.Append("        ,D.EnviadoSII ");
                query.Append("        ,D.FechaEnvioSII ");
                query.Append("        ,D.AceptadoSII ");
                query.Append("        ,D.EnviadoCliente ");
                query.Append("        ,D.FechaEnvioCliente ");
                query.Append("        ,D.AceptadoCliente ");
                query.Append("        ,D.Motivo ");
                query.Append("        ,D.IDSetDTECliente ");
                query.Append("        ,D.IDSetDTESII ");
                query.Append("        ,D.FirmaDTE ");
                query.Append("        ,D.IDXMLDoc ");
                query.Append("        ,D.TrackID ");
                query.Append("FROM softland.iw_gsaen G ");
                query.Append("LEFT JOIN softland.iw_gsaen_refdte R ON G.NroInt = R.NroInt ");
                query.Append("        AND G.Tipo = R.Tipo ");
                query.Append("        AND R.CodRefSII = 'SEN' ");
                query.Append("LEFT JOIN softland.dte_doccab D ON D.Tipo = G.Tipo ");
                query.Append("        AND D.NroInt = G.NroInt ");
                query.Append("        AND D.Folio = G.Folio ");
                query.Append($"WHERE G.NetoAfecto = {monto} ");
                query.Append($"        AND G.CodAux = '{instruction.ParticipantDebtor.Rut}' ");
                query.Append("        AND G.Estado = 'V' ");
                query.Append($"        AND G.FecHoraCreacion >= '{date}' ");
                query.Append($"        AND G.Tipo = '{tipo}' ");
                conexion.Query = query.ToString();
                dataTable = Conexion.ExecuteReaderAsync(conexion).Result;
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

        public static int InsertReference(ResultInstruction instruction, int nroInt, Conexion conexion)
        {
            // FALTA INSERT DE DTE_DocRef
            // Manipular el Xml  UPDATE
            try
            {
                StringBuilder query = new StringBuilder();
                CultureInfo cultureInfo = CultureInfo.GetCultureInfo("es-CL");
                //string date = string.Format(cultureInfo, "{0:yyyy-MM-dd HH:mm:ss}", instruction.PaymentMatrix.PublishDate);
                string date = instruction.PaymentMatrix.PublishDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                query.Append($"IF NOT EXISTS (SELECT * FROM softland.IW_GSaEn_RefDTE WHERE NroInt = {nroInt} ");
                query.Append("  AND Tipo = 'F' ");
                query.Append("  AND CodRefSII = 'SEN') ");
                query.Append("BEGIN ");
                query.Append("  INSERT INTO softland.IW_GSaEn_RefDTE (Tipo, NroInt, LineaRef, CodRefSII, FolioRef, FechaRef, Glosa) ");
                query.Append($"  VALUES ('F', {nroInt}, 2, 'SEN', '{instruction.PaymentMatrix.ReferenceCode}', '{date}', '{instruction.PaymentMatrix.NaturalKey}') ");
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

        public static int InsertReferenceTrans(Conexion conexion, Detalle detalle, ResultParticipant participant)
        {
            string date = null;
            StringBuilder query1 = new StringBuilder();
            StringBuilder query2 = new StringBuilder();
            string query3 = null;
            //string query4 = null;

            if (Environment.MachineName == "DEVELOPER")
            {
                // Developer
                date = detalle.Instruction.PaymentMatrix.PublishDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                date = detalle.Instruction.PaymentMatrix.PublishDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            // 1 softland.IW_GSaEn_RefDTE
            query1.Append($"IF NOT EXISTS (SELECT * FROM softland.IW_GSaEn_RefDTE WHERE NroInt = {detalle.NroInt} ");
            query1.Append("  AND Tipo = 'F' ");
            query1.Append("  AND CodRefSII = 'SEN') ");
            query1.Append("BEGIN ");
            query1.Append("  INSERT INTO softland.IW_GSaEn_RefDTE (Tipo, NroInt, LineaRef, CodRefSII, FolioRef, FechaRef, Glosa) ");
            query1.Append($"  VALUES ('F', {detalle.NroInt}, 2, 'SEN', '{detalle.Instruction.PaymentMatrix.ReferenceCode}', '{date}', '{detalle.Instruction.PaymentMatrix.NaturalKey}') ");
            query1.Append("  UPDATE softland.iw_gmovi ");
            query1.Append($" SET DetProd = '{detalle.Instruction.PaymentMatrix.NaturalKey}' ");
            query1.Append($" WHERE NroInt = {detalle.NroInt} ");
            query1.Append("  AND Tipo = 'F' ");
            query1.Append("END ");

            // 2 softland.DTE_DocRef
            query2.Append($"IF NOT EXISTS (SELECT * FROM softland.DTE_DocRef WHERE  Folio = {detalle.Folio} ");
            query2.Append("  AND TipoDTE = 33 ");
            query2.Append("  AND TpoDocRef = 'SEN') ");
            query2.Append("BEGIN ");
            query2.Append("  INSERT INTO softland.DTE_DocRef (RUTEmisor, TipoDTE, Folio, NroLinRef,TpoDocRef, FolioRef, FchRef, RazonRef) ");
            query2.Append($"  VALUES ('{participant.Rut}-{participant.VerificationCode}',33, {detalle.Folio}, 2, 'SEN', '{detalle.Instruction.PaymentMatrix.ReferenceCode}', '{date}', '{detalle.Instruction.PaymentMatrix.NaturalKey}') ");
            query2.Append("END ");


            // 3 Update File     
            DTEDefTypeDocumento dte = null;
            DTEDefTypeDocumentoReferencia[] references = null;
            DTEDefTypeDocumentoReferencia reference = null;

            try
            {

                if (detalle.DTEDef != null)
                {
                    dte = (DTEDefTypeDocumento)detalle.DTEDef.Item;
                    DTEDefTypeDocumentoReferencia[] listNew = null;
                    references = dte.Referencia;
                    if (references != null)
                    {
                        reference = references.FirstOrDefault(x => x.TpoDocRef.ToUpper() == "SEN");
                        if (reference == null)
                        {
                            // Update here
                            DTEDefTypeDocumentoReferencia referenciaCen = new DTEDefTypeDocumentoReferencia
                            {
                                NroLinRef = "2",
                                TpoDocRef = "SEN",
                                FolioRef = detalle.Instruction.PaymentMatrix.ReferenceCode, // DE04457A19C47
                                FchRef = detalle.Instruction.PaymentMatrix.PublishDate,
                                RazonRef = detalle.Instruction.PaymentMatrix.NaturalKey // SEN_[]
                            };
                            listNew = new List<DTEDefTypeDocumentoReferencia>() { references[0], referenciaCen }.ToArray();
                            dte.Referencia = listNew;
                            // Serialize To string
                            string result = ServicePdf.TransformObjectToXmlDte(detalle.DTEDef);
                            // PARA VERSIONES ANTIGUAS 
                            //string query = $"UPDATE softland.DTE_Archivos WHERE ID_Archivo = {1}";
                            query3 = $"UPDATE softland.DTE_Archivos SET Archivo = '{result}' WHERE Tipo = 'F' AND NroInt = {detalle.NroInt} AND Folio = {detalle.Folio} AND TipoXML = 'D' ";
                        }


                        // Update "SS"
                        //DteInfoRef infoLastF = detalle.DteInfoRef.OrderByDescending(x => x.Folio).First();
                        //string file = infoLastF.DteFiles.FirstOrDefault(x => x.TipoXML == "SS").Archivo;
                        //EnvioDTE res = ServicePdf.TransformStringDTEDefTypeToObjectDTE2(file);
                        //EnvioDTESetDTE ee = res.SetDTE;
                        //int c = 0;
                        //foreach (DTEDefType item in res.SetDTE.DTE)
                        //{
                        //    dte = (DTEDefTypeDocumento)item.Item;
                        //    if (dte.Encabezado.IdDoc.Folio == detalle.Folio.ToString())
                        //    {
                        //        references = dte.Referencia;
                        //        if (references != null)
                        //        {
                        //            reference = references.FirstOrDefault(x => x.TpoDocRef.ToUpper() == "SEN");
                        //            if (reference == null)
                        //            {
                        //                dte.Referencia = listNew;
                        //            }

                        //            //Update
                        //            ee.DTE[c] = item;
                        //            res.SetDTE = ee;

                        //            string resultado = ServicePdf.TransformObjectToXml2(res);
                        //            query4 = $"UPDATE softland.DTE_Archivos SET Archivo = '{resultado}' WHERE Tipo = 'F' AND NroInt = {detalle.NroInt} AND Folio = {detalle.Folio} AND TipoXML = 'SS' ";
                        //        }
                        //    }
                        //    c++;
                        //}


                    }

                }

                // Execute Transaction
                if (!string.IsNullOrEmpty(query1.ToString()) || !string.IsNullOrEmpty(query2.ToString()) || !string.IsNullOrEmpty(query3))
                {
                    int algo = Convert.ToInt32(Conexion.ExecuteNonQueryTranAsync(conexion, query1.ToString(), query2.ToString(), query3).Result);
                    //System.Windows.Forms.MessageBox.Show("RESULTADO QUERY :" + algo.ToString());
                    return algo;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("ALGUNA QUERY VACÍA!");
                }

            }
            catch (Exception)
            {
                throw;
            }

            return 0; // Error
        }

    }
}