﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Centralizador.WinApp.Properties {
    
    
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
        [global::System.Configuration.DefaultSettingValueAttribute("softland")]
        public string DBUser {
            get {
                return ((string)(this["DBUser"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("softland")]
        public string DBPassword {
            get {
                return ((string)(this["DBPassword"]));
            }
        }
    }
}
