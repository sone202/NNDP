using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleChartOilWells.Models
{
    public struct OilWell
    {
        public OilWell(List<double> values)
        {
            ID = (int)values[0];
            X = values[1];
            Y = values[2];
            Oil_Debit = values[3];
            Liquid_Debit = values[4];
            Oil_Production = values[5];
            Liquid_Production = values[6];
        }

        public int ID;

        public double X;
        public double Y;

        public double Oil_Debit;
        public double Liquid_Debit;

        public double Oil_Production;
        public double Liquid_Production;
    }
}
