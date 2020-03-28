using System.Collections.Generic;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class Softland
    {


        public IEnumerable<Reference> References { get; set; }

        public IList<InfoSii> InfoSiis { get; set; }

        public void GetSoftlandData(ResultInstruction instruction)
        {

            References = Reference.GetReferenceByGlosa(instruction);
            IList<InfoSii> lista = new List<InfoSii>();
            foreach (Reference item in References)
            {
                lista.Add(InfoSii.GetSendSiiByFolio(instruction, item.Folio));                 
               
            }
            InfoSiis = lista;  
        }
    }
}
