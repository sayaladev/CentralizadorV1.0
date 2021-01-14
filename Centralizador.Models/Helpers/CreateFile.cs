using System.Diagnostics;
using System.IO;
using System.Text;
using MimeKit;

namespace Centralizador.Models.Helpers
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

        public CreateFile(string path, MimeMessage mime, string nameFile)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (mime != null)
            {
                File.WriteAllText(path + nameFile + ".eml", mime.ToString());
                //ProcessStartInfo process = new ProcessStartInfo(path + nameFile + ".eml")
                //{
                //    WindowStyle = ProcessWindowStyle.Normal
                //};
                //Process.Start(process);
            }
        }
    }
}