using System.Collections.Generic;



namespace Centralizador.Models.DataBase
{
    public class Softland
    {

        public int IdInstruction { get; set; }

        public IList<DBReference> Reference { get; set; }

        public IList<DBSendSii> SendSii { get; set; }

        public Softland GetataFromSoftland(ApiCEN.ResultInstruction instruction)
        {



            IList<DBReference> references = new List<DBReference>();
            //references = DataBase.DBReference.GetReferenceByGlosa(instruction);



            return null;

        }


    }
}
