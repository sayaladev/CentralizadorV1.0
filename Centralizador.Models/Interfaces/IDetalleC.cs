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
    internal interface IDetalleC
    {
        string DataBaseName { get; set; }
        ResultParticipant UserParticipant { get; set; }
        string TokenSii { get; set; }
        string TokenCen { get; set; }
        ProgressReportModel ProgressReport { get; set; }

        StringBuilder StringLogging { get; set; }

        Task<List<Detalle>> GetDetalleCreditor(List<ResultPaymentMatrix> matrices, IProgress<ProgressReportModel> progress, CancellationToken cancellationToke);

        Task<List<int>> InsertNv(List<Detalle> detalles, IProgress<ProgressReportModel> progress, List<ResultBilingType> types);
    }
}