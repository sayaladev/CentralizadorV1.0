﻿using System.Collections.Generic;
using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.DataBase
{
    public class Softland
    {
     
        public int IdInstruction { get; set; }

        public IEnumerable<Reference> Reference { get; set; }

        public IEnumerable<InfoSii> InfoSii { get; set; }

       

    }
}
