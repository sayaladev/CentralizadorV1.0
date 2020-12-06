using System.Diagnostics;
using System.IO;
using System.Text;

namespace Centralizador.Models.AppFunctions
{
    public class CreateFile
    {
        public CreateFile(string path, StringBuilder log, string nameFile)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (log.Length > 0)
            {
                File.WriteAllText(path + nameFile + ".txt", log.ToString());
                ProcessStartInfo process = new ProcessStartInfo(path + nameFile + ".txt")
                {
                    WindowStyle = ProcessWindowStyle.Normal
                };
                Process.Start(process);
            }
        }
        public CreateFile(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
