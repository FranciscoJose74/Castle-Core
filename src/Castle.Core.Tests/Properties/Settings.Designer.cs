﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CastleTests.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>c:\Program Files\Microsoft SDKs\Windows\v7.1\Bin\NETFX 4.0 Tools</string>
  <string>c:\Program Files (x86)\Microsoft SDKs\Windows\v7.1\Bin\NETFX 4.0 Tools</string>
  <string>c:\Program Files\Microsoft SDKs\Windows\v7.0A\bin\NETFX 4.0 Tools</string>
  <string>c:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\NETFX 4.0 Tools</string>
  <string>c:\Program Files\Microsoft SDKs\Windows\v6.0A\Bin</string>
  <string>C:\Program Files (x86)\Microsoft Visual Studio 8\SDK\v2.0\bin</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection PeVerifyProbingPaths {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["PeVerifyProbingPaths"]));
            }
            set {
                this["PeVerifyProbingPaths"] = value;
            }
        }
    }
}
