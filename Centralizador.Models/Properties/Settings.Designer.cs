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
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.5.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
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
        [global::System.Configuration.DefaultSettingValueAttribute("facturacionchile@capvertenergie.com")]
        public string UserEmail {
            get {
                return ((string)(this["UserEmail"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Che@2019!")]
        public string UserPassword {
            get {
                return ((string)(this["UserPassword"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("12/01/2019 10:00:00")]
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
        [global::System.Configuration.DefaultSettingValueAttribute("46000")]
        public string UIDRange {
            get {
                return ((string)(this["UIDRange"]));
            }
            set {
                this["UIDRange"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://ppagos-sen.coordinadorelectrico.cl/")]
        public global::System.Uri BaseAddress {
            get {
                return ((global::System.Uri)(this["BaseAddress"]));
            }
            set {
                this["BaseAddress"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10.13.0.114\\SQL2014")]
        public string ServerName {
            get {
                return ((string)(this["ServerName"]));
            }
        }
    }
}
