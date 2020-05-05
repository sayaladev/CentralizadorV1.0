using System;
using System.Globalization;
using System.Linq;
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
                XmlDocument document = Properties.Settings.Default.DBSoftland;
                string DataBaseName = "";
                string ServerName = Properties.Settings.Default.ServerName;
                string id = Properties.Settings.Default.DBUser;
                string password = Properties.Settings.Default.DBPassword;

                foreach (XmlNode item in document.ChildNodes[0])
                {
                    if (item.Attributes["id"].Value == instruction.Creditor.ToString())
                    {
                        DataBaseName = item.FirstChild.InnerText;
                        break;
                    }
                }
                if (DataBaseName == null || ServerName == null || id == null || password == null)
                {
                    return 0;
                }
                Conexion con = new Conexion
                {
                    Cnn = $"Data Source={ServerName};Initial Catalog={DataBaseName};Persist Security Info=True;User ID={id};Password={password}"
                };
                con.Query = "select MAX(NVNumero) from softland.nw_nventa";
                if (Conexion.ExecuteScalar(con) != null )
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
                XmlDocument document = Properties.Settings.Default.DBSoftland;
                string DataBaseName = "";
                string ServerName = Properties.Settings.Default.ServerName;
                string id = Properties.Settings.Default.DBUser;
                string password = Properties.Settings.Default.DBPassword;

                foreach (XmlNode item in document.ChildNodes[0])
                {
                    if (item.Attributes["id"].Value == instruction.Creditor.ToString())
                    {
                        DataBaseName = item.FirstChild.InnerText;
                        break;
                    }
                }
                if (DataBaseName == null || ServerName == null || id == null || password == null)
                {
                    return 0;
                }
                Conexion con = new Conexion
                {
                    Cnn = $"Data Source={ServerName};Initial Catalog={DataBaseName};Persist Security Info=True;User ID={id};Password={password}"
                };
                StringBuilder query = new StringBuilder();
                string time = string.Format(cultureInfo, "{0:g}", DateTime.Now);
                uint neto = instruction.Amount;
                double iva = neto * 0.19;
                double total = Math.Ceiling(neto + iva);

                string concepto = $"Concepto: {instruction.AuxiliaryData.PaymentMatrixConcept}";
                query.Append("INSERT INTO softland.nw_nventa (CodAux,CveCod,NomCon,nvFeEnt,nvFem,NVNumero,nvObser,VenCod,nvSubTotal, ");
                query.Append("nvNetoAfecto,nvNetoExento,nvMonto,proceso,nvEquiv,CodMon,nvEstado) values ( ");
                query.Append($"'{instruction.ParticipantDebtor.Rut}','1','.','{time}','{time}',{folioNV}, '{concepto}', '1',{neto},{neto},0,{total}, ");
                query.Append("'Centralizador',1,'01','A')");
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

    }
}
