using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmMapViewer.Models {
    
    [Serializable]
    public class KitSelection{
        public int ID { get; set; }
        public string Name { get; set; }
        public string Json { get; set; }
    }
}
