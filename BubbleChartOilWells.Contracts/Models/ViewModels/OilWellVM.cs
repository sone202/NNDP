using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BubbleChartOilWells.Contracts.Models.ViewModels
{
    /// <summary>
    /// Скважина.
    /// </summary>
    public class OilWellVM : INotifyPropertyChanged
    {
        public static OilWellVM SelectedOilWell { get; set; }

        /// <summary>
        /// Регион
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Месторождение
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Площадь
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// Скважина
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Координата X
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Координата Y
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Статус которому сопоставляется условное обозначение скважины на карте
        /// </summary>
        public OilWellStatus OilWellStatus { get; set; }

        /// <summary>
        /// Объекты учета добычи
        /// </summary>
        public List<ObjectiveVM> Objectives { get; set; }

        // TODO: Need refactoring
        [JsonIgnore] private Canvas oilWellView;

        [JsonIgnore]
        public Canvas OilWellView
        {
            get => oilWellView;
            set
            {
                oilWellView = value;
                OnPropertyChanged(nameof(OilWellView));
            }
        }

        [JsonIgnore] public Image StatusImage { get; set; }

        public void CreateOilWellView()
        {
            var textBlock = GetTextBlock(0);

            StatusImage = GetStatusImage(0);
            StatusImage.MouseDown += new System.Windows.Input.MouseButtonEventHandler(OnClick);

            OilWellView = new Canvas
            {
                Children =
                {
                    StatusImage,
                    textBlock
                },
                Background = Brushes.Transparent,
                RenderTransform = new TranslateTransform(X, Y)
            };
        }

        public void CreateOilWellProdView(int multiplierCoefficient)
        {
            var waterBrush = Brushes.Green;
            var oilBrush = Brushes.LightGray;

            var liquidProd = Objectives.Sum(x => x.MonthlyObjectiveProduction.LiquidProd) / multiplierCoefficient;
            var waterProd = Objectives.Sum(x => x.MonthlyObjectiveProduction.WaterProd) / multiplierCoefficient;

            double radius = liquidProd;
            double angle = 360 * (waterProd / liquidProd);
            double radians = (Math.PI / 180) * angle;

            var circlePath = GetCircle(radius, radians, waterBrush, oilBrush);
            var sectorPath = GetSector(radius, radians, waterBrush, oilBrush);
            var textBlock = GetTextBlock(radius);

            StatusImage = GetStatusImage(radius);
            StatusImage.MouseDown += new System.Windows.Input.MouseButtonEventHandler(OnClick);
            OilWellView = new Canvas
            {
                Children =
                {
                    circlePath,
                    sectorPath,
                    StatusImage,
                    textBlock
                },
                Background = Brushes.Transparent,
                RenderTransform = new TranslateTransform(X, Y)
            };
        }

        public void CreateOilWellProdSumView(int multiplierCoefficient)
        {
            var waterBrush = Brushes.Blue;
            var oilBrush = Brushes.LightGray;

            var liquidProdSum = Objectives.Sum(x => x.MonthlyObjectiveProduction.LiquidProdSum) / multiplierCoefficient;
            var waterProdSum = Objectives.Sum(x => x.MonthlyObjectiveProduction.WaterProdSum) / multiplierCoefficient;

            double radius = liquidProdSum;
            double angle = 360 * (waterProdSum / liquidProdSum);
            double radians = (Math.PI / 180) * angle;

            var circlePath = GetCircle(radius, radians, waterBrush, oilBrush);
            var sectorPath = GetSector(radius, radians, waterBrush, oilBrush);
            var textBlock = GetTextBlock(radius);

            StatusImage = GetStatusImage(radius);
            StatusImage.MouseDown += new System.Windows.Input.MouseButtonEventHandler(OnClick);
            OilWellView = new Canvas
            {
                Children =
                {
                    circlePath,
                    sectorPath,
                    StatusImage,
                    textBlock
                },
                Background = Brushes.Transparent,
                RenderTransform = new TranslateTransform(X, Y)
            };
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            SelectedOilWell = this;
            OnPropertyChanged(nameof(SelectedOilWell));
        }

        private Path GetCircle(double radius, double radians, Brush waterBrush, Brush oilBrush)
        {
            var circle = new Path
            {
                Data = new EllipseGeometry
                {
                    RadiusX = radius,
                    RadiusY = radius
                },
                Fill = radians > Math.PI ? waterBrush : oilBrush,
                StrokeThickness = 0
            };
            return circle;
        }

        private Path GetSector(double radius, double radians, Brush waterBrush, Brush oilBrush)
        {
            var sectorPath = new Path
            {
                Data = new PathGeometry
                {
                    Figures = new PathFigureCollection
                    {
                        new PathFigure
                        {
                            StartPoint = new Point(0, 0),
                            Segments = new PathSegmentCollection
                            {
                                new LineSegment(new Point(radius, 0), false),
                                new ArcSegment
                                {
                                    Point = new Point(Math.Cos(radians) * radius, Math.Sin(radians) * radius),
                                    Size = new Size(radius, radius),
                                    SweepDirection = radians > Math.PI
                                        ? SweepDirection.Counterclockwise
                                        : SweepDirection.Clockwise
                                },
                                new LineSegment(new Point(Math.Cos(radians) * radius, Math.Sin(radians) * radius),
                                    false),
                            }
                        }
                    }
                },
                Fill = radians > Math.PI ? oilBrush : waterBrush,
                StrokeThickness = 0,
            };
            return sectorPath;
        }

        private Image GetStatusImage(double radius)
        {
            System.Drawing.Bitmap bitmapImage = null;

            switch (OilWellStatus)
            {
                case OilWellStatus.TrExpLiq:
                    bitmapImage = Properties.Resources._1_tr_exp_liq;
                    break;
                case OilWellStatus.Tr:
                    bitmapImage = Properties.Resources._2_tr;
                    break;
                case OilWellStatus.ExtPLiq:
                    bitmapImage = Properties.Resources._3_ext_pending_liq;
                    break;
                case OilWellStatus.ExtLiq:
                    bitmapImage = Properties.Resources._4_ext_liq;
                    break;
                case OilWellStatus.TrLiq:
                    bitmapImage = Properties.Resources._5_tr_liq;
                    break;
                case OilWellStatus.InjInactTY:
                    bitmapImage = Properties.Resources._6_inj_inact_this_year;
                    break;
                case OilWellStatus.EDCPPLiq:
                    bitmapImage = Properties.Resources._7_edcp_pending_liq;
                    break;
                case OilWellStatus.BHPInactTY:
                    bitmapImage = Properties.Resources._8_bhp_inact_this_year;
                    break;
                case OilWellStatus.Piez:
                    bitmapImage = Properties.Resources._9_piez;
                    break;
                case OilWellStatus.EDCPInactTY:
                    bitmapImage = Properties.Resources._10_edcp_inact_prev_years;
                    break;
                case OilWellStatus.BHP:
                    bitmapImage = Properties.Resources._11_bhp;
                    break;
                case OilWellStatus.BHPInactPY:
                    bitmapImage = Properties.Resources._12_bhp_inact_prev_years;
                    break;
                case OilWellStatus.EDCPInactPY:
                    bitmapImage = Properties.Resources._13_edcp_inact_prev_years;
                    break;
                case OilWellStatus.BHPInAccum:
                    bitmapImage = Properties.Resources._14_bhp_in_accum;
                    break;
                case OilWellStatus.InjLiq:
                    bitmapImage = Properties.Resources._15_inj_liq;
                    break;
                case OilWellStatus.EDCPInAccum:
                    bitmapImage = Properties.Resources._16_edcp_in_accum;
                    break;
                case OilWellStatus.BHPInCons:
                    bitmapImage = Properties.Resources._17_bhp_in_cons;
                    break;
                case OilWellStatus.InjStopped:
                    bitmapImage = Properties.Resources._18_inj_stopped;
                    break;
                case OilWellStatus.Inj:
                    bitmapImage = Properties.Resources._19_inj;
                    break;
                case OilWellStatus.BHPPLiq:
                    bitmapImage = Properties.Resources._20_bhp_pending_liq;
                    break;
                case OilWellStatus.ExtInCons:
                    bitmapImage = Properties.Resources._21_ext_in_cons;
                    break;
                case OilWellStatus.InjInactPY:
                    bitmapImage = Properties.Resources._22_inj_inact_prev_years;
                    break;
                case OilWellStatus.BHPStopped:
                    bitmapImage = Properties.Resources._23_bhp_stopped;
                    break;
                case OilWellStatus.InjPLiq:
                    bitmapImage = Properties.Resources._24_inj_pending_liq;
                    break;
                case OilWellStatus.EDCP:
                    bitmapImage = Properties.Resources._25_edcp;
                    break;
            }

            var imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmapImage.GetHbitmap(),
                IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            var statusImage = new Image
            {
                Source = imageSource,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Height = radius == 0 ? 20 : radius / 5,
                Width = radius == 0 ? 20 : radius / 5
            };
            statusImage.RenderTransform = new TranslateTransform(
                -statusImage.Width / 2,
                -statusImage.Height / 2);

            return statusImage;
        }

        private TextBlock GetTextBlock(double radius)
        {
            // name of the oilWell
            var name = new TextBlock
            {
                Text = Name,
                FontSize = radius == 0 ? 20 : radius / 5,
                RenderTransform = new TranslateTransform(radius == 0 ? 5 : (radius / 8),
                    radius == 0 ? 5 : (radius / 8)),
                LayoutTransform = new MatrixTransform(1, 0, 0, -1, 0, 0)
            };
            return name;
        }

        public void Select()
        {
            double radius = 30;
            OilWellView.Children.Insert(0, new Path
            {
                Data = new EllipseGeometry
                {
                    RadiusX = radius,
                    RadiusY = radius
                },
                Fill = Brushes.Transparent,
                Stroke = Brushes.Red,
                StrokeThickness = 4
            });
            OnPropertyChanged(nameof(OilWellView));
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}