using BubbleChartOilWells.Contracts;
using BubbleChartOilWells.Contracts.Dto;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BubbleChartOilWells.ViewModels
{
    public class OilWellVM : BaseVM
    {
        static public double MaxOilValue = -1;

        private readonly Brush oilBrush = Brushes.SaddleBrown;
        private readonly Brush waterBrush = Brushes.LightBlue;
        private readonly string statusImage;
        private ObservableCollection<object> SumPath;
        private ObservableCollection<object> DebitPath;

        public bool IsSelected { get; private set; } = false;
        public ObservableCollection<object> Paths { get; set; }
        public Point Coordinates { get; private set; }


        public TextBlock OilWellNameTextBlock { get; private set; }
        public OilWellDto OilWellDto { get; set; }

        public OilWellVM(OilWellDto oilWellDto)
        {
            OilWellDto = oilWellDto;
            Paths = new ObservableCollection<object>();


            Coordinates = new Point(oilWellDto.CoordinateX, oilWellDto.CoordinateY);

            SumPath = GetPath(
                oilWellDto.Objectives[0].MonthlyObjectiveProduction.LiquidProdSum / 500,
                oilWellDto.Objectives[0].MonthlyObjectiveProduction.WaterProdSum / 500,
                oilWellDto.Objectives[0].MonthlyObjectiveProduction.OilProdSum / 500,
                Brushes.Green,
                Brushes.Black);

            //DebitPath = GetPath(
            //    oilWellDto.Objectives[0].MonthlyObjectiveProduction.LiquidDebit / 500,
            //    oilWellDto.Objectives[0].MonthlyObjectiveProduction.WaterDebit / 500,
            //    oilWellDto.Objectives[0].MonthlyObjectiveProduction.OilDebit / 500,
            //    Brushes.Yellow,
            //    Brushes.Red);



            Paths = SumPath;
        }

        public void Select()
        {
            IsSelected = true;
            //Paths[0].Stroke = Brushes.Red;
            //Paths[0].StrokeThickness = 2;
        }
        public void Unselect()
        {
            IsSelected = false;
            //Paths[0].Stroke = Brushes.DarkGray;
            //Paths[0].StrokeThickness = 0;
        }
        public bool Contains(object sender)
        {
            try
            {
                foreach (var el in Paths)
                    if (el == sender)
                        return true;
                if (OilWellNameTextBlock == sender)
                    return true;
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return false;
        }

        private ObservableCollection<object> GetPath(double liquid, double water, double oil, Brush waterBrush, Brush oilBrush)
        {
            double radius = Math.Round(liquid, 2);
            double angleInRad = Math.Round((Math.PI / 180) * (360 / liquid) * water, 2);
            //var r = new Random();
            //var radius = r.Next(10, 30);
            //double angleInRad = (Math.PI / 180) * (360 / (30 + 40)) * 100;

            //statusImage = GetStatusImage(oilWellDto.OilWellStatus);

            // main circle (отображение одного круга)
            Path pathCircle = new Path
            {
                Fill = angleInRad > Math.PI ? waterBrush : oilBrush,
                StrokeThickness = 0,
                Data = new EllipseGeometry
                {
                    RadiusX = radius,
                    RadiusY = radius
                },
                RenderTransform = new TranslateTransform(Coordinates.X, Coordinates.Y)
            };

            // pie segment
            Path pathSegment = GetSegment(angleInRad, radius, waterBrush, oilBrush);

            Image center = new Image
            {
                RenderTransform = new TranslateTransform
                (Coordinates.X,
                Coordinates.Y),
                Source = new BitmapImage(new Uri("C:\\Users\\timzl\\source\\repos\\BubbleChartOilWells\\BubbleChartOilWells\\Resources\\Images\\tr-exp-liq.png"))
            };



            //OilWellNameTextBlock = new TextBlock
            //{
            //    Text = oilWellDto.Name,
            //    FontSize = 14,
            //    Height = 20,
            //    Width = 40
            //};
            //Canvas.SetLeft(OilWellNameTextBlock, coordinates.X + 5);
            //Canvas.SetTop(OilWellNameTextBlock, coordinates.Y - 20);


            return new ObservableCollection<object> { pathCircle, pathSegment };
        }
        private Path GetSegment(double angle, double radius, Brush waterBrush, Brush oilBrush)
        {
            var resPath = new Path
            {

                Fill = angle > Math.PI ? oilBrush : waterBrush,
                StrokeThickness = 0,
                Data = new PathGeometry
                {

                    Figures = new PathFigureCollection {
                        new PathFigure {
                            StartPoint = new Point(Coordinates.X + radius, Coordinates.Y),
                            Segments = new PathSegmentCollection {
                                new ArcSegment
                                {
                                    Point = new Point(Math.Round(Coordinates.X + Math.Cos(angle) * radius,2), Math.Round(Coordinates.Y + Math.Sin(angle) * radius,2)),
                                    Size = new Size(radius, radius),
                                    SweepDirection = angle > Math.PI ? SweepDirection.Counterclockwise : SweepDirection.Clockwise
                                },
                                new PolyLineSegment
                                {
                                    Points = new PointCollection {
                                    new Point(Coordinates.X, Coordinates.Y),
                                    new Point(Coordinates.X + radius, Coordinates.Y),
                                    new Point(Math.Round(Coordinates.X + Math.Cos(angle) * radius,2), Math.Round(Coordinates.Y + Math.Sin(angle) * radius,2)) }
                                }
                            }
                        }
                    }
                }
            };
            return resPath;
        }

        private string GetStatusImage(OilWellStatus status)
        {
            var result = string.Empty;

            switch (status)
            {
                case OilWellStatus.TrExpLiq:
                    result = "Resources/Images/tr-exp-liq.png";
                    break;
                case OilWellStatus.Tr:
                    result = "Resources/Images/tr.png";
                    break;
                    // TODO: остальные картинки
            }

            return result;
        }
    }
}
