using System.Collections.Generic;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;

namespace Centralizador.Models.DataBase
{
    public class Softland
    {


        public IList<Reference> References { get; set; }

        public IList<InfoSii> InfoSiis { get; set; }

        public IList<Detalle> Detalles { get; set; }

        public Softland(IList<Detalle> detalles)
        {
            Detalles = detalles;
        }

        public Softland()
        {
        }

        public void GetSoftlandData(ResultInstruction instruction)
        {

            References = Reference.GetReferenceByGlosa(instruction);
            if (References != null)
            {

                IList<InfoSii> lista = new List<InfoSii>();
                foreach (Reference item in References)
                {
                    InfoSii info;
                    info = InfoSii.GetSendSiiByFolio(instruction, item.Folio);
                    if (info != null)
                    {
                        lista.Add(info);
                    }
                }
                if (lista.Count == 0)
                {
                    lista = null;
                }
                else
                {
                    InfoSiis = lista;
                }

            }

        }
    }
}
