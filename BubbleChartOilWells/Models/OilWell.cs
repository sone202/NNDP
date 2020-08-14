using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleChartOilWells.Models
{
    public class OilWell
    {
        public int ID;

        public double X;
        public double Y;

        public double oil_debit;
        public double liquid_debit;

        public double oil_prod;
        public double liquid_prod;

        public OilWell(List<double> values)
        {
            ID = (int)values[0];
            X = values[1];
            Y = values[2];
            oil_debit = values[3];
            liquid_debit = values[4];
            oil_prod = values[5];
            liquid_prod = values[6];
        }
       
    }
}
