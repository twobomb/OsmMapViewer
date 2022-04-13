using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmMapViewer.Misc
{
    public static class Config{
        //public const string EXTRA_SEARCH_STRING = "http://localhost/osm_ids.php";
        public const string EXTRA_SEARCH_STRING = "http://10.113.0.183/api/osm_ids.php";

        //public const string GET_DATA = "http://localhost/get_data.php";
        public const string GET_DATA = "http://10.113.0.183/api/get_data.php";

        //public const string NOMINATIM_HOST = "https://nominatim.openstreetmap.org/";
        public const string NOMINATIM_HOST = "http://10.113.0.183/nominatim/";
        public const string NOMINATIM_SEARCH = "search.php";
        public const string NOMINATIM_LOOKUP = "lookup.php";


        public const string TILE_SERVER_TEMPLATE = "http://10.113.0.183/hot/{tileLevel}/{tileX}/{tileY}.png";
        //public const string TILE_SERVER_TEMPLATE = "https://tile.openstreetmap.org/{tileLevel}/{tileX}/{tileY}.png";
        public static string JSON_TAGS
        {
            get
            {
                return Utils.GetExeDir() + "\\tag_data_local.json";
            }
        }
    }
}
