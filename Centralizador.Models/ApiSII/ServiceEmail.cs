using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using Centralizador.Models.ApiCEN;

namespace Centralizador.Models.ApiSII
{
    public class ServiceEmail
    {

        public static string GetFile(ResultParticipant participante)
        {
            try
            {
                // Get digital cert  
                X509Certificate2 cert = null;
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                foreach (X509Certificate2 item in store.Certificates)
                {
                    if (item.SerialNumber == Properties.Settings.Default.SerialDigitalCert)
                    {
                        cert = item;
                    }
                }
                store.Close();
                string[] subject =  cert.SubjectName.Name.Split(',');
                string rut = subject.GetValue(0).ToString();
                rut = rut.Substring(13, 10);

                string url = "https://palena.sii.cl/cvc_cgi/dte/ce_empresas_dwnld";
                string token = TokenSeed.GETTokenFromSii();

                WebClient wc = new WebClient();
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                wc.Encoding = Encoding.UTF8;
                //wc.Headers[HttpRequestHeader.Cookie] = $"RUT_NS={rut.Split('-').GetValue(0)}; DV_NS={rut.Split('-').GetValue(1)};TOKEN={token}";
                wc.Headers[HttpRequestHeader.Cookie] = "cert_Origin=hercules.sii.cl; s_cc=true; NETSCAPE_LIVEWIRE.rut=16003040; NETSCAPE_LIVEWIRE.rutm=16003040; NETSCAPE_LIVEWIRE.dv=2; NETSCAPE_LIVEWIRE.dvm=2; NETSCAPE_LIVEWIRE.clave=SIiY4pGm6R./sSIlci1D1pnkCs; NETSCAPE_LIVEWIRE.mac=776prrkuutns0nt9f2b7mdgt37; NETSCAPE_LIVEWIRE.exp=20200330043214; NETSCAPE_LIVEWIRE.sec=0000; NETSCAPE_LIVEWIRE.lms=120; TOKEN=CEKO52oXS0jvU; CSESSIONID=CEKO52oXS0jvU; RUT_NS=16003040; DV_NS=2";

                wc.DownloadFile(url, "algo.csv");
                //if (result != null)
                //{


                //}






            }
            catch (Exception)
            {

                throw;
            }
            return null;
        }


    }
}
