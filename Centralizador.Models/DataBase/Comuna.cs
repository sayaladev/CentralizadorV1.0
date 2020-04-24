using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class Comuna
    {
      
            public string ComCod { get; set; }
            public string ComDes { get; set; }
            public int Id_Region { get; set; }

        public static IList<Comuna> GetComunas(ResultInstruction instruction)
        {

            try
            {
                XmlDocument document = Properties.Settings.Default.DBSoftland;
                string DataBaseName = "";
                string ServerName = Properties.Settings.Default.ServerName;
                string id = Properties.Settings.Default.DBUser;
                string password = Properties.Settings.Default.DBPassword;
                IList<Comuna> comunas = new List<Comuna>();
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
                    return null;
                }
                Conexion con = new Conexion
                {
                    Cnn = $"Data Source={ServerName};Initial Catalog={DataBaseName};Persist Security Info=True;User ID={id};Password={password}"
                };
                con.Query = "SELECT * FROM softland.cwtcomu";
                DataTable dataTable = new DataTable();
                dataTable = Conexion.ExecuteReader(con);
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
