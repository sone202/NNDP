using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleChartOilWells.Models
{
    public class Region
    {
        public string name;
        public List<Field> fields = new List<Field>();
        public Region(string name) { this.name = name; }
        public List<OilWell> GetOilWells()
        {
            List<OilWell> oil_wells = new List<OilWell>();
            foreach (Field field in fields)
                    oil_wells.AddRange(field.GetOilWells());
            return oil_wells;
        }
        public List<Area> GetAreas()
        {
            List<Area> areas = new List<Area>();
            foreach (Field field in fields)
                areas.AddRange(field.areas);
            return areas;
        }
        public List<Objective> GetObjectives()
        {
            List<Objective> objectives = new List<Objective>();
            foreach (Field field in fields)
                objectives.AddRange(field.objectives);
            return objectives;
        }
    }
}
