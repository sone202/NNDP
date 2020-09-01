using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel.Channels;
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
        public static int scale = 2;

        private Brush _brush_oil = Brushes.SaddleBrown;
        private Brush _brush_liquid = Brushes.LightBlue;

        public bool is_selected { get; private set; } = false;

        public List<Path> paths_prod { get; private set; } = new List<Path>();
        public List<Path> paths_debit { get; private set; } = new List<Path>();
        public TextBlock ID { get; private set; }



        //public void Update(OilWell data)
        //{
        //    paths_prod.Clear();
        //    paths_debit.Clear();
        //    ID = null;

        //    paths_prod = CreatePaths(new Point(data.X, data.Y),
        //        (data.oil_prod + data.liquid_prod) / scale,
        //        Math.PI / 180 * (360 / (data.oil_prod + data.liquid_prod)) * data.liquid_prod);
        //    paths_prod.RemoveAt(paths_prod.Count - 1);
        //    paths_prod.RemoveAt(paths_prod.Count - 1);

        //    paths_debit = CreatePaths(new Point(data.X, data.Y),
        //        (data.oil_debit + data.liquid_debit) / scale,
        //        Math.PI / 180 * (360 / (data.oil_debit + data.liquid_debit)) * data.liquid_debit);


        //    ID = new TextBlock
        //    {
        //        Text = data.ID.ToString(),
        //        FontSize = 14,
        //        Height = 20,
        //        Width = 40
        //    };
        //    Canvas.SetLeft(ID, data.X + 5);
        //    Canvas.SetTop(ID, data.Y - 20);
        //}
        public List<Path> CreatePaths(Point Coordinates, double Radius, double Angle)
        {


            EllipseGeometry coordinate_point = new EllipseGeometry
            {
                Center = Coordinates,
                RadiusX = Radius / 10 > 2 ? 2 : Radius / 10,
                RadiusY = Radius / 10 > 2 ? 2 : Radius / 10
            };

            LineGeometry coordinate_cross_hor = new LineGeometry(
                new Point(Coordinates.X - Radius / 3, Coordinates.Y),
                new Point(Coordinates.X + Radius / 3, Coordinates.Y));

            LineGeometry coordinate_cross_ver = new LineGeometry(
                new Point(Coordinates.X, Coordinates.Y - Radius / 3),
                new Point(Coordinates.X, Coordinates.Y + Radius / 3));

            // figures
            EllipseGeometry circle = new EllipseGeometry
            {
                Center = Coordinates,
                RadiusX = Radius,
                RadiusY = Radius
            };

            PolyLineSegment polyline = new PolyLineSegment
            {
                Points = new PointCollection {
                Coordinates,
                new Point(Coordinates.X + Radius, Coordinates.Y),
                new Point(Coordinates.X + Math.Cos(Angle) * Radius, Coordinates.Y + Math.Sin(Angle) * Radius) }

            };

            ArcSegment arc = new ArcSegment
            {
                Point = new Point(Coordinates.X + Math.Cos(Angle) * Radius, Coordinates.Y + Math.Sin(Angle) * Radius),
                Size = new Size(Radius, Radius),
                SweepDirection = Angle > Math.PI ? SweepDirection.Counterclockwise : SweepDirection.Clockwise
            };



            // coordinate point
            Path path_coordinate_point = new Path
            {
                Stroke = Brushes.Black,
                Fill = Brushes.Black,
                StrokeThickness = 0,
                Data = coordinate_point
            };

            Path path_coordinate_cross_hor = new Path
            {
                Stroke = Brushes.Black,
                Fill = Brushes.Black,
                StrokeThickness = 1,
                Data = coordinate_cross_hor
            };

            Path path_coordinate_cross_ver = new Path
            {
                Stroke = Brushes.Black,
                Fill = Brushes.Black,
                StrokeThickness = 1,
                Data = coordinate_cross_ver
            };


            // main circle
            Path path_circle = new Path
            {
                Stroke = Brushes.DarkGray,
                Fill = Angle > Math.PI ? _brush_liquid : _brush_oil,
                StrokeThickness = 1,
                Data = circle,
            };

            // pie segment
            Path path_segment = new Path
            {
                Fill = Angle > Math.PI ? _brush_oil : _brush_liquid,
                StrokeThickness = 0,
                Data = new PathGeometry
                {
                    Figures = new PathFigureCollection {
                        new PathFigure {
                            StartPoint = new Point(Coordinates.X + Radius, Coordinates.Y),
                            Segments = new PathSegmentCollection { arc, polyline }
                        }
                    }
                }
            };

            // adding to the path list
            return new List<Path> { path_circle, path_segment, path_coordinate_point, path_coordinate_cross_hor, path_coordinate_cross_ver };
        }

        public void Select()
        {
            is_selected = true;
            paths_prod[0].Stroke = Brushes.Red;
            paths_debit[0].Stroke = Brushes.Red;
            paths_prod[0].StrokeThickness = 2;
            paths_debit[0].StrokeThickness = 2;
        }
        public void Unselect()
        {
            is_selected = false;
            paths_prod[0].Stroke = Brushes.DarkGray;
            paths_debit[0].Stroke = Brushes.DarkGray;
            paths_prod[0].StrokeThickness = 1;
            paths_debit[0].StrokeThickness = 1;
        }

        public bool Contains(object sender)
        {
            try
            {
                foreach (var el in paths_prod)
                    if (el == sender)
                        return true;

                foreach (var el in paths_debit)
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

        public bool ShowProd(bool val)
        {

            if (paths_prod != null)
            {
                var vs = val ? Visibility.Visible : Visibility.Hidden;
                foreach (var path in paths_prod)
                {
                    path.Visibility = vs;
                }
                return val;
            }
            else
                return false;
        }
        public bool ShowDebit(bool val)
        {

            if (paths_debit != null)
            {
                var vs = val ? Visibility.Visible : Visibility.Hidden;
                foreach (var path in paths_debit)
                {
                    path.Visibility = vs;
                }
                return val;
            }
            else
                return false;
        }
        public bool ShowID(bool showprod, bool showdebit)
        {
            if (showprod == false && showdebit == false)
            {
                ID.Visibility = Visibility.Hidden;
                return false;
            }
            else
            {
                ID.Visibility = Visibility.Visible;
                return true;
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
