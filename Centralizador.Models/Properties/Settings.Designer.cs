﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Centralizador.Models.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://ppagos-sen.coordinadorelectrico.cl/api/v1/resources/")]
        public string BaseAddress {
            get {
                return ((string)(this["BaseAddress"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("german.gomez@cvegroup.com")]
        public string UserCEN {
            get {
                return ((string)(this["UserCEN"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("cvepagos2017")]
        public string PasswordCEN {
            get {
                return ((string)(this["PasswordCEN"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://palena.sii.cl/DTEWS/CrSeed.jws")]
        public string Centralizador_Models_CrSeed_CrSeedService {
            get {
                return ((string)(this["Centralizador_Models_CrSeed_CrSeedService"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://palena.sii.cl/DTEWS/GetTokenFromSeed.jws")]
        public string Centralizador_Models_GetTokenFromSeed_GetTokenFromSeedService {
            get {
                return ((string)(this["Centralizador_Models_GetTokenFromSeed_GetTokenFromSeedService"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("https://ws1.sii.cl/WSREGISTRORECLAMODTE/registroreclamodteservice")]
        public string Centralizador_Models_registroreclamodteservice_RegistroReclamoDteServiceEndpointService {
            get {
                return ((string)(this["Centralizador_Models_registroreclamodteservice_RegistroReclamoDteServiceEndpointS" +
                    "ervice"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0138BF")]
        public string SerialDigitalCert {
            get {
                return ((string)(this["SerialDigitalCert"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<DatosEmpresas>
  <Empresa id=""189"">SANFRANCISCO</Empresa>
  <Empresa id=""190"">QUINTA</Empresa>
  <Empresa id=""194"">VALLE</Empresa>
  <Empresa id=""364"">OCOA</Empresa>
  <Empresa id=""415"">ST4</Empresa>
  <Empresa id=""427"">ST10</Empresa>
  <Empresa id=""428"">TRICAHUESOLAR</Empresa>
  <Empresa id=""469"">ST11</Empresa>
  <Empresa id=""470"">PILPILEN</Empresa>
</DatosEmpresas>")]
        public global::System.Xml.XmlDocument DBSoftland {
            get {
                return ((global::System.Xml.XmlDocument)(this["DBSoftland"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("02/12/2020 13:07:00")]
        public global::System.DateTime DateTimeEmail {
            get {
                return ((global::System.DateTime)(this["DateTimeEmail"]));
            }
            set {
                this["DateTimeEmail"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string TokenSii {
            get {
                return ((string)(this["TokenSii"]));
            }
            set {
                this["TokenSii"] = value;
            }
        }
    }
}
