using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Centralizador.Models.Outlook
{

    // NOTA: El código generado puede requerir, como mínimo, .NET Framework 4.5 o .NET Core/Standard 2.0.
    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    [XmlRoot(Namespace = "http://www.sii.cl/SiiDte", IsNullable = false)]
    public partial class EnvioDTE
    {

        /// <remarks/>
        public EnvioDTESetDTE SetDTE { get; set; }

        /// <remarks/>
        //[XmlElement(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
        public Signature Signature { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public decimal version { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTE
    {

        /// <remarks/>
        public EnvioDTESetDTECaratula Caratula { get; set; }

        /// <remarks/>
        [XmlElement("DTE")]
        public EnvioDTESetDTEDTE[] DTE { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string ID { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTECaratula
    {

        /// <remarks/>
        public string RutEmisor { get; set; }

        /// <remarks/>
        public string RutEnvia { get; set; }

        /// <remarks/>
        public string RutReceptor { get; set; }

        /// <remarks/>
        [XmlElement(DataType = "date")]
        public DateTime FchResol { get; set; }

        /// <remarks/>
        public string NroResol { get; set; }

        /// <remarks/>
        public DateTime TmstFirmaEnv { get; set; }

        /// <remarks/>
        public EnvioDTESetDTECaratulaSubTotDTE SubTotDTE { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public float version { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTECaratulaSubTotDTE
    {

        /// <remarks/>
        public byte TpoDTE { get; set; }

        /// <remarks/>
        public byte NroDTE { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTE
    {

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumento Documento { get; set; }

        /// <remarks/>
        [XmlElement(Namespace = "http://www.w3.org/2000/09/xmldsig#")]
        public Signature Signature { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public decimal version { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumento
    {

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoEncabezado Encabezado { get; set; }

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoDetalle Detalle { get; set; }

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoReferencia Referencia { get; set; }

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoTED TED { get; set; }

        /// <remarks/>
        public DateTime TmstFirma { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string ID { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoEncabezado
    {

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoEncabezadoIdDoc IdDoc { get; set; }

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoEncabezadoEmisor Emisor { get; set; }

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoEncabezadoReceptor Receptor { get; set; }

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoEncabezadoTotales Totales { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoEncabezadoIdDoc
    {

        /// <remarks/>
        public byte TipoDTE { get; set; }

        /// <remarks/>
        public uint Folio { get; set; }

        /// <remarks/>
        [XmlElement(DataType = "date")]
        public DateTime FchEmis { get; set; }

        /// <remarks/>
        public byte FmaPago { get; set; }

        /// <remarks/>
        public string TermPagoGlosa { get; set; }

        /// <remarks/>
        [XmlElement(DataType = "date")]
        public DateTime FchVenc { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoEncabezadoEmisor
    {

        /// <remarks/>
        public string RUTEmisor { get; set; }

        /// <remarks/>
        public string RznSoc { get; set; }

        /// <remarks/>
        public string GiroEmis { get; set; }

        /// <remarks/>
        [XmlElement("Acteco")]
        public uint[] Acteco { get; set; }

        /// <remarks/>
        public string DirOrigen { get; set; }

        /// <remarks/>
        public string CmnaOrigen { get; set; }

        /// <remarks/>
        public string CiudadOrigen { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoEncabezadoReceptor
    {

        /// <remarks/>
        public string RUTRecep { get; set; }

        /// <remarks/>
        public string CdgIntRecep { get; set; }

        /// <remarks/>
        public string RznSocRecep { get; set; }

        /// <remarks/>
        public string GiroRecep { get; set; }

        /// <remarks/>
        public string DirRecep { get; set; }

        /// <remarks/>
        public string CmnaRecep { get; set; }

        /// <remarks/>
        public string CiudadRecep { get; set; }

        /// <remarks/>
        public uint CiudadPostal { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoEncabezadoTotales
    {

        /// <remarks/>
        public uint MntNeto { get; set; }

        /// <remarks/>
        public float TasaIVA { get; set; }

        /// <remarks/>
        public uint IVA { get; set; }

        /// <remarks/>
        public uint MntTotal { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoDetalle
    {

        /// <remarks/>
        public byte NroLinDet { get; set; }

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoDetalleCdgItem CdgItem { get; set; }

        /// <remarks/>
        public string NmbItem { get; set; }

        /// <remarks/>
        public string DscItem { get; set; }

        /// <remarks/>
        public float QtyItem { get; set; }

        /// <remarks/>
        public float PrcItem { get; set; }

        /// <remarks/>
        public float MontoItem { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoDetalleCdgItem
    {

        /// <remarks/>
        public string TpoCodigo { get; set; }

        /// <remarks/>
        public string VlrCodigo { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoReferencia
    {

        /// <remarks/>
        public byte NroLinRef { get; set; }

        /// <remarks/>
        public string TpoDocRef { get; set; }

        /// <remarks/>
        public string FolioRef { get; set; }

        /// <remarks/>
        [XmlElement(DataType = "date")]
        public DateTime FchRef { get; set; }

        /// <remarks/>
        public string RazonRef { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoTED
    {

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoTEDDD DD { get; set; }

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoTEDFRMT FRMT { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public decimal version { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoTEDDD
    {

        /// <remarks/>
        public string RE { get; set; }

        /// <remarks/>
        public byte TD { get; set; }

        /// <remarks/>
        public ushort F { get; set; }

        /// <remarks/>
        [XmlElement(DataType = "date")]
        public DateTime FE { get; set; }

        /// <remarks/>
        public string RR { get; set; }

        /// <remarks/>
        public string RSR { get; set; }

        /// <remarks/>
        public string MNT { get; set; }

        /// <remarks/>
        public string IT1 { get; set; }

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoTEDDDCAF CAF { get; set; }

        /// <remarks/>
        public DateTime TSTED { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoTEDDDCAF
    {

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoTEDDDCAFDA DA { get; set; }

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoTEDDDCAFFRMA FRMA { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public decimal version { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoTEDDDCAFDA
    {

        /// <remarks/>
        public string RE { get; set; }

        /// <remarks/>
        public string RS { get; set; }

        /// <remarks/>
        public byte TD { get; set; }

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoTEDDDCAFDARNG RNG { get; set; }

        /// <remarks/>
        [XmlElement(DataType = "date")]
        public DateTime FA { get; set; }

        /// <remarks/>
        public EnvioDTESetDTEDTEDocumentoTEDDDCAFDARSAPK RSAPK { get; set; }

        /// <remarks/>
        public ushort IDK { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoTEDDDCAFDARNG
    {

        /// <remarks/>
        public ushort D { get; set; }

        /// <remarks/>
        public ushort H { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoTEDDDCAFDARSAPK
    {

        /// <remarks/>
        public string M { get; set; }

        /// <remarks/>
        public string E { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoTEDDDCAFFRMA
    {

        /// <remarks/>
        [XmlAttribute()]
        public string algoritmo { get; set; }

        /// <remarks/>
        [XmlText()]
        public string Value { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sii.cl/SiiDte")]
    public partial class EnvioDTESetDTEDTEDocumentoTEDFRMT
    {

        /// <remarks/>
        [XmlAttribute()]
        public string algoritmo { get; set; }

        /// <remarks/>
        [XmlText()]
        public string Value { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    [XmlRoot(Namespace = "http://www.w3.org/2000/09/xmldsig#", IsNullable = false)]
    public partial class Signature
    {

        /// <remarks/>
        public SignatureSignedInfo SignedInfo { get; set; }

        /// <remarks/>
        public string SignatureValue { get; set; }

        /// <remarks/>
        public SignatureKeyInfo KeyInfo { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureSignedInfo
    {

        /// <remarks/>
        public SignatureSignedInfoCanonicalizationMethod CanonicalizationMethod { get; set; }

        /// <remarks/>
        public SignatureSignedInfoSignatureMethod SignatureMethod { get; set; }

        /// <remarks/>
        public SignatureSignedInfoReference Reference { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureSignedInfoCanonicalizationMethod
    {

        /// <remarks/>
        [XmlAttribute()]
        public string Algorithm { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureSignedInfoSignatureMethod
    {

        /// <remarks/>
        [XmlAttribute()]
        public string Algorithm { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureSignedInfoReference
    {

        /// <remarks/>
        public SignatureSignedInfoReferenceTransforms Transforms { get; set; }

        /// <remarks/>
        public SignatureSignedInfoReferenceDigestMethod DigestMethod { get; set; }

        /// <remarks/>
        public string DigestValue { get; set; }

        /// <remarks/>
        [XmlAttribute()]
        public string URI { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureSignedInfoReferenceTransforms
    {

        /// <remarks/>
        public SignatureSignedInfoReferenceTransformsTransform Transform { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureSignedInfoReferenceTransformsTransform
    {

        /// <remarks/>
        [XmlAttribute()]
        public string Algorithm { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureSignedInfoReferenceDigestMethod
    {

        /// <remarks/>
        [XmlAttribute()]
        public string Algorithm { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureKeyInfo
    {

        /// <remarks/>
        public SignatureKeyInfoKeyValue KeyValue { get; set; }

        /// <remarks/>
        public SignatureKeyInfoX509Data X509Data { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureKeyInfoKeyValue
    {

        /// <remarks/>
        public SignatureKeyInfoKeyValueRSAKeyValue RSAKeyValue { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureKeyInfoKeyValueRSAKeyValue
    {

        /// <remarks/>
        public string Modulus { get; set; }

        /// <remarks/>
        public string Exponent { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.w3.org/2000/09/xmldsig#")]
    public partial class SignatureKeyInfoX509Data
    {

        /// <remarks/>
        public string X509Certificate { get; set; }
    }



}

