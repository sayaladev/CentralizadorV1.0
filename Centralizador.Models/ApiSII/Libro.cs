using System.Collections.Generic;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.ApiSII
{
    public class Libro
    {
        public IList<Detalle> DetalleCReditor { get; set; }

        //Outlook clases (o en clase Detalle, luego lo veremos)

        public IList<ResultPaymentMatrix> ResultPaymentMatrix { get; set; }
    }
}
