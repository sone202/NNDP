using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BubbleChartOilWells.DataAccess.Models
{
    public class Map
    {
        public double CellWidth { get; set; }
        public double CellHeight { get; set; }
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public List<double> Z { get; set; }
        public bool IsSelected { get; set; }
        public bool IsExporting { get; set; }
        public Point LeftBottomCoordinate { get; set; }
        public BitmapSource BitmapSource { get; set; }
        public string Objective { get; set; }
    }
}