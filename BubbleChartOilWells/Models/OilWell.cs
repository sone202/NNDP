using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BubbleChartOilWells.Models
{
    public class OilWell
    {
        public Region region;
        public Field field;
        public Area area;
        public List<Objective> objectives;

        public int ID;

        public Point coordinates;

        public List<DateTime> dates;

        public Dictionary<DateTime, int> work_period;

        public Dictionary<DateTime, double> liquid_debit;
        public Dictionary<DateTime, double> oil_debit;
        public Dictionary<DateTime, double> water_debit;

        public Dictionary<DateTime, double> liquid_prod;
        public Dictionary<DateTime, double> oil_prod;
        public Dictionary<DateTime, double> water_prod;

        public Dictionary<DateTime, double> liquid_prod_SUM;
        public Dictionary<DateTime, double> oil_prod_SUM;
        public Dictionary<DateTime, double> water_prod_SUM;

        public Dictionary<DateTime, double> water_encroachment;
        public Dictionary<DateTime, double> injection;
        public Dictionary<DateTime, double> injection_capacity;




        public Bubble bubble;

        public OilWell(int ID) { this.ID = ID; }
        public OilWell(int ID, Region region,Field field, Area area, Point coordinates)
        {
            this.ID = ID;
            this.region = region;
            this.field = field;
            this.area = area;
            this.coordinates = coordinates;
        }
        //public void Update()
        //{
        //    bubble.Update(this);
        //}
    }
}
