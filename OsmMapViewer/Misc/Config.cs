using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using OsmMapViewer.Properties;

namespace OsmMapViewer.Misc
{
    public static class Config {

        public static void InitData(){
            login_osm = Settings.Default.login_osm;
            pwd_osm = Settings.Default.pwd_osm;
            GET_DATA = Settings.Default.GET_DATA;
            NOMINATIM_HOST = Settings.Default.NOMINATIM_HOST;
            NOMINATIM_SEARCH = Settings.Default.NOMINATIM_SEARCH;
            NOMINATIM_LOOKUP = Settings.Default.NOMINATIM_LOOKUP;
            NOMINATIM_REVERSE= Settings.Default.NOMINATIM_REVERSE;
            OSRM_HOST= Settings.Default.OSRM_HOST;
            API_OSM = Settings.Default.API_OSM;
            BingKey = Settings.Default.BingKey;
            TILE_SERVER_TEMPLATE = Settings.Default.TILE_SERVER_TEMPLATE;
        }

        public static string login_osm = "mchs_lpr";
        public static string pwd_osm = "lbgfgths";

        public static double DEFAULT_DOT_SIZE = 20;//размеры dot по дефолту
        public static SolidColorBrush SELECT_BRUSH_BORDER = new SolidColorBrush(Colors.Aqua);


        public static string OSRM_HOST = "https://routing.openstreetmap.de/routed-car/route/";

        public static string GET_DATA = "http://localhost/get_data.php";
        //public const string GET_DATA = "http://10.113.0.183/api/get_data.php";

        public static string NOMINATIM_HOST = "https://nominatim.openstreetmap.org/";
        //public const string NOMINATIM_HOST = "http://10.113.0.183/nominatim/";
        public static string NOMINATIM_SEARCH = "search.php";
        public static string NOMINATIM_LOOKUP = "lookup.php";
        public static string NOMINATIM_REVERSE = "reverse.php";

        public static string API_OSM = "https://api.openstreetmap.org/"; //production
        //public const string API_OSM = "https://master.apis.dev.openstreetmap.org/"; //test

        public static string BingKey =
            "TQJlJkdqn8eblIn2ndan~z1_5jmraoFpBYePLV-eYSg~Ah9W-VI_DJOOzIzE5sXX2KD6cnS4vIa2yWFj_J1KE2z_y910L2KjRV54D2SZsmM8";

        //public const string TILE_SERVER_TEMPLATE = "http://10.113.0.183/hot/{tileLevel}/{tileX}/{tileY}.png";
        public static string TILE_SERVER_TEMPLATE = "https://tile.openstreetmap.org/{tileLevel}/{tileX}/{tileY}.png";
        
        public static string JSON_TAGS{
            get{
                return Utils.GetExeDir() + "\\tag_data_local.json";
            }
        }
    }
}
