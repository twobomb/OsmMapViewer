﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OsmMapViewer.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.8.1.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string KitSelections {
            get {
                return ((string)(this["KitSelections"]));
            }
            set {
                this["KitSelections"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string Layers {
            get {
                return ((string)(this["Layers"]));
            }
            set {
                this["Layers"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("mchs_lpr")]
        public string login_osm {
            get {
                return ((string)(this["login_osm"]));
            }
            set {
                this["login_osm"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("lbgfgths")]
        public string pwd_osm {
            get {
                return ((string)(this["pwd_osm"]));
            }
            set {
                this["pwd_osm"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://10.113.0.183/api/get_data.php")]
        public string GET_DATA {
            get {
                return ((string)(this["GET_DATA"]));
            }
            set {
                this["GET_DATA"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://10.113.0.183/nominatim/")]
        public string NOMINATIM_HOST {
            get {
                return ((string)(this["NOMINATIM_HOST"]));
            }
            set {
                this["NOMINATIM_HOST"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("search.php")]
        public string NOMINATIM_SEARCH {
            get {
                return ((string)(this["NOMINATIM_SEARCH"]));
            }
            set {
                this["NOMINATIM_SEARCH"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("lookup.php")]
        public string NOMINATIM_LOOKUP {
            get {
                return ((string)(this["NOMINATIM_LOOKUP"]));
            }
            set {
                this["NOMINATIM_LOOKUP"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://api.openstreetmap.org/")]
        public string API_OSM {
            get {
                return ((string)(this["API_OSM"]));
            }
            set {
                this["API_OSM"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("TQJlJkdqn8eblIn2ndan~z1_5jmraoFpBYePLV-eYSg~Ah9W-VI_DJOOzIzE5sXX2KD6cnS4vIa2yWFj_" +
            "J1KE2z_y910L2KjRV54D2SZsmM8")]
        public string BingKey {
            get {
                return ((string)(this["BingKey"]));
            }
            set {
                this["BingKey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://10.113.0.183/hot/{tileLevel}/{tileX}/{tileY}.png")]
        public string TILE_SERVER_TEMPLATE {
            get {
                return ((string)(this["TILE_SERVER_TEMPLATE"]));
            }
            set {
                this["TILE_SERVER_TEMPLATE"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Win10Light")]
        public string theme {
            get {
                return ((string)(this["theme"]));
            }
            set {
                this["theme"] = value;
            }
        }
    }
}
