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

        public Conexion(string dataBaseName,string dbUser, string dbPassword)
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
            Cnn += $"Data Source={serverName};";
            Cnn += $"Initial Catalog={dataBaseName};"; // null?
            Cnn += $"Persist Security Info=True;";
            Cnn += $"User ID={dbUser};";
            Cnn += $"Password={dbPassword}";

        }


        public static async Task<DataTable> ExecuteReaderAsync(Conexion conn)
        {
            using (SqlConnection cnn = new SqlConnection(conn.Cnn))
            {
                try
                {
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(conn.Query, cnn ))
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
                    return null; // Error server
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
                    SqlCommand cmd = new SqlCommand
                    {
                        CommandTimeout = 900000,
                        Connection = cnn,
                        CommandText = conn.Query,
                        CommandType = CommandType.Text
                    };
                    SqlDataReader.Close();   
                    return await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception)
                {
                    return 99; // Error
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
                    SqlCommand cmd = new SqlCommand
                    {
                        CommandTimeout = 900000,
                        Connection = cnn,
                        CommandText = conn.Query,
                        CommandType = CommandType.Text
                    };
                    object obj = await cmd.ExecuteScalarAsync();
                    if (obj != null && DBNull.Value != obj)
                    {
                        SqlDataReader.Close();   
                        return obj;
                    }
                    else
                    {
                        SqlDataReader.Close();      
                        return null;
                    }
                }
                catch (Exception)
                {
                    return 99; // Error
                }
            }
        }
    }
}
