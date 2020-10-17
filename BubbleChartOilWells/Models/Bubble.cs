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
        public static double MINOilValue = 100000;
        public static double MAXOilValue = 0;

        private Point coordinates;
        private double radius;

        private Brush brushOil = Brushes.SaddleBrown;
        private Brush brushLiquid = Brushes.LightBlue;

        public OilWell Data { get; private set; }

        public bool IsSelected { get; private set; } = false;

        public List<Path> Paths { get; private set; } = new List<Path>();

        public TextBlock ID { get; private set; }

        public Bubble(OilWell data) { Data = data; }

        public void Update()
        {
            coordinates = new Point(Data.X, Data.Y);
            radius = Math.Round(100 / MAXOilValue * (Data.oil_prod + Data.liquid_prod), 2);
            double angleInRad = Math.PI / 180 * (360 / (Data.oil_prod + Data.liquid_prod)) * Data.liquid_prod;



            EllipseGeometry coordinatePoint = new EllipseGeometry
            {
                Center = coordinates,
                RadiusX = 2,
                RadiusY = 2
            };

            // figures
            EllipseGeometry circle = new EllipseGeometry
            {
                Center = coordinates,
                RadiusX = radius,
                RadiusY = radius
            };

            PolyLineSegment polyline = new PolyLineSegment
            {
                Points = new PointCollection {
                coordinates,
                new Point(coordinates.X + radius, coordinates.Y),
                new Point(coordinates.X + Math.Cos(angleInRad) * radius, coordinates.Y + Math.Sin(angleInRad) * radius) }

            };

            ArcSegment arc = new ArcSegment
            {
                Point = new Point(coordinates.X + Math.Cos(angleInRad) * radius, coordinates.Y + Math.Sin(angleInRad) * radius),
                Size = new Size(radius, radius),
                SweepDirection = angleInRad > Math.PI ? SweepDirection.Counterclockwise : SweepDirection.Clockwise
            };



            // coordinate point
            Path pathCoordinatePoint = new Path
            {
                Stroke = Brushes.Black,
                Fill = Brushes.Black,
                StrokeThickness = 1,
                Data = coordinatePoint,
            };

            // main circle
            Path pathCircle = new Path
            {
                Stroke = Brushes.DarkGray,
                Fill = angleInRad > Math.PI ? brushLiquid : brushOil,
                StrokeThickness = 0,
                Data = circle,
            };

            // pie segment
            Path pathSegment = new Path
            {
                Fill = angleInRad > Math.PI ? brushOil : brushLiquid,
                StrokeThickness = 0,
                Data = new PathGeometry
                {
                    Figures = new PathFigureCollection {
                        new PathFigure {
                            StartPoint = new Point(coordinates.X + radius, coordinates.Y),
                            Segments = new PathSegmentCollection { arc, polyline }
                        }
                    }
                }
            };

            TextBlock ID = new TextBlock
            {
                Text = Data.ID.ToString(),
                FontSize = 14,
                Height = 20,
                Width = 40
            };
            Canvas.SetLeft(ID, coordinates.X + 5);
            Canvas.SetTop(ID, coordinates.Y - 20);




            // adding to the path list
            Paths.Clear();
            Paths.Add(pathCircle);
            Paths.Add(pathSegment);
            Paths.Add(pathCoordinatePoint);
            this.ID = ID;
        }

        public void Select()
        {
            IsSelected = true;
            Paths[0].Stroke = Brushes.Red;
            Paths[0].StrokeThickness = 2;
        }
        public void Unselect()
        {
            IsSelected = false;
            Paths[0].Stroke = Brushes.DarkGray;
            Paths[0].StrokeThickness = 0;

        }

        public bool Contains(object sender)
        {
            try
            {
                foreach (var el in Paths)
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
