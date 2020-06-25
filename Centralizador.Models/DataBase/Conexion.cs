using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Centralizador.Models.DataBase
{
    public class Conexion
    {
        private string Cnn { get; set; }
        public string Query { get; set; }
        private static SqlDataReader SqlDataReader { get; set; }


        /// <summary>
        /// Constructor Clase Conexión Softland
        /// </summary>
        /// <param name="dataBaseName"></param>
        /// <param name="dbUser"></param>
        /// <param name="dbPassword"></param>
        public Conexion(string dataBaseName, string dbUser, string dbPassword)
        {
            // change user name 
            string serverName;
            if (Environment.MachineName == "DEVELOPER")
            {
                serverName = "DEVELOPER";
            }
            else
            {
                serverName = Properties.Settings.Default.ServerName;
            }
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = dataBaseName,
                UserID = dbUser,
                Password = dbPassword
            };
            Cnn = builder.ToString();
        }


        public static async Task<DataTable> ExecuteReaderAsync(Conexion conn)
        {   
            using (SqlConnection cnn = new SqlConnection(conn.Cnn))
            {
                try
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(conn.Query, cnn))
                    {
                        using (SqlDataReader)
                        {
                            SqlDataReader = await cmd.ExecuteReaderAsync();
                            DataTable dataTable = new DataTable();
                            dataTable.Load(SqlDataReader);
                            return dataTable;
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }            
        }
        public static async Task<int> ExecuteNonQueryAsync(Conexion conn)
        {
            using (SqlConnection cnn = new SqlConnection(conn.Cnn))
            {
                try
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(conn.Query, cnn))
                    {
                        using (SqlDataReader)
                        {
                            SqlDataReader.Close();
                            return await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


        public static async Task<object> ExecuteScalarAsync(Conexion conn)
        {
            using (SqlConnection cnn = new SqlConnection(conn.Cnn))
            {
                try
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(conn.Query, cnn))
                    {
                        using (SqlDataReader)
                        {
                            object obj = await cmd.ExecuteScalarAsync();
                            if (obj != null && DBNull.Value != obj)
                            {                            
                                return obj;
                            }
                            else
                            {                           
                                return null;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
