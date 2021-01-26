using Centralizador.Models.ApiSII;
using Centralizador.Models.Helpers;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Centralizador.Models.DataBase
{
    public class Comuna
    {
        public string ComCod { get; set; }
        public string ComDes { get; set; }
        public int Id_Region { get; set; }

        private static async Task<List<Comuna>> GetComunasAsync(Conexion conexion)
        {
            try
            {
                conexion.Query = "SELECT * FROM softland.cwtcomu";
                DataTable dataTable = new DataTable();
                dataTable = await Conexion.ExecuteReaderAsync(conexion);
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    List<Comuna> comunas = new List<Comuna>();
                    foreach (DataRow item in dataTable.Rows)
                    {
                        Comuna comuna = new Comuna
                        {
                            ComCod = item["ComCod"].ToString(),
                            ComDes = item["ComDes"].ToString(),
                            Id_Region = Convert.ToInt32(item["Id_Region"])
                        };
                        comunas.Add(comuna);
                    }
                    return comunas;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        public static async Task<Comuna> GetComunaFromInput(Detalle detalle, Conexion con, bool isNew)
        {
            string promptValue;
            Comuna comunaobj = null;
            List<Comuna> comunas = await GetComunasAsync(con);
            do
            {
                if (isNew)
                {
                    promptValue = ComunaInput.ShowDialog("New Auxiliar, be careful.",
                   $"'Comuna' not found, please input below:",
                   detalle.Instruction.ParticipantDebtor.BusinessName,
                   detalle.RutReceptor,
                   detalle.Instruction.ParticipantDebtor.CommercialAddress,
                   comunas);
                }
                else
                {
                    promptValue = ComunaInput.ShowDialog("Update Auxiliar, be careful.",
                                       $"'Comuna' not found, please input below:",
                                       detalle.Instruction.ParticipantDebtor.BusinessName,
                                       detalle.RutReceptor,
                                       detalle.Instruction.ParticipantDebtor.CommercialAddress,
                                       comunas);
                }

                //
                comunaobj = comunas.FirstOrDefault(x => Auxiliar.RemoveDiacritics(x.ComDes).ToLower() == Auxiliar.RemoveDiacritics(promptValue.ToLower()));
            } while (comunaobj == null);

            return await Task.FromResult(comunaobj);
        }

        internal static Task<Comuna> GetComunaFromInput(Detalle item, object con, bool v)
        {
            throw new NotImplementedException();
        }
    }
}