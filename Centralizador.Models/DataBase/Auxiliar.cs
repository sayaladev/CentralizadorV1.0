using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public  static async Task<Auxiliar> InsertAuxiliarAsync(ResultInstruction instruction, Conexion conexion, Comuna comuna)
        {
            string acteco = null;

            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            string adressTemp = ti.ToTitleCase(instruction.ParticipantDebtor.CommercialAddress.ToLower());
            if (instruction.ParticipantDebtor.CommercialAddress.Contains(','))
            {
                int index = instruction.ParticipantDebtor.CommercialAddress.IndexOf(',');
                adressTemp = instruction.ParticipantDebtor.CommercialAddress.Substring(0, index);
            }
            if (adressTemp.Length > 60) { adressTemp = adressTemp.Substring(0, 60); }
            // Get acteco from CEN
            if (instruction.ParticipantDebtor.CommercialBusiness != null)
            {
                if (instruction.ParticipantDebtor.CommercialBusiness.Length > 60)
                {
                    acteco = instruction.ParticipantDebtor.CommercialBusiness.Substring(0, 60);
                }
                else
                {
                    acteco = instruction.ParticipantDebtor.CommercialBusiness;
                }
                // Insert new Acteco
                await Acteco.InsertActecoAsync(acteco, conexion);

                // Production:
                if (Environment.MachineName == "DEVELOPER")
                {
                    time = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                }
            }
            try
            {
                string rut = string.Format(CultureInfo.CurrentCulture, "{0:N0}", instruction.ParticipantDebtor.Rut).Replace(',', '.');
                StringBuilder query = new StringBuilder();
                query.Append($"IF (NOT EXISTS(SELECT * FROM softland.cwtauxi WHERE CodAux = '{instruction.ParticipantDebtor.Rut}')) BEGIN ");
                query.Append("INSERT INTO softland.CWTAUXI (CodAux, NomAux, NoFAux, RutAux, ActAux, GirAux, PaiAux, Comaux, ");
                query.Append("DirAux, ClaCli, ClaPro, Bloqueado, BloqueadoPro, EsReceptorDTE ,eMailDTE, Usuario, Proceso, Sistema, Region, FechaUlMod) ");
                query.Append($"VALUES ('{instruction.ParticipantDebtor.Rut}', ");
                query.Append($"'{instruction.ParticipantDebtor.BusinessName}','{instruction.ParticipantDebtor.Name}', ");
                query.Append($"'{rut}-{instruction.ParticipantDebtor.VerificationCode}','S',(SELECT TOP 1 GirCod from softland.cwtgiro where GirDes = '{acteco}' ),'CL','{comuna.ComCod}', ");
                query.Append($"'{adressTemp}','S', 'S','N', 'N', 'S','{instruction.ParticipantDebtor.DteReceptionEmail}' ");
                query.Append($",'Softland','Centralizador', 'IW',{comuna.Id_Region}, '{time}') END");
                conexion.Query = query.ToString();
                //aux.DirAux = adressTemp;
                //aux.ComAux = comuna.ComDes;
                //await Conexion.ExecuteNonQueryAsync(conexion);
                if (await Conexion.ExecuteNonQueryAsync(conexion) == 2)
                {
                    return new Auxiliar()
                    {
                        DirAux = adressTemp,
                        ComAux = comuna.ComDes
                    };
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        public static async Task<Auxiliar> GetAuxiliarAsync(ResultInstruction instruction, Conexion conexion)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append($"SELECT * FROM softland.cwtauxi WHERE CodAux = '{instruction.ParticipantDebtor.Rut}' ");
                conexion.Query = query.ToString();
                DataTable dataTable = new DataTable();
                dataTable = await Conexion.ExecuteReaderAsync(conexion);
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

        public async Task<int> UpdateAuxiliarAsync(ResultInstruction instruction, Conexion conexion, Comuna comuna)
        {
            string name;
            string acteco = null;
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo("es-CL");
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            string time = string.Format(cultureInfo, "{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
            string adressTemp = ti.ToTitleCase(instruction.ParticipantDebtor.CommercialAddress.ToLower());
            if (instruction.ParticipantDebtor.CommercialAddress.Contains(','))
            {
                int index = instruction.ParticipantDebtor.CommercialAddress.IndexOf(',');
                adressTemp = instruction.ParticipantDebtor.CommercialAddress.Substring(0, index);
            }
            if (adressTemp.Length > 60) { adressTemp = adressTemp.Substring(0, 60); }

            // Address
            if (instruction.ParticipantDebtor.BusinessName.Length > 60)
            {
                name = instruction.ParticipantDebtor.BusinessName.Substring(0, 60);
            }
            else
            {
                name = instruction.ParticipantDebtor.BusinessName;
            }
            // Get acteco from CEN
            if (instruction.ParticipantDebtor.CommercialBusiness != null)
            {
                if (instruction.ParticipantDebtor.CommercialBusiness.Length > 60)
                {
                    acteco = instruction.ParticipantDebtor.CommercialBusiness.Substring(0, 60);
                }
                else
                {
                    acteco = instruction.ParticipantDebtor.CommercialBusiness;
                }
                // Insert new Acteco
                await Acteco.InsertActecoAsync(acteco, conexion);
            }

            // Production:
            if (Environment.MachineName == "DEVELOPER")
            {
                time = string.Format(cultureInfo, "{0:g}", DateTime.Now);
            }

            try
            {
                StringBuilder query = new StringBuilder();
                string rut = string.Format(CultureInfo.CurrentCulture, "{0:N0}", instruction.ParticipantDebtor.Rut).Replace(',', '.');
                query.Append($"UPDATE softland.cwtauxi SET NomAux='{name}', NoFAux='{instruction.ParticipantDebtor.Name}', ClaCli = 'S', ClaPro = 'S', Bloqueado = 'N', ");
                query.Append($"Comaux = '{comuna.ComCod}', GirAux = (SELECT TOP 1 GirCod from softland.cwtgiro where GirDes = '{acteco}' ), EsReceptorDTE = 'S', PaiAux='CL', ActAux='S', ");
                query.Append($"Usuario='Softland', Proceso='Centralizador', Sistema='IW', RutAux='{rut}-{instruction.ParticipantDebtor.VerificationCode}', FechaUlMod ='{time}', ");
                query.Append($"DirAux = '{adressTemp}', ");
                query.Append($"eMailDTE='{instruction.ParticipantDebtor.DteReceptionEmail}' WHERE CodAux='{instruction.ParticipantDebtor.Rut}'");
                conexion.Query = query.ToString();
                int res = await Conexion.ExecuteNonQueryAsync(conexion);
                return res;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string RemoveDiacritics(string text)
        {
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }





}
