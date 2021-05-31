using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BubbleChartOilWells.BusinessLogic.Mappers;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using BubbleChartOilWells.ViewModels;
using Newtonsoft.Json;

namespace BubbleChartOilWells.Views.Functional
{
    /// <summary>
    /// Interaction logic for DrawingAreaUC.xaml
    /// </summary>
    public partial class DrawingAreaUC : UserControl
    {
        private const int GAP = 50;
        private const int X_LABELS_COUNT = 50;
        private const int Y_LABELS_COUNT = 27;
        private const double MULTIPLIER = 1.1;
        private const double GAUGE_LENGTH = 15;
        private const double OFFSET = 100;

        private readonly List<double> xAxisLabels;
        private readonly List<double> yAxisLabels;
        private readonly double marginBottom;
        private readonly double marginLeft;
        private Point renderOffset = new Point(0, 0);
        private bool isDataUploaded = false;

        public DrawingAreaUC()
        {
            InitializeComponent();

            marginBottom = BottomRow.Height.Value;
            marginLeft = LeftColumn.Width.Value;

            #region AXES

            xAxisLabels = new List<double>();
            yAxisLabels = new List<double>();

            // Ox point lines
            for (int i = 0; i < X_LABELS_COUNT; i++)
            {
                if (i != 0)
                {
                    AxesCanvas.Children.Add(new Line
                    {
                        X1 = i * GAP + marginLeft,
                        Y1 = marginBottom - GAUGE_LENGTH,
                        X2 = i * GAP + marginLeft,
                        Y2 = marginBottom,

                        Stroke = (Brush)(new BrushConverter().ConvertFrom("#596275")),
                        StrokeThickness = 1
                    });
                }

                xAxisLabels.Add(i * GAP);
                XAxisLabelsItemsControl.Items.Add(i * GAP);
            }

            // Oy point lines
            for (int i = 0; i < Y_LABELS_COUNT; i++)
            {
                if (i != 0)
                {
                    AxesCanvas.Children.Add(new Line
                    {
                        X1 = marginLeft - GAUGE_LENGTH,
                        Y1 = i * GAP + marginBottom,
                        X2 = marginLeft,
                        Y2 = i * GAP + marginBottom,

                        Stroke = (Brush)(new BrushConverter().ConvertFrom("#596275")),
                        StrokeThickness = 1
                    });
                }

                yAxisLabels.Insert(0, i * GAP);
                YAxisLabelsItemsControl.Items.Insert(0, i * GAP);
            }

            #endregion
        }

        public void SetMap()
        {
            if (MapRectangle != null)
            {
                //setting transform
                MapRectangle.RenderTransform = new TranslateTransform(
                    (DataContext as MainVM).SelectedMap.LeftBottomCoordinate.X - renderOffset.X,
                    (DataContext as MainVM).SelectedMap.LeftBottomCoordinate.Y - renderOffset.Y);
            }
        }

        public void SetOilWells()
        {
            if (DrawItemsControl.Items.Count != 0)
            {
                foreach (Canvas oilWell in DrawItemsControl.Items)
                {
                    oilWell.RenderTransform = new TranslateTransform(
                        oilWell.RenderTransform.Value.OffsetX - renderOffset.X,
                        oilWell.RenderTransform.Value.OffsetY - renderOffset.Y);
                }
            }
        }

        public void SetStartPoint()
        {
            isDataUploaded = true;

            // first time oilWells import
            if (DrawItemsControl.Items.Count != 0)
            {
                if (renderOffset.X == 0 && renderOffset.Y == 0)
                {
                    renderOffset.X = DrawItemsControl.Items.Cast<Canvas>().Sum(x => x.RenderTransform.Value.OffsetX) /
                                     DrawItemsControl.Items.Count;
                    renderOffset.Y = DrawItemsControl.Items.Cast<Canvas>().Sum(y => y.RenderTransform.Value.OffsetY) /
                                     DrawItemsControl.Items.Count;
                }

                SetOilWells();
            }

            // first time map import
            if ((DataContext as MainVM).SelectedMap != null)
            {
                if (renderOffset.X == 0 && renderOffset.Y == 0)
                {
                    renderOffset = (DataContext as MainVM).SelectedMap.LeftBottomCoordinate;
                }

                SetMap();
            }

            UpdateLabels(renderOffset.X, renderOffset.Y);
        }

        public void MoveTo(object sender, MouseButtonEventArgs e)
        {
            var localOffsetX = MapRectangle.RenderTransform.Value.OffsetX;
            var localOffsetY = MapRectangle.RenderTransform.Value.OffsetY;

            renderOffset.X += localOffsetX;
            renderOffset.Y += localOffsetY;

            SetMap();
            SetOilWells();

            UpdateLabels(localOffsetX, localOffsetY);
        }

        #region Moving

        private Point mouseDownPoint;
        private Point mouseUpPoint;

        private void CanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseDownPoint = e.GetPosition(sender as UIElement);

            // TODO: delete
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                var parseJson = $"{JsonConvert.SerializeObject(OilWellVM.SelectedOilWell, Formatting.None)}";
                parseJson = parseJson.Replace("{", "").Replace("}", "");
                parseJson = parseJson.Replace("[", "").Replace("]", "");
                parseJson = parseJson.Replace("\"", "").Replace("\"", "");
                parseJson = parseJson.Replace(":", ": ");

                (DataContext as MainVM).SelectedOilWellPropertyValues = parseJson.Split(',').ToList();
            }
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (isDataUploaded && (e.LeftButton == MouseButtonState.Pressed))
            {
                mouseUpPoint = e.GetPosition(sender as UIElement);

                if (DrawItemsControl.Items.Count != 0)
                {
                    foreach (Canvas oilWell in DrawItemsControl.Items)
                    {
                        if (oilWell != null)
                        {
                            oilWell.RenderTransform = new TranslateTransform(
                                oilWell.RenderTransform.Value.OffsetX + mouseUpPoint.X - mouseDownPoint.X,
                                oilWell.RenderTransform.Value.OffsetY + mouseUpPoint.Y - mouseDownPoint.Y);
                        }
                    }
                }

                if (MapRectangle != null)
                {
                    MapRectangle.RenderTransform = new TranslateTransform(
                        MapRectangle.RenderTransform.Value.OffsetX + mouseUpPoint.X - mouseDownPoint.X,
                        MapRectangle.RenderTransform.Value.OffsetY + mouseUpPoint.Y - mouseDownPoint.Y);
                }

                renderOffset.X -= mouseUpPoint.X - mouseDownPoint.X;
                renderOffset.Y -= mouseUpPoint.Y - mouseDownPoint.Y;

                UpdateLabels((mouseDownPoint.X - mouseUpPoint.X),
                    (mouseDownPoint.Y - mouseUpPoint.Y));

                mouseDownPoint = mouseUpPoint;
            }
        }

        #endregion

        private void UpdateLabels(double offsetX, double offsetY)
        {
            for (int i = 0; i < XAxisLabelsItemsControl.Items.Count; i++)
            {
                xAxisLabels[i] += offsetX;
                XAxisLabelsItemsControl.Items[i] = xAxisLabels[i];
            }

            for (int i = 0; i < YAxisLabelsItemsControl.Items.Count; i++)
            {
                yAxisLabels[i] += offsetY;
                YAxisLabelsItemsControl.Items[i] = yAxisLabels[i];
            }
        }

        #region Scaling

        private void CanvasMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (isDataUploaded)
                {
                    if (e.Delta > 0)
                    {
                        foreach (var oilWellVM in (DataContext as MainVM).OilWellVMs)
                        {
                            var oilWellView = oilWellVM.OilWellView;

                            oilWellView.LayoutTransform = new ScaleTransform(
                                oilWellView.LayoutTransform.Value.M11 * MULTIPLIER,
                                oilWellView.LayoutTransform.Value.M22 * MULTIPLIER);
                        }
                    }
                    else
                    {
                        foreach (var oilWellVM in (DataContext as MainVM).OilWellVMs)
                        {
                            var oilWellView = oilWellVM.OilWellView;

                            oilWellView.LayoutTransform = new ScaleTransform(
                                oilWellView.LayoutTransform.Value.M11 / MULTIPLIER,
                                oilWellView.LayoutTransform.Value.M22 / MULTIPLIER);
                        }
                    }
                }
            }
            else
            {
                var canvas = sender as Canvas;
                if (e.Delta > 0)
                {
                    canvas.LayoutTransform = new ScaleTransform(
                        canvas.LayoutTransform.Value.M11 * MULTIPLIER,
                        canvas.LayoutTransform.Value.M22 * MULTIPLIER);

                    UserIrapMapCanvas.LayoutTransform = canvas.LayoutTransform;
                }
                else if (canvas.LayoutTransform.Value.M11 > 0.000001)
                {
                    canvas.LayoutTransform = new ScaleTransform(
                        canvas.LayoutTransform.Value.M11 / MULTIPLIER,
                        canvas.LayoutTransform.Value.M22 / MULTIPLIER);

                    UserIrapMapCanvas.LayoutTransform = canvas.LayoutTransform;
                }

                for (int i = 1; i < X_LABELS_COUNT; i++)
                {
                    xAxisLabels[i] = xAxisLabels[0] + i * GAP / canvas.LayoutTransform.Value.M11;
                    XAxisLabelsItemsControl.Items[i] = xAxisLabels[i];
                }

                for (int i = Y_LABELS_COUNT - 2; i >= 0; i--)
                {
                    yAxisLabels[i] = yAxisLabels[Y_LABELS_COUNT - 1] +
                                     (Y_LABELS_COUNT - 1 - i) * GAP / -canvas.LayoutTransform.Value.M22;
                    YAxisLabelsItemsControl.Items[i] = yAxisLabels[i];
                }
            }
        }

        #endregion

        public void OilWellMapChecked(object sender, RoutedEventArgs e)
        {
            if (!isDataUploaded)
            {
                SetStartPoint();
            }
            else
            {
                SetOilWells();
            }
        }

        public void MapChecked(object sender, RoutedEventArgs e)
        {
            (DataContext as MainVM).SelectedMap = (sender as RadioButton).DataContext as MapVM;
            if (!isDataUploaded)
            {
                SetStartPoint();
            }
            else
            {
                SetMap();
            }
        }

        public void MapVMsItemsControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((DataContext as MainVM).SelectedMap != null)
            {
                if (!isDataUploaded)
                {
                    SetStartPoint();
                }
                else
                {
                    SetMap();
                }
            }
        }
    }
}