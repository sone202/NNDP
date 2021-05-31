using System.Collections.Generic;

namespace BubbleChartOilWells.DataAccess.Models
{
    public class Session
    {
        public IEnumerable<OilWell> OilWells { get; set; }
        public IEnumerable<Map> Maps { get; set; }
        public DebitGain DebitGain { get; set; }
    }
}