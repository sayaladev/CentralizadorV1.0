using System;
using System.Data;
using System.Data.SqlClient;

namespace Centralizador.Models.DataBase
{
    public class DBConn
    {
        public string Cnn { get; set; }
        public string Query { get; set; }

        public static SqlDataReader SqlDataReader { get; set; }


        public static DataTable ConexionBdQuery(DBConn conn)
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
                    //Para nonquery: cmd.ExecuteNonQuery();
                    if (SqlDataReader.HasRows)
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(SqlDataReader);
                        return dataTable;
                    }
                }
                catch (Exception)
                {
                    //
                }
                finally
                {
                    SqlDataReader.Close();
                    cnn.Close();
                }
            }
            return null;
        }
    }
}
