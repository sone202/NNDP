using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleChartOilWells.Models
{
    public class Area
    {
        public string name;
        public List<OilWell> oil_wells = new List<OilWell>();

        public Area(string name) { this.name = name; }
    }
}
