using System;
using System.Drawing;
using System.Linq;

using Centralizador.Models.ApiSII;

namespace Centralizador.Models.AppFunctions
{
    public class ValidatorFlag
    {
        //Reference (DTEDefTypeDocumentoReferencia)
        public bool NroLinRef { get; set; }
        public bool TpoDocRef { get; set; }
        public bool FolioRef { get; set; }
        public bool FchRef { get; set; }
        public bool RazonRef { get; set; }

        // DscItem (DTEDefTypeDocumentoDetalle)
        public bool DscItem { get; set; }

        // FmaPago (DTEDefTypeDocumentoEncabezadoIdDoc)
        public bool FmaPago { get; set; }

        // Flag Color
        public LetterFlag Flag { get; set; }

        // Constructor
        public ValidatorFlag(Detalle detalle, bool isCreditor)
        {
            ValidateCen(detalle, isCreditor);
        }

        public ValidatorFlag()
        {
        }



        // Functions
        public static int GetFlagImageIndex(LetterFlag flag)
        {

            switch (flag)
            {
                case LetterFlag.Red:
                    return 11;
                case LetterFlag.Blue:
                    return 12;
                case LetterFlag.Yellow:
                    return 13;
                case LetterFlag.Green:
                    return 14;
                case LetterFlag.Complete:
                    return 15;
                default:
                    return 16;
            }
        }
        public static Color GetFlagBackColor(LetterFlag flag)
        {
            switch (flag)
            {
                case LetterFlag.Red:
                    return Color.FromArgb(207, 93, 96);
                case LetterFlag.Blue:
                    return Color.FromArgb(92, 131, 180);
                case LetterFlag.Yellow:
                    return Color.FromArgb(255, 193, 96);
                case LetterFlag.Green:
                    return Color.FromArgb(139, 180, 103);
                case LetterFlag.Complete:
                    return Color.White;
                default:
                    return Color.Empty;
            }
        }
        private void ValidateCen(Detalle detalle, bool isCreditor)
        {
            try
            {
                if (detalle.Folio > 0 && detalle.IsParticipant) // Facturada
                {
                    // Set Default.
                    Flag = LetterFlag.Green;
                    if (detalle.DTEDef != null)
                    {
                        DTEDefTypeDocumento dte = (DTEDefTypeDocumento)detalle.DTEDef.Item;
                        // Valide FmaPago
                        if (dte.Encabezado.IdDoc.FmaPago != DTEDefTypeDocumentoEncabezadoIdDocFmaPago.Crédito)
                        {
                            Flag = LetterFlag.Yellow;
                            FmaPago = true;
                        }

                        // Valide Reference
                        if (dte.Referencia != null)
                        {
                            DTEDefTypeDocumentoReferencia referencia = dte.Referencia.FirstOrDefault(x => x.TpoDocRef.ToUpper() == "SEN");
                            if (referencia != null)
                            {
                                // Valide NroLinRef
                                if (string.IsNullOrEmpty(referencia.NroLinRef))
                                {
                                    NroLinRef = true;
                                }
                                // Valide FolioRef ex: DE01724A17C14S0015
                                if (detalle.Instruction != null && (string.Compare(referencia.FolioRef.Trim(), detalle.Instruction.PaymentMatrix.ReferenceCode, true) != 0)) // IgnoreCase
                                {
                                    Flag = LetterFlag.Red;
                                    FolioRef = true;
                                }
                                // Valide RazonRef ex: SEN_[RBPA][Ene18-Dic18][R][V02] / Glosa
                                if (detalle.Instruction != null && (string.Compare(referencia.RazonRef.Trim(), detalle.Instruction.PaymentMatrix.NaturalKey, true) != 0)) // IgnoreCase
                                {
                                    Flag = LetterFlag.Red;
                                    RazonRef = true;
                                }
                                // Valide FchRef
                                if (string.IsNullOrEmpty(referencia.FchRef.ToString()))
                                {
                                    FchRef = true;
                                }
                            }
                            else
                            {
                                Flag = LetterFlag.Red;
                                TpoDocRef = true;
                            }
                        }
                        else
                        {
                            Flag = LetterFlag.Red;
                            FolioRef = true;
                            RazonRef = true;
                            TpoDocRef = true;
                        }

                        // Valide Instruction (Only Debtor)
                        if (detalle.Instruction == null)
                        {
                            Flag = LetterFlag.Red;
                            FolioRef = true;
                            RazonRef = true;
                            TpoDocRef = true;
                        }
                        else
                        {
                            // Valide Amount 
                            if (Convert.ToUInt32(dte.Encabezado.Totales.MntNeto) != detalle.Instruction.Amount)
                            {
                                Flag = LetterFlag.Blue;                     
                            }
                            // Valide DscItem ex: SEN_[RBPA][Ene18-Dic18][R][V02]
                            //if (dte.Detalle != null && dte.Detalle.Length == 1 && dte.Detalle[0].DscItem != null )
                            //{
                            //    if (string.Compare(dte.Detalle[0].DscItem.TrimEnd(), detalle.Instruction.PaymentMatrix.NaturalKey, true) != 0) // IgnoreCase
                            //    {
                            //        Flag = LetterFlag.Yellow;
                            //        DscItem = true;
                            //    }
                            //}
                        }
                        // Valide excluide
                        // Enel Distribución Chile S.A. & Chilquinta Energía S.A && Cge S.A
                        // 96800570 / 96813520 / 76411321
                        if ((detalle.RutReceptor == "96800570" || detalle.RutReceptor == "96813520" || detalle.RutReceptor == "76411321") && (isCreditor == false ) )
                        {
                            Flag = LetterFlag.Clear;
                        }
                    }
                    else
                    {   // No Xml
                        Flag = LetterFlag.Red;
                    }
                }
                else
                {
                    // No facturada & No participant
                    Flag = LetterFlag.Clear;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public enum LetterFlag
        {
            Red,
            Blue,
            Yellow,
            Green,
            Complete,
            Clear
        }
    }
}
