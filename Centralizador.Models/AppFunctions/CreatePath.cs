using System.IO;

namespace Centralizador.Models.AppFunctions
{
    public class CreatePath
    {
        public CreatePath(string path)
        {
            //@"C:\Centralizador\Log\"
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
