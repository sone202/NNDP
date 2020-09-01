using Microsoft.Build.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleChartOilWells.Models
{
    public class Field
    {
        public string name;
        public List<Area> areas = new List<Area>();
        public List<Objective> objectives = new List<Objective>();

        public Field(string name) { this.name = name; }

        public List<OilWell> GetOilWells()
        {
            List<OilWell> oil_wells = new List<OilWell>();
            foreach(Area area in areas)
                    oil_wells.AddRange(area.oil_wells);
            return oil_wells;    
        }
    }
}
