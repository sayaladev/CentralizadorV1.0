using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Centralizador.Models.DataBase
{
    public class Conexion
    {
        public Conexion(string dataBaseName)
        {
            string serverName;
            if (Environment.MachineName == "DEVELOPER")
            {
                serverName = "DEVELOPER";
            }
            else
            {
                serverName = Properties.Settings.Default.DBServer;
            }
            DBName = dataBaseName;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = dataBaseName,
                UserID = Properties.Settings.Default.DBUser,
                Password = Properties.Settings.Default.DBPassword
            };
            Cnn = builder.ToString();
        }

        public string DBName { get; set; }
        public string Query { get; set; }
        private static SqlDataReader SqlDataReader { get; set; }
        private string Cnn { get; set; }

        /// <summary>
        /// INSERT / UPDATE / DELETE
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static async Task<object> ExecuteNonQueryAsync(Conexion conn)
        {
            using (SqlConnection cnn = new SqlConnection(conn.Cnn))
            {
                try
                {
                    // cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(conn.Query, cnn))
                    {
                        await cmd.Connection.OpenAsync();
                        object res = await cmd.ExecuteNonQueryAsync();
                        return res;
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
        public static async void ExecuteNonQueryAsyncTG(Conexion conn)
        {
            using (SqlConnection cnn = new SqlConnection(conn.Cnn))
            {
                try
                {
                    SqlCommand sqlCommand = cnn.CreateCommand();
                    //cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(conn.Query, cnn))
                    {
                        await cmd.Connection.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public static async Task<int> ExecuteNonQueryTranAsync(Conexion conn, List<string> listQ)
        {
            using (SqlConnection cnn = new SqlConnection(conn.Cnn))
            {
                await cnn.OpenAsync();
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
        /// SELECT
        /// </summary>
        /// <param name="conn"></param>
        /// <returns>CONJUNTO DE FILAS</returns>
        public static async Task<DataTable> ExecuteReaderAsync(Conexion conn)
        {
            using (SqlConnection cnn = new SqlConnection(conn.Cnn))
            {
                try
                {
                    //cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(conn.Query, cnn))
                    {
                        await cmd.Connection.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader != null)
                            {
                                DataTable dataTable = new DataTable();
                                dataTable.Load(reader);
                                return dataTable;
                            }
                            else
                            {
                                return null;
                            }
                            //SqlDataReader = await cmd.ExecuteReaderAsync();
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
                    //cnn.Open();
                    using (SqlCommand cmd = new SqlCommand(conn.Query, cnn))
                    {
                        await cmd.Connection.OpenAsync();
                        //using (SqlDataReader reader = await cmd.ExecuteScalarAsync())
                        //{
                        object obj = await cmd.ExecuteScalarAsync();
                        if (obj != null && DBNull.Value != obj)
                        {
                            return obj;
                        }
                        else
                        {
                            return null;
                        }
                        //}
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