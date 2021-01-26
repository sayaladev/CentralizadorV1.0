using Centralizador.Models.ApiSII;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace Centralizador.Models.Helpers
{
    public class HFlagValidator
    {
        public bool NroLinRef { get; set; }
        public bool TpoDocRef { get; set; }
        public bool FolioRef { get; set; }
        public bool FchRef { get; set; }
        public bool RazonRef { get; set; }
        public bool DscItem { get; set; }
        public bool FmaPago { get; set; }
        public LetterFlag Flag { get; set; }

        // CTOR.
        public HFlagValidator(Detalle detalle, bool isCreditor)
        {
            ValidateCen(detalle, isCreditor);
        }

        public HFlagValidator()
        {
        }

        // METHODS.
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
                    Flag = LetterFlag.Green; // DEFAULT.
                    if (detalle.DTEDef != null)
                    {
                        DTEDefTypeDocumento dte = (DTEDefTypeDocumento)detalle.DTEDef.Item;
                        if (dte.Encabezado.IdDoc.FmaPago != DTEDefTypeDocumentoEncabezadoIdDocFmaPago.Crédito) // VALIDE FORMA PAGO.
                        {
                            Flag = LetterFlag.Yellow;
                            FmaPago = true;
                        }
                        if (dte.Referencia != null)
                        {
                            DTEDefTypeDocumentoReferencia referencia = dte.Referencia.FirstOrDefault(x => x.TpoDocRef.ToUpper() == "SEN");
                            if (referencia != null && detalle.Instruction != null)
                            {
                                if (string.IsNullOrEmpty(referencia.NroLinRef)) { NroLinRef = true; }
                                if (Compare(referencia.FolioRef, detalle.Instruction.PaymentMatrix.ReferenceCode, true) == -1) // DE01724A17C14S0015
                                {
                                    Flag = LetterFlag.Red;
                                    FolioRef = true;
                                }
                                if (Compare(referencia.RazonRef, detalle.Instruction.PaymentMatrix.NaturalKey, true) == -1) // SEN_[RBPA][Ene18-Dic18][R][V02]
                                {
                                    Flag = LetterFlag.Red;
                                    RazonRef = true;
                                }
                                if (string.IsNullOrEmpty(referencia.FchRef.ToString())) { FchRef = true; }
                            }
                            else
                            {
                                // NO REF CEN.
                                Flag = LetterFlag.Red;
                                FolioRef = true;
                                RazonRef = true;
                                TpoDocRef = true;
                            }
                        }
                        else
                        {
                            // NO REFS.
                            Flag = LetterFlag.Red;
                            FolioRef = true;
                            RazonRef = true;
                            TpoDocRef = true;
                        }

                        // Valide Instruction (Only Debtor)
                        if (detalle.Instruction == null && isCreditor == false)
                        {
                            Flag = LetterFlag.Red;
                            FolioRef = true;
                            RazonRef = true;
                            TpoDocRef = true;
                        }
                        else
                        {
                            // Valide Amount
                            if (Convert.ToUInt32(dte.Encabezado.Totales.MntNeto) != detalle.Instruction.Amount) { Flag = LetterFlag.Blue; }
                        }
                        // Valide excluide
                        // Enel Distribución Chile S.A. & Chilquinta Energía S.A && Cge S.A
                        // 96800570 / 96813520 / 76411321
                        if ((detalle.RutReceptor == "96800570" || detalle.RutReceptor == "96813520" || detalle.RutReceptor == "76411321") && (isCreditor == false))
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

        public static int Compare(string strA, string strB, bool ignoreCase)
        {
            // RETURN 0 TRUE
            // RETURN -1 FALSE
            if (ignoreCase)
            {
                return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
            }
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
        }

        public enum StatusDetalle
        {
            Accepted,
            Rejected,
            Pending,
            Factoring
        }

        public static StatusDetalle GetStatus(Detalle detalle)
        {
            // http://www.sii.cl/factura_electronica/Webservice_Registro_Reclamo_DTE_V1.2.pdf
            if (detalle.DataEvento != null && detalle.DataEvento.ListEvenHistDoc != null && detalle.DataEvento.ListEvenHistDoc.Count > 0)
            {
                // Ordeno por el evento más reciente
                List<ListEvenHistDoc> eventos = detalle.DataEvento.ListEvenHistDoc.OrderByDescending(x => x.FechaEvento).ToList();
                ListEvenHistDoc res = eventos.FirstOrDefault();

                switch (res.CodEvento)
                {
                    case "ACD": // Acepta Contenido del Documento
                        return StatusDetalle.Accepted;

                    case "RCD": // Reclamo al Contenido del Documento
                        return StatusDetalle.Rejected;

                    case "PAG": // Pago Contado
                        return StatusDetalle.Accepted;

                    case "ERM": // Acuse de  Recibo de Mercaderías y Servicios Ley 19.983
                        return StatusDetalle.Accepted;

                    case "ENC": // Recepción de NC, distinta de anulación, que referencia al documento.
                        return StatusDetalle.Accepted;

                    case "RFT": // Receclamo por falta total de mercaderías.
                        return StatusDetalle.Rejected;

                    case "RFP": // Receclamo por falta parcial de mercaderías.
                        return StatusDetalle.Rejected;

                    case "NCA": // Recepción de NC de anulación que referencia al documento.
                        return StatusDetalle.Rejected;

                    case "CED": // DTE Cedido.
                        return StatusDetalle.Factoring;

                    default:
                        return StatusDetalle.Pending;
                }
            }
            else
            {
                if (detalle.DataEvento.MayorOchoDias)
                {
                    return StatusDetalle.Accepted;
                }
                else
                {
                    return StatusDetalle.Pending;
                }
            }
        }
    }
}