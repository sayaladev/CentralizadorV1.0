using System;
using System.Collections.Generic;
using System.Data;

namespace Centralizador.Models.DataBase
{
    public class Comuna
    {

        public string ComCod { get; set; }
        public string ComDes { get; set; }
        public int Id_Region { get; set; }

        public static async System.Threading.Tasks.Task<IList<Comuna>> GetComunasAsync(Conexion conexion)
        {
            try
            {
                conexion.Query = "SELECT * FROM softland.cwtcomu";
                DataTable dataTable = new DataTable();
                dataTable = await Conexion.ExecuteReaderAsync(conexion);
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    IList<Comuna> comunas = new List<Comuna>();
                    foreach (DataRow item in dataTable.Rows)
                    {
                        Comuna comuna = new Comuna
                        {
                            ComCod = item["ComCod"].ToString(),
                            ComDes = item["ComDes"].ToString(),
                            Id_Region = Convert.ToInt32(item["Id_Region"])
                        };
                        comunas.Add(comuna);
                    }
                    return comunas;
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
