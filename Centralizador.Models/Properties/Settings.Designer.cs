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
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://ppagos-sen.coordinadorelectrico.cl/api/v1/resources/")]
        public string BaseAddress {
            get {
                return ((string)(this["BaseAddress"]));
            }
            set {
                this["BaseAddress"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("german.gomez@cvegroup.com")]
        public string UserCEN {
            get {
                return ((string)(this["UserCEN"]));
            }
            set {
                this["UserCEN"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("cvepagos2017")]
        public string PasswordCEN {
            get {
                return ((string)(this["PasswordCEN"]));
            }
            set {
                this["PasswordCEN"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<DatosEmpresas>\r\n  <Empresa id=\"189\">SAN" +
            "FRANCISCO</Empresa>\r\n  <Empresa id=\"190\">QUINTA</Empresa>\r\n</DatosEmpresas>")]
        public global::System.Xml.XmlDocument DBSoftland {
            get {
                return ((global::System.Xml.XmlDocument)(this["DBSoftland"]));
            }
            set {
                this["DBSoftland"] = value;
            }
        }
    }
}
