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
        public string region;
        public string field;
        public string area;
        public SortedSet<string> objectives;

        public int ID;

        public Point coordinates;


        public List<(string, DateTime)> dates = new List<(string, DateTime)>();

        public Dictionary<(string, DateTime), double> work_period = new Dictionary<(string, DateTime), double>();

        public Dictionary<(string, DateTime), double> liquid_debit = new Dictionary<(string, DateTime), double>();
        public Dictionary<(string, DateTime), double> oil_debit = new Dictionary<(string, DateTime), double>();
        public Dictionary<(string, DateTime), double> water_debit = new Dictionary<(string, DateTime), double>();

        public Dictionary<(string, DateTime), double> liquid_prod = new Dictionary<(string, DateTime), double>();
        public Dictionary<(string, DateTime), double> oil_prod = new Dictionary<(string, DateTime), double>();
        public Dictionary<(string, DateTime), double> water_prod = new Dictionary<(string, DateTime), double>();

        public Dictionary<(string, DateTime), double> liquid_prod_SUM = new Dictionary<(string, DateTime), double>();
        public Dictionary<(string, DateTime), double> oil_prod_SUM = new Dictionary<(string, DateTime), double>();
        public Dictionary<(string, DateTime), double> water_prod_SUM = new Dictionary<(string, DateTime), double>();

        public Dictionary<(string, DateTime), double> water_encroachment = new Dictionary<(string, DateTime), double>();
        public Dictionary<(string, DateTime), double> injection = new Dictionary<(string, DateTime), double>();
        public Dictionary<(string, DateTime), double> injection_capacity = new Dictionary<(string, DateTime), double>();




        public Bubble bubble;
        public OilWell() { }
        public OilWell(int ID) { this.ID = ID; }

        //public void Update()
        //{
        //    bubble.Update(this);
        //}
    }
}
