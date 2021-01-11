using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;

namespace Centralizador.Models.Interfaces
{
    internal interface IDetalleD
    {
        string DataBaseName { get; set; }
        ResultParticipant UserParticipant { get; set; }
        string TokenSii { get; set; }
        string TokenCen { get; set; }
        ProgressReportModel ProgressReport { get; set; }

        StringBuilder StringLogging { get; set; }

        Task<List<Detalle>> GetDetalleDebtor(List<Detalle> detalles, IProgress<ProgressReportModel> progress, string p);
    }
}