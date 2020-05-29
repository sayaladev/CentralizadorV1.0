using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class Auxiliar
    {
        public string CodAux { get; set; }
        public string NomAux { get; set; }
        public string RutAux { get; set; }
        public string GirAux { get; set; }
        public string DirAux { get; set; }
        public string ComAux { get; set; }

        public static int InsertAuxiliar(ResultInstruction instruction, string acteco, Comuna comuna, Conexion conexion)
        {
            try
            {

                string rut = string.Format(CultureInfo.CurrentCulture, "{0:N0}", instruction.ParticipantDebtor.Rut).Replace(',', '.');
             
                StringBuilder query = new StringBuilder();

                query.Append($"IF (NOT EXISTS(SELECT * FROM softland.cwtauxi WHERE CodAux = '{instruction.ParticipantDebtor.Rut}')) BEGIN ");
                query.Append("INSERT INTO softland.CWTAUXI (CodAux, NomAux, NoFAux, RutAux, ActAux, GirAux, PaiAux, Comaux, ");
                query.Append("DirAux, ClaCli, ClaPro, Bloqueado, BloqueadoPro, EsReceptorDTE ,eMailDTE, Usuario, Proceso, Sistema, Region) ");
                query.Append($"VALUES ('{instruction.ParticipantDebtor.Rut}', ");
                query.Append($"'{instruction.ParticipantDebtor.BusinessName}','{instruction.ParticipantDebtor.Name}', ");
                query.Append($"'{rut}-{instruction.ParticipantDebtor.VerificationCode}','S',(select GirCod from softland.cwtgiro where GirDes = '{acteco}' ),'CL','{comuna.ComCod}', ");
                query.Append($"'{instruction.ParticipantDebtor.CommercialAddress}','S', 'S','N', 'N', 'S','{instruction.ParticipantDebtor.DteReceptionEmail}' ");
                query.Append($",'Softland','Centralizador', 'IW',{comuna.Id_Region}) END");
                conexion.Query = query.ToString();
                return Conexion.ExecuteNonQuery(conexion);
            }
            catch (Exception)
            {
                return 99;
            }
        }

        public static Auxiliar GetAuxiliar(ResultInstruction instruction, Conexion conexion)
        {
            try
            {

                StringBuilder query = new StringBuilder();


                query.Append($"SELECT * FROM softland.cwtauxi WHERE CodAux = '{instruction.ParticipantDebtor.Rut}' ");
                conexion.Query = query.ToString();
                DataTable dataTable = new DataTable();
                dataTable = Conexion.ExecuteReaderAsync(conexion).Result;
                if (dataTable != null && dataTable.Rows.Count == 1)
                {
                    Auxiliar auxiliar = new Auxiliar();
                    if (dataTable.Rows[0]["CodAux"] != DBNull.Value)
                    {
                        auxiliar.CodAux = dataTable.Rows[0]["CodAux"].ToString();
                    }
                    if (dataTable.Rows[0]["NomAux"] != DBNull.Value)
                    {
                        auxiliar.NomAux = dataTable.Rows[0]["NomAux"].ToString();
                    }
                    if (dataTable.Rows[0]["RutAux"] != DBNull.Value)
                    {
                        auxiliar.RutAux = dataTable.Rows[0]["RutAux"].ToString();
                    }
                    if (dataTable.Rows[0]["GirAux"] != DBNull.Value)
                    {
                        auxiliar.GirAux = dataTable.Rows[0]["GirAux"].ToString();
                    }
                    if (dataTable.Rows[0]["DirAux"] != DBNull.Value)
                    {
                        auxiliar.DirAux = dataTable.Rows[0]["DirAux"].ToString();
                    }
                    if (dataTable.Rows[0]["ComAux"] != DBNull.Value)
                    {
                        auxiliar.ComAux = dataTable.Rows[0]["ComAux"].ToString();
                    }

                    return auxiliar;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        public static int UpdateAuxiliar(ResultInstruction instruction, Conexion conexion)
        {

            try
            {
                StringBuilder query = new StringBuilder();

                query.Append($"UPDATE softland.cwtauxi SET NomAux='{instruction.ParticipantDebtor.BusinessName}', NoFAux='{instruction.ParticipantDebtor.Name}', ");
                query.Append($"eMailDTE='{instruction.ParticipantDebtor.DteReceptionEmail}' WHERE CodAux='{instruction.ParticipantDebtor.Rut}'");
                conexion.Query = query.ToString();
                return Conexion.ExecuteNonQuery(conexion);
            }
            catch (Exception)
            {
                return 99;
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
