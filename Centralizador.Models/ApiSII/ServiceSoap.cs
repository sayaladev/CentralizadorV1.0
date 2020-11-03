using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Serialization;

using Centralizador.Models.CrSeed;
using Centralizador.Models.GetTokenFromSeed;
using Centralizador.Models.registroreclamodteservice;

namespace Centralizador.Models.ApiSII
{
    public class ServiceSoap
    {
        public static string GETTokenFromSii(string serialDigitalCert)
        {
            // Get digital cert                  
            DateTime now = DateTime.Now;
            X509Certificate2 cert = null;
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            foreach (X509Certificate2 item in store.Certificates)
            {
                if (item.SerialNumber == serialDigitalCert && item.NotAfter > now)
                {
                    cert = item;
                }
            }
            store.Close();
            if (cert != null)
            {
                try
                {
                    RESPUESTA XmlObject;
                    using (CrSeedService proxyCrSeedService = new CrSeedService())
                    {
                        string responseCrSeedService = proxyCrSeedService.getSeed();
                        XmlSerializer serializadorCrSeedService = new XmlSerializer(typeof(RESPUESTA));
                        using (TextReader readerCrSeedService = new StringReader(responseCrSeedService))
                        {
                            RESPUESTA xmlObjectCrSeedService = (RESPUESTA)serializadorCrSeedService.Deserialize(readerCrSeedService);
                            if (xmlObjectCrSeedService.RESP_HDR.ESTADO == "00")
                            {
                                string xmlNofirmado = string.Format("<getToken><item><Semilla>{0}</Semilla></item></getToken>", xmlObjectCrSeedService.RESP_BODY.SEMILLA);
                                using (GetTokenFromSeedService proxyGetTokenFromSeedService = new GetTokenFromSeedService())
                                {
                                    string responseGetTokenFromSeedService = proxyGetTokenFromSeedService.getToken(FirmarSeedDigital(xmlNofirmado, cert));
                                    XmlSerializer serializadorGetTokenFromSeedService = new XmlSerializer(typeof(RESPUESTA));
                                    using (TextReader readerGetTokenFromSeedService = new StringReader(responseGetTokenFromSeedService))
                                    {
                                        XmlObject = (RESPUESTA)serializadorGetTokenFromSeedService.Deserialize(readerGetTokenFromSeedService);
                                        if (XmlObject.RESP_HDR.ESTADO == "00")
                                        {
                                            return XmlObject.RESP_BODY.TOKEN;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                return null;
            }
            else
            {
                return null;
            }

        }
        private static string FirmarSeedDigital(string documento, X509Certificate2 certificado)
        {
            try
            {
                XmlDocument doc = new XmlDocument()
                {
                    PreserveWhitespace = true
                };
                doc.LoadXml(documento);

                SignedXml signedXml = new SignedXml(doc)
                {
                    SigningKey = certificado.PrivateKey
                };

                Signature XMLSignature = signedXml.Signature;
                Reference reference = new Reference("");

                reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                XMLSignature.SignedInfo.AddReference(reference);
                KeyInfo keyInfo = new KeyInfo();

                keyInfo.AddClause(new RSAKeyValue((System.Security.Cryptography.RSA)certificado.PrivateKey));
                keyInfo.AddClause(new KeyInfoX509Data(certificado));


                XMLSignature.KeyInfo = keyInfo;
                signedXml.ComputeSignature();
                XmlElement xmlDigitalSignature = signedXml.GetXml();
                doc.DocumentElement.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

                if (doc.FirstChild is XmlDeclaration)
                {
                    doc.RemoveChild(doc.FirstChild);
                }

                return doc.InnerXml;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static respuestaTo SendActionToSii(string token, Detalle detalle, string accionDoc)
        {
            respuestaTo respuesta = new respuestaTo();
            try
            {
                using (RegistroReclamoDteServiceEndpointService proxy = new RegistroReclamoDteServiceEndpointService(token))
                {
                    respuestaTo response = proxy.ingresarAceptacionReclamoDoc(detalle.RutReceptor.ToString(), detalle.DvReceptor, "33", detalle.Folio.ToString(), accionDoc);
                    respuesta = response;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return respuesta;
        }
    }


    // NOTA: El código generado puede requerir, como mínimo, .NET Framework 4.5 o .NET Core/Standard 2.0.
    /// <remarks/>
    [System.Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/XMLSchema")]
    [XmlRoot(Namespace = "http://www.sii.cl/XMLSchema", IsNullable = false)]
    public partial class RESPUESTA
    {

        /// <remarks/>
        public RESPUESTARESP_BODY RESP_BODY { get; set; }

        /// <remarks/>
        public RESPUESTARESP_HDR RESP_HDR { get; set; }
    }

    /// <remarks/>
    [System.Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/XMLSchema")]
    public partial class RESPUESTARESP_BODY
    {

        /// <remarks/>
        [XmlElement(Namespace = "")]
        public string SEMILLA { get; set; }

        /// <remarks/>
        [XmlElement(Namespace = "")]
        public string TOKEN { get; set; }
    }

    /// <remarks/>
    [System.Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/XMLSchema")]
    public partial class RESPUESTARESP_HDR
    {

        /// <remarks/>
        [XmlElement(Namespace = "")]
        public string ESTADO { get; set; }

        /// <remarks/>
        [XmlElement(Namespace = "")]
        public string GLOSA { get; set; }
    }


}
