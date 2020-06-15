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

        public static IList<Comuna> GetComunas(Conexion conexion)
        {
            IList<Comuna> comunas = new List<Comuna>();
            try
            {
                conexion.Query = "SELECT * FROM softland.cwtcomu";
                DataTable dataTable = new DataTable();
                dataTable = Conexion.ExecuteReaderAsync(conexion).Result;
                if (dataTable == null)
                {
                    return null;
                }
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
            }
            catch (Exception)
            {
                return null;
            }
            return comunas;
        }
    }
}
