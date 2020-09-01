using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BubbleChartOilWells.Models
{
    public class Objective
    {
        public string name;
        public List<OilWell> oil_wells = new List<OilWell>();

        public Objective(string name) { this.name = name; }
    }
}
