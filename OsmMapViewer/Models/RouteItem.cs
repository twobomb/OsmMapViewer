using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpf.Map;

namespace OsmMapViewer.Models
{
    public class RouteItem {
        public string DisplayName { get; set; }

        public string DisplayDist
        {
            get
            {
                return Utils.PrettyDistance(DistMetr);
            }
        }

        public string DisplayDuration
        {
            get
            {
                if (DurationSec < 60)
                    return DurationSec + " сек";
                int hour = (int)Math.Floor((decimal) (DurationSec / 3600));
                int min = (int)Math.Floor((decimal) (DurationSec%3600/60));
                if (hour == 0)
                    return min + " мин";
                return hour + " ч " + min + " мин";
            }
        }

        public MapItem Object{ get; set; }
        public string LegsJson { get; set; }
        public List<InstructionItem> Instructions{ get; set; }

        public int DistMetr = 0;
        public int DurationSec = 0;
    }
}
