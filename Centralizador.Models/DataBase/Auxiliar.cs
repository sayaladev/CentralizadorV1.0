﻿using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class Auxiliar
    {
        public static int InsertAuxiliar(ResultInstruction instruction, string acteco, Comuna comuna)
        {
            try
            {
                XmlDocument document = Properties.Settings.Default.DBSoftland;
                string DataBaseName = "";
                string ServerName = Properties.Settings.Default.ServerName;
                string id = Properties.Settings.Default.DBUser;
                string password = Properties.Settings.Default.DBPassword;
                string rut = string.Format(CultureInfo.CurrentCulture, "{0:N0}", instruction.ParticipantDebtor.Rut).Replace(',', '.');

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
                    return -1;
                }
                Conexion con = new Conexion
                {
                    Cnn = $"Data Source={ServerName};Initial Catalog={DataBaseName};Persist Security Info=True;User ID={id};Password={password}"
                };
                StringBuilder query = new StringBuilder();
                query.Append($"IF (NOT EXISTS(SELECT * FROM softland.cwtauxi WHERE CodAux = '{instruction.ParticipantDebtor.Rut}')) BEGIN ");
                query.Append("INSERT INTO softland.CWTAUXI (CodAux, NomAux, NoFAux, RutAux, ActAux, GirAux, PaiAux, Comaux, ");
                query.Append("DirAux, ClaCli, ClaPro, Bloqueado, BloqueadoPro, EsReceptorDTE ,eMailDTE, Usuario, Proceso, Sistema, Region) ");
                query.Append($"VALUES ('{instruction.ParticipantDebtor.Rut}', ");
                query.Append($"'{instruction.ParticipantDebtor.BusinessName}','{instruction.ParticipantDebtor.Name}', ");
                query.Append($"'{rut}-{instruction.ParticipantDebtor.VerificationCode}','S',(select GirCod from softland.cwtgiro where GirDes = '{acteco}' ),'CL','{comuna.ComCod}', ");
                query.Append($"'{instruction.ParticipantDebtor.CommercialAddress}','S', 'S','N', 'N', 'S','{instruction.ParticipantDebtor.DteReceptionEmail}' ");
                query.Append($",'Softland','Centralizador', 'IW',{comuna.Id_Region}) END");
                con.Query = query.ToString();
                return Conexion.ExecuteNonQuery(con);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool GetAuxiliar(ResultInstruction instruction)
        {
            try
            {
                XmlDocument document = Properties.Settings.Default.DBSoftland;
                string DataBaseName = "";
                string ServerName = Properties.Settings.Default.ServerName;
                string id = Properties.Settings.Default.DBUser;
                string password = Properties.Settings.Default.DBPassword;

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
                    return false;
                }
                Conexion con = new Conexion
                {
                    Cnn = $"Data Source={ServerName};Initial Catalog={DataBaseName};Persist Security Info=True;User ID={id};Password={password}"
                };
                StringBuilder query = new StringBuilder();

                query.Append($"SELECT * FROM softland.cwtauxi WHERE CodAux = '{instruction.ParticipantDebtor.Rut}' ");
                con.Query = query.ToString();
                DataTable dataTable = new DataTable();
                dataTable = Conexion.ExecuteReader(con);
                if (dataTable != null)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }

        public static int UpdateAuxiliar(ResultInstruction instruction)
        {

            try
            {
                XmlDocument document = Properties.Settings.Default.DBSoftland;
                string DataBaseName = "";
                string ServerName = Properties.Settings.Default.ServerName;
                string id = Properties.Settings.Default.DBUser;
                string password = Properties.Settings.Default.DBPassword;

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
                    return -1;
                }
                Conexion con = new Conexion
                {
                    Cnn = $"Data Source={ServerName};Initial Catalog={DataBaseName};Persist Security Info=True;User ID={id};Password={password}"
                };
                StringBuilder query = new StringBuilder();

                query.Append($"UPDATE softland.cwtauxi SET NomAux='{instruction.ParticipantDebtor.BusinessName}', NoFAux='{instruction.ParticipantDebtor.Name}', ");
                query.Append($"eMailDTE='{instruction.ParticipantDebtor.DteReceptionEmail}' WHERE CodAux='{instruction.ParticipantDebtor.Rut}'");
                con.Query = query.ToString();
                return Conexion.ExecuteNonQuery(con);

            }
            catch (Exception)
            {
                throw;
            }          
        }
    }



    public class AuxCsv
    {
        public string Rut { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public static AuxCsv GetFronCsv(string csvLine)
        {
            try
            {
                string[] values = csvLine.Split(';');
                if (values.Count() == 6)
                {
                    AuxCsv aux = new AuxCsv
                    {
                        Rut = values[0],
                        Name = values[1],
                        Email = values[4]
                    };
                    return aux;
                }
                else
                {
                    AuxCsv aux = new AuxCsv
                    {
                        Rut = "0",
                        Name = "0",
                        Email = "0"
                    };
                    return aux;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

}
