using System.Collections.Generic;
using System.Data;

namespace BubbleChartOilWells.DataAccess.Models
{
    public class DebitGain
    {
        public double Pwf { get; set; }
        public double Re { get; set; }
        public double Rw { get; set; }
        public double Stot { get; set; }
        public List<double> KoInEachMapCell { get; set; }
        public List<double> KwInEachMapCell { get; set; }
        public double Krwor { get; set; }
        public double Krocw { get; set; }
        public double Sor { get; set; }
        public double Scw { get; set; }
        public double MUo { get; set; }
        public double MUw { get; set; }
        public double Bo { get; set; }
        public double Bw { get; set; }
        public double Nw { get; set; }
        public double No { get; set; }
        public DataTable Ofp { get; set; }

    }
}