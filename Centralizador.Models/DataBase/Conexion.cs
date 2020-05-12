﻿using System;
using System.Data;
using System.Data.SqlClient;

namespace Centralizador.Models.DataBase
{
    public class Conexion
    {
        private string Cnn { get; set; }
        public string Query { get; set; }
        private string DataBaseName { get; set; }
        private static SqlDataReader SqlDataReader { get; set; }

        public Conexion(string dataBaseName)
        {
            DataBaseName = dataBaseName;
            Cnn = Cnn = $"Data Source={Properties.Settings.Default.ServerName};Initial Catalog={DataBaseName};Persist Security Info=True;User ID={Properties.Settings.Default.DBUser};Password={Properties.Settings.Default.DBPassword}";

        }


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
