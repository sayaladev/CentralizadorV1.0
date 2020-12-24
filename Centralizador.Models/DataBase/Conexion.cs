using System;
using System.Collections.Generic;
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

        public string DBName { get; set; }


        /// <summary>
        /// Constructor Clase Conexión Softland
        /// </summary>
        /// <param name="dataBaseName"></param>
        /// <param name="dbUser"></param>
        /// <param name="dbPassword"></param>
        public Conexion(string dataBaseName)
        {
            // change server name 
            string serverName;           
            if (Environment.MachineName == "DEVELOPER")
            {
                serverName = "DEVELOPER";
            }
            else
            {
                serverName = Properties.Settings.Default.ServerName;
            }
            DBName = serverName;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = dataBaseName,
                UserID = Properties.Settings.Default.DBUser,
                Password = Properties.Settings.Default.DBPassword
            };
            Cnn = builder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <returns>CONJUNTO DE FILAS</returns>
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


        /// <summary>
        /// INSERT / UPDATE / DELETE
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
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
                            int res = await cmd.ExecuteNonQueryAsync();
                            return res;
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public static async void ExecuteNonQueryAsyncTG(Conexion conn)
        {
            using (SqlConnection cnn = new SqlConnection(conn.Cnn))
            {
                try
                {
                    SqlCommand sqlCommand = cnn.CreateCommand();
                    cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(conn.Query, cnn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception)
                {
                    //throw;
                }
            }
        }
        public static async Task<int> ExecuteNonQueryTranAsync(Conexion conn, List<string> listQ)
        {
            using (SqlConnection cnn = new SqlConnection(conn.Cnn))
            {
                cnn.Open();
                SqlCommand sqlCommand = cnn.CreateCommand();
                SqlTransaction sqlTransaction;

                // Start
                sqlTransaction = cnn.BeginTransaction("Centralizador");
                sqlCommand.Connection = cnn;
                sqlCommand.Transaction = sqlTransaction;

                try
                {
                    foreach (string item in listQ)
                    {
                        sqlCommand.CommandText = item;
                        await sqlCommand.ExecuteNonQueryAsync();
                    }
                    //sqlCommand.CommandText = q1;
                    //await sqlCommand.ExecuteNonQueryAsync();
                    //sqlCommand.CommandText = q2;
                    //await sqlCommand.ExecuteNonQueryAsync();
                    //sqlCommand.CommandText = q3;
                    //await sqlCommand.ExecuteNonQueryAsync();
                    //sqlCommand.CommandText = q4;
                    //await sqlCommand.ExecuteNonQueryAsync();

                    sqlTransaction.Commit();
                    return 1; // Success
                }
                catch (Exception)
                {
                    sqlTransaction.Rollback();
                    return 0;
                    throw;
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <returns>UN ÚNICO VALOR</returns>
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
