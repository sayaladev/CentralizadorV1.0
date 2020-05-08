using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace Centralizador.Models.DataBase
{
    public class Conexion
    {
        public string Cnn { get; set; }
        public string Query { get; set; }

        public Conexion(string id)
        {
            XmlDocument document = Properties.Settings.Default.DBSoftland;
            string dataBaseName = "";
            foreach (XmlNode item in document.ChildNodes[0])
            {
                if (item.Attributes["id"].Value == id)
                {
                    dataBaseName = item.FirstChild.InnerText;
                    break;
                }
            }

            Cnn = Cnn = $"Data Source={Properties.Settings.Default.ServerName};Initial Catalog={dataBaseName};Persist Security Info=True;User ID={Properties.Settings.Default.DBUser};Password={Properties.Settings.Default.DBPassword}";
           
        }

        public static SqlDataReader SqlDataReader { get; set; }


        public static DataTable ExecuteReader(Conexion conn)
        {
            using (SqlConnection cnn = new SqlConnection(conn.Cnn))
            {
                try
                {
                    cnn.Open();
                    SqlCommand cmd = new SqlCommand
                    {
                        CommandTimeout = 900000,
                        Connection = cnn,
                        CommandText = conn.Query,
                        CommandType = CommandType.Text
                    };
                    SqlDataReader = cmd.ExecuteReader();     
                    if (SqlDataReader.HasRows)
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(SqlDataReader);
                        return dataTable;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    SqlDataReader.Close();
                    cnn.Close();
                }
            }
            return null;
        }

        public static int ExecuteNonQuery(Conexion conn)
        {
            using (SqlConnection cnn = new SqlConnection(conn.Cnn))
            {
                try
                {
                    cnn.Open();
                    SqlCommand cmd = new SqlCommand
                    {
                        CommandTimeout = 900000,
                        Connection = cnn,
                        CommandText = conn.Query,
                        CommandType = CommandType.Text
                    };                   
                    return cmd.ExecuteNonQuery();               
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    SqlDataReader.Close();
                    cnn.Close();
                }
            }          
        }

        public static object ExecuteScalar(Conexion conn)
        {
            using (SqlConnection cnn = new SqlConnection(conn.Cnn))
            {
                try
                {
                    cnn.Open();
                    SqlCommand cmd = new SqlCommand
                    {
                        CommandTimeout = 900000,
                        Connection = cnn,
                        CommandText = conn.Query,
                        CommandType = CommandType.Text
                    };
                    object obj  = cmd.ExecuteScalar();
                    if (obj != null && DBNull.Value != obj)
                    {
                        return obj;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
                finally
                {
                    SqlDataReader.Close();
                    cnn.Close();
                }
            }
        }
    }
}
