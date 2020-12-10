using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class NotaVenta
    {
        /// <summary>
        /// Get 1 F° available of NV.
        /// </summary>
        /// <param name="conexion"></param>
        /// <returns></returns>
        public static int GetLastNv(Conexion conexion)
        {
            try
            {
                conexion.Query = "select MAX(NVNumero) + 1  from softland.nw_nventa";
                object result = Conexion.ExecuteScalarAsync(conexion).Result;
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }

            }
            catch (Exception)
            {
                throw;
            }
            return 0;
        }

        /// <summary>
        /// Get count of F° Availables of DTE.
        /// </summary>
        /// <param name="conexion"></param>
        /// <returns></returns>
        public static async Task<int> GetFoliosDisponiblesDTEAsync(Conexion conexion)
        {
            try
            {
                conexion.Query = "EXEC [softland].[DTE_FoliosDisp] @Tipo = N'F', @SubTipo = N'T'";
                DataTable dataTable = await Conexion.ExecuteReaderAsync(conexion);
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    return dataTable.Rows.Count;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return 0;
        }

        /// <summary>
        /// Delete all NV without DTE asociate.
        /// </summary>
        /// <param name="conexion"></param>
        /// <returns></returns>
        public static async Task<int> DeleteNvAsync(Conexion conexion)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.AppendLine("SELECT nv.nvnumero ");
                query.AppendLine("FROM softland.nw_nventa nv ");
                query.AppendLine("INNER JOIN softland.nw_detnv d ");
                query.AppendLine("ON   nv.nvnumero = d.nvnumero ");
                query.AppendLine("LEFT JOIN softland.nw_ffactncrednv() f ");
                query.AppendLine("ON   f.nvnumero = d.nvnumero ");
                query.AppendLine("     AND f.codprod = d.codprod ");
                query.AppendLine("     AND f.nvcorrela = d.nvlinea ");
                query.AppendLine("WHERE f.folio IS NULL ");
                query.AppendLine("ORDER BY nvnumero DESC");
                conexion.Query = query.ToString();
                DataTable dataTable = await Conexion.ExecuteReaderAsync(conexion);
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    var listQ = new List<string>();                    
                    foreach (DataRow item in dataTable.Rows)
                    {                       
                            string q = $"DELETE FROM softland.nw_nventa where NVNumero = {item["nvnumero"]}";
                            listQ.Add(q);                                           
                    }
                    var res = await Conexion.ExecuteNonQueryTranAsync(conexion, listQ);
                    return res; // 1 Success
                }
            }
            catch (Exception)
            {
                return 0;
                throw;
            }
            return 1;
        }

        /// <summary>
        /// Insert a NV & details
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="folioNV"></param>
        /// <param name="codProd"></param>
        /// <param name="conexion"></param>
        /// <returns></returns>
        public static async Task<int> InsertNvAsync(ResultInstruction instruction, int folioNV, string codProd, Conexion conexion)
        {           
            try
            {
                StringBuilder query = new StringBuilder();
          
                string date;
                if (Environment.MachineName == "DEVELOPER")
                {
                    // Developer
                    date = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                }
                else
                {
                    date = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }

                // HACER UNA TRANSACCIÓN!!!!!!!!!!!!!


                int neto = instruction.Amount;
                double iva = neto * 0.19;
                double total = Math.Ceiling(neto + iva);
                string concepto = $"Concepto: {instruction.AuxiliaryData.PaymentMatrixConcept}";

                query.Append("INSERT INTO softland.nw_nventa (CodAux,CveCod,NomCon,nvFeEnt,nvFem,NVNumero,nvObser,VenCod,nvSubTotal, ");
                query.Append("nvNetoAfecto,nvNetoExento,nvMonto,proceso,nvEquiv,CodMon,nvEstado,FechaHoraCreacion) values ( ");
                query.Append($"'{instruction.ParticipantDebtor.Rut}','1','.','{date}','{date}',{folioNV}, '{concepto}', '1',{neto},{neto},0,{total}, ");
                query.Append($"'Centralizador',1,'01','A','{date}') ");
                conexion.Query = query.ToString();
                if (Convert.ToInt32(await Conexion.ExecuteNonQueryAsync(conexion)) == 2) // 2 : Softland execute batch with 2 queries (nventa + log).
                {
                    query.Clear();
                    query.Append("INSERT INTO softland.nw_detnv (NVNumero,nvLinea,nvFecCompr,CodProd,nvCant,nvPrecio,nvSubTotal,nvTotLinea,CodUMed,CantUVta,nvEquiv)VALUES(");
                    query.Append($"{folioNV},1,'{date}','{codProd}',1,{neto},{neto},{neto},'UN',1,1)");
                    conexion.Query = query.ToString();
                    if (Convert.ToInt32(await Conexion.ExecuteNonQueryAsync(conexion)) == 1) // 1 : Softland execute only this query
                    {
                        query.Clear();
                        query.Append("INSERT INTO softland.NW_Impto (nvNumero, CodImpto, ValPctIni, AfectoImpto, Impto)  VALUES ( ");
                        query.Append($"{folioNV},'IVA',19,{neto},{iva})");
                        conexion.Query = query.ToString();
                        return Convert.ToInt32(await Conexion.ExecuteNonQueryAsync(conexion)); // Return 1 if ok!     
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return 0;
        }

        /// <summary>
        /// Check if Insert NV.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="conexion"></param>
        /// <returns></returns>
        public static async Task<int> GetNvIfExistsAsync(ResultInstruction instruction, Conexion conexion)
        {
            string date;
            if (Environment.MachineName == "DEVELOPER")
            {
                // Developer
                date = instruction.PaymentMatrix.PublishDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                date = instruction.PaymentMatrix.PublishDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            StringBuilder query = new StringBuilder();
            query.AppendLine("SELECT DISTINCT top (1) nv.nvnumero ");
            query.AppendLine("FROM softland.nw_nventa nv ");
            query.AppendLine("INNER JOIN softland.nw_detnv d ");
            query.AppendLine("ON   nv.nvnumero = d.nvnumero ");
            query.AppendLine("LEFT JOIN softland.nw_ffactncrednv() f ");
            query.AppendLine("ON   f.nvnumero = d.nvnumero ");
            query.AppendLine("     AND f.codprod = d.codprod ");
            query.AppendLine("     AND f.nvcorrela = d.nvlinea ");
            query.AppendLine($"WHERE nv.codaux = '{instruction.ParticipantDebtor.Rut}' ");
            query.AppendLine($"    AND nv.nvsubtotal = {instruction.Amount} ");
            query.AppendLine("     AND f.folio IS NULL ");
            query.AppendLine($"    AND nv.fechahoracreacion >= '{date}'");

            try
            {
                conexion.Query = query.ToString();
                object result = await Conexion.ExecuteScalarAsync(conexion);
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return 0;
        }
    }
}
