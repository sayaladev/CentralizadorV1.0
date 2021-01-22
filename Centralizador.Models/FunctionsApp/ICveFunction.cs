using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.DataBase;
using Centralizador.Models.Helpers;

using static Centralizador.Models.Helpers.HEnum;

namespace Centralizador.Models.FunctionsApp
{
    public interface ICveFunction : IDisposable
    {
        CancellationTokenSource Cancellation { get; set; }

        HPgModel PgModel { get; set; }
        IProgress<HPgModel> Progress { get; set; }

        string TokenCen { get; set; }
        string TokenSii { get; set; }
        ResultParticipant UserParticipant { get; set; }
        List<Detalle> DetalleList { get; set; }
        Conexion Conn { get; set; }
        TipoTask Mode { get; set; }
        List<ResultBilingType> BilingTypes { get; set; }
        StringBuilder StringLogging { get; set; }
        bool IsRuning { get; set; }

        Task CancelTask();

        Task ReportProgress(float p, string msg);

        //void SaveParam();

        Task GetDocFromStore(DateTime period);

        Task ConvertXmlToPdf(TipoTask task, List<Detalle> lista);

        Task ConvertXmlToPdf(Detalle d, TipoTask task);

        void SaveLogging(string path, string nameFile);
    }
}