﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Centralizador.Models.ApiSII;

namespace Centralizador.Models.AppFunctions
{
    public class FileSii
    {
        public bool ExistsFile { get; set; }

        private static List<AuxCsv> AuxCsvsList { get; set; }

        public static string Path => $"ce_empresas_dwnld_{DateTime.Now.Year}{string.Format("{0:00}", DateTime.Now.Month)}{string.Format("{0:00}", DateTime.Now.Day)}.csv";

        public FileSii()
        {
            ExistsFile = GetFile();
        }

        private bool GetFile()
        {
            try
            {
                return File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + Path);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static AuxCsv GetAuxCvsFromFile(Detalle detalle)
        {
            AuxCsv auxCsv = new AuxCsv();
            try
            {
                // List<AuxCsv> res = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + Path).Skip(1).Select(v => AuxCsv.GetFronCsv(v)).ToList();
                auxCsv = AuxCsvsList.FirstOrDefault(x => x.Rut == detalle.Instruction.ParticipantDebtor.Rut + "-" + detalle.Instruction.ParticipantDebtor.VerificationCode);
                return auxCsv;
                //Task.Run(() =>
                //{
                //    List<AuxCsv> res = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + Path).Skip(1).Select(v => AuxCsv.GetFronCsv(v)).ToList();

                //    auxCsv = res.FirstOrDefault(x => x.Rut == detalle.Instruction.ParticipantDebtor.Rut + "-" + detalle.Instruction.ParticipantDebtor.VerificationCode);
                //    return auxCsv;

                //}).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
            //return null;
        }

        public static void GetValues()
        {
            AuxCsvsList = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + Path).Skip(1).Select(v => AuxCsv.GetFronCsv(v)).ToList();
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