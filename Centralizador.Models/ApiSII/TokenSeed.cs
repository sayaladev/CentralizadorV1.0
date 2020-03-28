using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using Centralizador.Models.CrSeed;
using Centralizador.Models.GetTokenFromSeed;

namespace Centralizador.Models.ApiSII
{
    internal class TokenSeed
    {
        public string Seed { get; set; }

        public static string GETTokenFromSii(X509Certificate2 cert)
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
                                    else
                                    {
                                        return null;
                                    }
                                }
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                MessageBox.Show($"This certificate is not valid:{Environment.NewLine} {cert.FriendlyName} ");
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
            catch (System.Exception)
            {

                return null;
            }
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
    }

}
