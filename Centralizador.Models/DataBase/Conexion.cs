using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace Centralizador.Models.DataBase
{
    public class Conexion
    {
        private string Cnn { get; set; }
        public string Query { get; set; }
        private static SqlDataReader SqlDataReader { get; set; }

        public Conexion(string dataBaseName, string serverName, string dbUser, string dbPassword)
        {
            Cnn += $"Data Source={serverName};";
            Cnn += $"Initial Catalog={dataBaseName};";
            Cnn += $"Persist Security Info=True;";
            Cnn += $"User ID={dbUser};";
            Cnn += $"Password={dbPassword}";

        }


        public static async System.Threading.Tasks.Task<DataTable> ExecuteReaderAsync(Conexion conn)
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
                    SqlDataReader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
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
                    object obj = cmd.ExecuteScalar();
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
