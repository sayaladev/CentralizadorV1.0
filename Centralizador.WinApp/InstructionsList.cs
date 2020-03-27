using System.Collections.Generic;
using Centralizador.Models.ApiCEN;

namespace Centralizador.WinApp
{
    internal class InstructionsList
    {
        //Main, first point to the get data.
        public IList<ResultInstruction> InstructionCreditor { get; set; }
        public IList<ResultInstruction> InstructionDebitor { get; set; }
    }
}
