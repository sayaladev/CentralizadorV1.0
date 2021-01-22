using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Centralizador.Models.DataBase
{
    public interface IConexion
    {
        string DBName { get; set; }
        string Query { get; set; }

        Task<object> ExecuteNonQueryAsync(Conexion conn);

        void ExecuteNonQueryAsyncTG(Conexion conn);

        Task<int> ExecuteNonQueryTranAsync(Conexion conn, List<string> listQ);

        Task<DataTable> ExecuteReaderAsync(Conexion conn);

        Task<object> ExecuteScalarAsync(Conexion conn);
    }
}