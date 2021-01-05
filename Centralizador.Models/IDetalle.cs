using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.DataBase;

namespace Centralizador.Models
{
    internal interface IDetalle
    {
        Task<List<Detalle>> GetDetalleCreditor(List<ResultPaymentMatrix> matrices, IProgress<ProgressReportModel> progress, CancellationToken cancellationToke);

        Task<List<Detalle>> GetDetalleDebtor(List<Detalle> detalles, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken, string p);

        Task<List<int>> InsertNv(List<Detalle> detalles, IProgress<ProgressReportModel> progress, List<ResultBilingType> types);

        // Task<int> GetStatusDteAsync("Creditor", TokenSii, "33", detalle, UserParticipant);
        // Task<List<Detalle>> GetStatusDteAsync(string mode, string tokenSii, string tipoDte, List<Detalle> detallesList, ResultParticipant UserParticipant);
    }
}