using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.AppFunctions;

using static Centralizador.Models.ApiSII.ServiceDetalle;

namespace Centralizador.Models
{
    interface IDetalle
    {
        string DataBaseName { get; set; }
        ResultParticipant UserParticipant { get; set; }
        string TokenSii { get; set; }
        string TokenCen { get; set; }
        ProgressReportModel ReportModel { get; set; }

        Task<List<Detalle>> GetDetalleCreditor(List<ResultPaymentMatrix> matrices, IProgress<ProgressReportModel> progress, CancellationToken cancellationToke);

       


    }
}
