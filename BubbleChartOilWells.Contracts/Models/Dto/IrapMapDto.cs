 using System.Collections.Generic;
 using System.Dynamic;

 namespace BubbleChartOilWells.Contracts.Models.Dto
{
    public class IrapMapDto
    {
        public double CellWidth { get; set; }

        public double CellHeight { get; set; }

        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }

        public double MinX { get; set; }

        public double MinY { get; set; }

        public double MaxX { get; set; } 

        public double MaxY { get; set; }

        public double CountPerColumn { get; set; }
        
        public string Objective { get; set; }
        public List<double> Z { get; set; }
    }
}
