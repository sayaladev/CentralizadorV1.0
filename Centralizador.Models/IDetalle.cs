using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;

namespace Centralizador.Models
{
    internal interface IDetalle
    {
        Task<List<Detalle>> GetDetalleCreditor(List<ResultPaymentMatrix> matrices, IProgress<ProgressReportModel> progress, CancellationToken cancellationToke);

        Task<List<Detalle>> GetDetalleDebtor(List<Detalle> detalles, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken, string p);

        Task<List<int>> InsertNv(List<Detalle> detalles, IProgress<ProgressReportModel> progress, List<ResultBilingType> types);

    }
}
