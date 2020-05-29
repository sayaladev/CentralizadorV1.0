using System;
using System.Collections.Generic;
using System.Data;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class Comuna
    {

        public string ComCod { get; set; }
        public string ComDes { get; set; }
        public int Id_Region { get; set; }

        public static IList<Comuna> GetComunas(Conexion conexion)
        {
            try
            {
                IList<Comuna> comunas = new List<Comuna>();
                conexion.Query = "SELECT * FROM softland.cwtcomu";

                DataTable dataTable = new DataTable();
                dataTable = Conexion.ExecuteReaderAsync(conexion).Result;
                if (dataTable != null)
                {
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
                return comunas;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
