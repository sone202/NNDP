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
        public static double MIN_oil_value = 100000;
        public static double MAX_oil_value = 0;

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
            _radius = Math.Round(100 / MAX_oil_value * (data.oil_prod + data.liquid_prod), 2);
            double angle_in_rad = Math.PI / 180 * (360 / (data.oil_prod + data.liquid_prod)) * data.liquid_prod;



            EllipseGeometry coordinate_point = new EllipseGeometry
            {
                Center = _coordinates,
                RadiusX = 2,
                RadiusY = 2
            };

            // figures
            EllipseGeometry circle = new EllipseGeometry
            {
                Center = _coordinates,
                RadiusX = _radius,
                RadiusY = _radius
            };

            PolyLineSegment polyline = new PolyLineSegment
            {
                Points = new PointCollection {
                _coordinates,
                new Point(_coordinates.X + _radius, _coordinates.Y),
                new Point(_coordinates.X + Math.Cos(angle_in_rad) * _radius, _coordinates.Y + Math.Sin(angle_in_rad) * _radius) }

            };

            ArcSegment arc = new ArcSegment
            {
                Point = new Point(_coordinates.X + Math.Cos(angle_in_rad) * _radius, _coordinates.Y + Math.Sin(angle_in_rad) * _radius),
                Size = new Size(_radius, _radius),
                SweepDirection = angle_in_rad > Math.PI ? SweepDirection.Counterclockwise : SweepDirection.Clockwise
            };



            // coordinate point
            Path path_coordinate_point = new Path
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
                Fill = angle_in_rad > Math.PI ? _brush_liquid : _brush_oil,
                StrokeThickness = 0,
                Data = circle,
            };

            // pie segment
            Path path_segment = new Path
            {
                Fill = angle_in_rad > Math.PI ? _brush_oil : _brush_liquid,
                StrokeThickness = 0,
                Data = new PathGeometry
                {
                    Figures = new PathFigureCollection {
                        new PathFigure {
                            StartPoint = new Point(_coordinates.X + _radius, _coordinates.Y),
                            Segments = new PathSegmentCollection { arc, polyline }
                        }
                    }
                }
            };

            TextBlock ID = new TextBlock
            {
                Text = data.ID.ToString(),
                FontSize = 14,
                Height = 20,
                Width = 40
            };
            Canvas.SetLeft(ID, _coordinates.X + 5);
            Canvas.SetTop(ID, _coordinates.Y - 20);




            // adding to the path list
            paths.Clear();
            paths.Add(path_circle);
            paths.Add(path_segment);
            paths.Add(path_coordinate_point);
            this.ID = ID;
        }

        public void Select()
        {
            is_selected = true;
            paths[0].Stroke = Brushes.Red;
            paths[0].StrokeThickness = 2;
        }
        public void Unselect()
        {
            is_selected = false;
            paths[0].Stroke = Brushes.DarkGray;
            paths[0].StrokeThickness = 0;

        }

        public bool Contains(object sender)
        {
            try
            {
                foreach (var el in paths)
                    if (el == sender)
                        return true;
                if (ID == sender)
                    return true;
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return false;
        }




        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
