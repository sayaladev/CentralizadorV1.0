using System;
using System.Globalization;
using System.Text;
using System.Xml;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class NotaVenta
    {

        public static int GetLastNv(ResultInstruction instruction)
        {
            try
            {

                Conexion con = new Conexion(instruction.Creditor.ToString())
                {
                    Query = "select MAX(NVNumero) from softland.nw_nventa"
                };
                if (Conexion.ExecuteScalar(con) != null)
                {
                    return Convert.ToInt32(Conexion.ExecuteScalar(con));
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static int InsertNv(ResultInstruction instruction, int folioNV, string codProd)
        {
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo("es-CL");
            try
            {

                Conexion con = new Conexion(instruction.Creditor.ToString());
                StringBuilder query = new StringBuilder();
                string time = string.Format(cultureInfo, "{0:g}", DateTime.Now);
                uint neto = instruction.Amount;
                double iva = neto * 0.19;
                double total = Math.Ceiling(neto + iva);
                string concepto = $"Concepto: {instruction.AuxiliaryData.PaymentMatrixConcept}";
                
                query.Append("INSERT INTO softland.nw_nventa (CodAux,CveCod,NomCon,nvFeEnt,nvFem,NVNumero,nvObser,VenCod,nvSubTotal, ");
                query.Append("nvNetoAfecto,nvNetoExento,nvMonto,proceso,nvEquiv,CodMon,nvEstado) values ( ");
                query.Append($"'{instruction.ParticipantDebtor.Rut}','1','.','{time}','{time}',{folioNV}, '{concepto}', '1',{neto},{neto},0,{total}, ");
                query.Append("'Centralizador',1,'01','A') ");
                con.Query = query.ToString();
                if (Convert.ToInt32(Conexion.ExecuteNonQuery(con)) == 2) // 2 : Softland execute batch with 2 queries (nventa + log).
                {
                    query.Clear();
                    query.Append("INSERT INTO softland.nw_detnv (NVNumero,nvLinea,nvFecCompr,CodProd,nvCant,nvPrecio,nvSubTotal,nvTotLinea,CodUMed,CantUVta,nvEquiv)VALUES(");
                    query.Append($"{folioNV},1,'{time}','{codProd}',1,{neto},{neto},{neto},'UN',1,1)");
                    con.Query = query.ToString();
                    if (Convert.ToInt32(Conexion.ExecuteNonQuery(con)) == 1) // 1 : Softland execute only this query
                    {
                        query.Clear();
                        query.Append("INSERT INTO softland.NW_Impto (nvNumero, CodImpto, ValPctIni, AfectoImpto, Impto)  VALUES ( ");
                        query.Append($"{folioNV},'IVA',19,{neto},{iva})");
                        con.Query = query.ToString();
                        return Convert.ToInt32(Conexion.ExecuteNonQuery(con)); // Return 1 if ok!
                    }
                }
                return 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static int GetNv(ResultInstruction instruction)
        {
            try
            {

                Conexion con = new Conexion(instruction.Creditor.ToString());
                StringBuilder query = new StringBuilder();
                string concepto = $"Concepto: {instruction.AuxiliaryData.PaymentMatrixConcept}";

                query.Append("SELECT distinct TOP (1) nv.NVNumero from softland.nw_nventa nv ");
                query.Append("INNER JOIN softland.nw_detnv d on nv.NVNumero = d.NVNumero ");
                query.Append("left join softland.nw_fFactNCredNV() f on f.nvnumero = d.nvnumero and  f.codprod = d.codprod and f.nvcorrela = d.nvlinea ");
                query.Append($"where nv.CodAux = '{instruction.ParticipantDebtor.Rut}' and nv.nvSubTotal = {instruction.Amount} and f.folio is null");
                con.Query = query.ToString();
                return Convert.ToInt32(Conexion.ExecuteScalar(con));               
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
