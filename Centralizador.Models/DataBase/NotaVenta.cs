using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class NotaVenta
    {

        public static int GetLastNv(Conexion conexion)
        {
            try
            {
                conexion.Query = "select MAX(NVNumero) from softland.nw_nventa";
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

        public static async Task<int> CheckFoliosAsync(Conexion conexion)
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

        public static int InsertNv(ResultInstruction instruction, int folioNV, string codProd, Conexion conexion)
        {
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo("es-CL");
            try
            {
                StringBuilder query = new StringBuilder();
                //string time = string.Format(cultureInfo, "{0:g}", DateTime.Now);
                // Insertó 2020-09-06 17:28:00.000

                // Prueba
                string time = string.Format(cultureInfo, "{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);

                if (Environment.MachineName == "DEVELOPER")
                {
                    time = string.Format(cultureInfo, "{0:g}", DateTime.Now);
                }

                int neto = instruction.Amount;
                double iva = neto * 0.19;
                double total = Math.Ceiling(neto + iva);
                string concepto = $"Concepto: {instruction.AuxiliaryData.PaymentMatrixConcept}";

                query.Append("INSERT INTO softland.nw_nventa (CodAux,CveCod,NomCon,nvFeEnt,nvFem,NVNumero,nvObser,VenCod,nvSubTotal, ");
                query.Append("nvNetoAfecto,nvNetoExento,nvMonto,proceso,nvEquiv,CodMon,nvEstado,FechaHoraCreacion) values ( ");
                query.Append($"'{instruction.ParticipantDebtor.Rut}','1','.','{time}','{time}',{folioNV}, '{concepto}', '1',{neto},{neto},0,{total}, ");
                query.Append($"'Centralizador',1,'01','A','{time}') ");
                conexion.Query = query.ToString();
                if (Convert.ToInt32(Conexion.ExecuteNonQueryAsync(conexion).Result) == 2) // 2 : Softland execute batch with 2 queries (nventa + log).
                {
                    query.Clear();
                    query.Append("INSERT INTO softland.nw_detnv (NVNumero,nvLinea,nvFecCompr,CodProd,nvCant,nvPrecio,nvSubTotal,nvTotLinea,CodUMed,CantUVta,nvEquiv)VALUES(");
                    query.Append($"{folioNV},1,'{time}','{codProd}',1,{neto},{neto},{neto},'UN',1,1)");
                    conexion.Query = query.ToString();
                    if (Convert.ToInt32(Conexion.ExecuteNonQueryAsync(conexion).Result) == 1) // 1 : Softland execute only this query
                    {
                        query.Clear();
                        query.Append("INSERT INTO softland.NW_Impto (nvNumero, CodImpto, ValPctIni, AfectoImpto, Impto)  VALUES ( ");
                        query.Append($"{folioNV},'IVA',19,{neto},{iva})");
                        conexion.Query = query.ToString();
                        return Convert.ToInt32(Conexion.ExecuteNonQueryAsync(conexion).Result); // Return 1 if ok!     
                    }
                }            
            }
            catch (Exception)
            {
                throw;
            }
            return 0;
        }

        public static int GetNvIfExists(ResultInstruction instruction, Conexion conexion)
        {

            StringBuilder query = new StringBuilder();
            query.Append("SELECT DISTINCT TOP (1) ");
            query.Append("  nv.NVNumero ");
            query.Append("FROM softland.nw_nventa nv ");
            query.Append("INNER JOIN softland.nw_detnv d ");
            query.Append("  ON nv.NVNumero = d.NVNumero ");
            query.Append("LEFT JOIN softland.nw_fFactNCredNV() f ");
            query.Append("  ON f.nvnumero = d.nvnumero ");
            query.Append("  AND f.codprod = d.codprod ");
            query.Append("  AND f.nvcorrela = d.nvlinea ");
            query.Append($"WHERE nv.CodAux = '{instruction.ParticipantDebtor.Rut}' ");
            query.Append($"AND nv.nvSubTotal = {instruction.Amount} ");
            query.Append("AND f.folio IS NULL ");

            try
            {
                conexion.Query = query.ToString();
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
    }
}
