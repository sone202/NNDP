using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace BubbleChartOilWells.Models
{
    public class Bubble : INotifyPropertyChanged
    {
        public static double _min_oil_value = 100000;
        public static double _max_oil_value = 0;

        private Point _coordinates;
        private double _radius;

        private Brush _brush_oil = Brushes.SaddleBrown;
        private Brush _brush_liquid = Brushes.LightBlue;

        public OilWell data { get; private set; }

        public bool is_selected { get; private set; } = false;

        public List<Path> paths { get; private set; } = new List<Path>();

        public TextBlock ID { get; private set; }

        public Bubble(OilWell data) { this.data = data; }

        public void Update()
        {
            _coordinates = new Point(data.X, data.Y);
            _radius = 100 / _max_oil_value * (data.Oil_Production + data.Liquid_Production);
            double angle_in_rad = Math.PI / 180 * (360 / (data.Oil_Production + data.Liquid_Production)) * data.Liquid_Production;




            // figures
            EllipseGeometry circle = new EllipseGeometry
            {
                Center = _coordinates,
                RadiusX = _radius,
                RadiusY = _radius
            };


            PointCollection polyline_points = new PointCollection {
                _coordinates,
                new Point(_coordinates.X + _radius, _coordinates.Y),
                new Point(_coordinates.X + Math.Cos(angle_in_rad) * _radius, _coordinates.Y + Math.Sin(angle_in_rad) * _radius) };

            
            PolyLineSegment polyline = new PolyLineSegment
            {
                Points = polyline_points
            };

            ArcSegment arc = new ArcSegment
            {
                Point = new Point(_coordinates.X + Math.Cos(angle_in_rad) * _radius, _coordinates.Y + Math.Sin(angle_in_rad) * _radius),
                Size = new Size(_radius, _radius),
                SweepDirection = angle_in_rad > Math.PI ? SweepDirection.Counterclockwise : SweepDirection.Clockwise
            };


            EllipseGeometry coordinate_point = new EllipseGeometry
            {
                Center = _coordinates,
                RadiusX = 2,
                RadiusY = 2
            };


            ID = new TextBlock
            {
                Text = data.ID.ToString(),
                FontSize = 14,
                Height = 20,
                Width = 40
            };

            Canvas.SetLeft(ID, _coordinates.X + 5);
            Canvas.SetTop(ID, _coordinates.Y - 20);

            // coordinate point
            Path path_coordinate = new Path
            {
                Stroke = Brushes.Black,
                Fill = Brushes.Black,
                StrokeThickness = 1,
                Data = coordinate_point,
            };


            // main circle
            Path path_circle = new Path
            {
                Stroke = Brushes.DarkGray,
                Fill = angle_in_rad > Math.PI ? _brush_oil : _brush_liquid,
                StrokeThickness = 1,
                Data = circle,
            };


            // segment
            PathFigure path_figure = new PathFigure
            {
                StartPoint = new Point(_coordinates.X + _radius, _coordinates.Y),
                Segments = new PathSegmentCollection { arc, polyline }
            };

            Path path_segment = new Path
            {
                Fill = angle_in_rad > Math.PI ? _brush_oil : _brush_liquid,
                StrokeThickness = 1,
                Data = new PathGeometry { Figures = new PathFigureCollection { path_figure } }
            };


            // adding to the path list
            paths.Clear();
            paths.Add(path_circle);
            paths.Add(path_segment);
            paths.Add(path_coordinate);
        }

        public void Select()
        {
            is_selected = true;
            paths[0].Stroke = Brushes.Red;
        }
        public void Unselect()
        {
            is_selected = false;
            paths[0].Stroke = Brushes.DarkGray;
        }




        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
