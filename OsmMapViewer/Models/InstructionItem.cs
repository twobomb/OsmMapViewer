using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpf.Map;

namespace OsmMapViewer.Models
{
    public class InstructionItem
    {
        public int Index{ get; set; }
        public MapItem Geometry { get; set; }
        public string DisplayName { get; set; }
        public string DisplayDist{ get; set; }
    }
}
