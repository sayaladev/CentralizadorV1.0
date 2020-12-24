using System.Linq;

using Centralizador.Models.ApiSII;

namespace Centralizador.Models.AppFunctions
{
    public class GetReferenceCen
    {
        public DTEDefTypeDocumentoReferencia DocumentoReferencia { get; set; }
        public GetReferenceCen(Detalle detalle)
        {
            DocumentoReferencia = GetRef(detalle);
        }


        private DTEDefTypeDocumentoReferencia GetRef(Detalle detalle)
        {

            DTEDefTypeDocumento obj = (DTEDefTypeDocumento)detalle.DTEDef.Item;
            DTEDefTypeDocumentoReferencia[] refr = obj.Referencia;
            DTEDefTypeDocumentoReferencia r = refr.FirstOrDefault(x => x.TpoDocRef.ToUpper() == "SEN");
            if (r != null)
            {
                return r;
            }
            return null;
        }

    }
}
