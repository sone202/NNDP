using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using LiveCharts.Wpf;
using Newtonsoft.Json;

namespace BubbleChartOilWells.Views.Functional
{
    /// <summary>
    /// Interaction logic for DrawingAreaUC.xaml
    /// </summary>
    public partial class DrawingAreaUC : UserControl
    {
        private List<double> xLabels;
        private List<double> yLabels;
        private readonly double marginBottom;
        private readonly double marginLeft;
        private readonly double pointLineLength = 15;
        private readonly int gap = 50;
        private readonly int xN = 50;
        private readonly int yN = 27;
        private Point renderOffset = new Point(0, 0);
        private bool isDataUploaded = false;

        public DrawingAreaUC()
        {
            InitializeComponent();

            marginBottom = BottomRow.Height.Value;
            marginLeft = LeftColumn.Width.Value;

            #region AXES

            xLabels = new List<double>();
            yLabels = new List<double>();

            // Ox point lines
            for (int i = 0; i < xN; i++)
            {
                if (i != 0)
                    AxesCanvas.Children.Add(new Line
                    {
                        X1 = i * gap + marginLeft,
                        Y1 = marginBottom - pointLineLength,
                        X2 = i * gap + marginLeft,
                        Y2 = marginBottom,

                        Stroke = Brushes.Gray,
                        StrokeThickness = 1
                    });
                xLabels.Add(i * gap);
                OxValuesItemsControl.Items.Add(i * gap);

            }

            // Oy point lines
            for (int i = 0; i < yN; i++)
            {
                if (i != 0)
                    AxesCanvas.Children.Add(new Line
                    {
                        X1 = marginLeft - pointLineLength,
                        Y1 = i * gap + marginBottom,
                        X2 = marginLeft,
                        Y2 = i * gap + marginBottom,

                        Stroke = Brushes.Gray,
                        StrokeThickness = 1
                    });
                yLabels.Insert(0, i * gap);
                OyValuesItemsControl.Items.Insert(0, i * gap);
            }
            #endregion
        }

        public void SetMap()
        {
            // setting transform
            MapRectangle.RenderTransform = new TranslateTransform(
                 (DataContext as MainVM).SelectedMap.LeftBottomCoordinate.X - renderOffset.X,
                 (DataContext as MainVM).SelectedMap.LeftBottomCoordinate.Y - renderOffset.Y);
        }

        public void SetOilWell()
        {
            foreach (Canvas oilWell in DrawItemsControl.Items)
            {
                oilWell.RenderTransform = new TranslateTransform(
                    oilWell.RenderTransform.Value.OffsetX - renderOffset.X,
                    oilWell.RenderTransform.Value.OffsetY - renderOffset.Y);
            }
        }

        public void SetStartPoint()
        {
            isDataUploaded = true;

            var offset = 100;

            // first time oilWells import
            if (DrawItemsControl.Items.Count != 0)
            {
                var minX = DrawItemsControl.Items.Cast<Canvas>().Min(x => x.RenderTransform.Value.OffsetX);
                var minY = DrawItemsControl.Items.Cast<Canvas>().Min(y => y.RenderTransform.Value.OffsetY);

                var maxX = DrawItemsControl.Items.Cast<Canvas>().Max(x => x.RenderTransform.Value.OffsetX);
                var maxY = DrawItemsControl.Items.Cast<Canvas>().Max(y => y.RenderTransform.Value.OffsetY);

                renderOffset = new Point(minX + (maxX - minX) / 2 + offset, minY + (maxY - minY) / 2 + offset);

                foreach (Canvas oilWell in DrawItemsControl.Items)
                {
                    oilWell.RenderTransform = new TranslateTransform(
                        oilWell.RenderTransform.Value.OffsetX - renderOffset.X,
                        oilWell.RenderTransform.Value.OffsetY - renderOffset.Y);
                }
            }

            // first time map import
            if ((DataContext as MainVM).SelectedMap != null)
            {
                renderOffset = (DataContext as MainVM).SelectedMap.LeftBottomCoordinate;
                renderOffset.X -= offset;
                renderOffset.Y -= offset;

                //setting transform
                MapRectangle.RenderTransform = new TranslateTransform(offset, offset);
            }

            // labels update
            for (int i = 0; i < OxValuesItemsControl.Items.Count; i++)
                OxValuesItemsControl.Items[i] = xLabels[i] += renderOffset.X;
            for (int i = 0; i < OyValuesItemsControl.Items.Count; i++)
                OyValuesItemsControl.Items[i] = yLabels[i] += renderOffset.Y;
        }

        public void MoveTo(object sender, MouseButtonEventArgs e)
        {

            for (int i = 0; i < OxValuesItemsControl.Items.Count; i++)
                OxValuesItemsControl.Items[i] = xLabels[i] += MapRectangle.RenderTransform.Value.OffsetX - 200;
            for (int i = 0; i < OyValuesItemsControl.Items.Count; i++)
                OyValuesItemsControl.Items[i] = yLabels[i] += MapRectangle.RenderTransform.Value.OffsetY - 200;

            if (MapRectangle != null)
            {
                renderOffset.X += MapRectangle.RenderTransform.Value.OffsetX - 200;
                renderOffset.Y += MapRectangle.RenderTransform.Value.OffsetY - 200;
                SetMap();
            }

            if (DrawItemsControl.Items.Count != 0)
            {
                SetOilWell();
            }
        }

        #region Moving

        private Point mouseDownPoint;
        private Point mouseUpPoint;
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
           
            mouseDownPoint = e.GetPosition(sender as UIElement);
            //int n = (DataContext as MainVM).OilWellVMs.Count();
            int n = 0;
            List<OilWellVM> temp = new List<OilWellVM>();
            



            // TODO: delete
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                var parseJson = $"{ JsonConvert.SerializeObject(OilWellVM.SelectedOilWell, Formatting.None)}";
                parseJson = parseJson.Replace("{", "").Replace("}", "");
                parseJson = parseJson.Replace("[", "").Replace("]", "");
                parseJson = parseJson.Replace("\"", "").Replace("\"", "");
                parseJson = parseJson.Replace(":", ": ");

                (DataContext as MainVM).SelectedOilWell = parseJson.Split(',').ToList();
                var currentPoint = OilWellVM.SelectedOilWell;
                //var monthlyObjectiveProductionDto = new List<MonthlyObjectiveProductionExcelDto>();

                foreach (var nextPoint in (DataContext as MainVM).OilWellVMs)
                {
                    if ((currentPoint.Region == nextPoint.Region)
                        && (currentPoint.Area == nextPoint.Area)
                        && (currentPoint.Field == nextPoint.Field)
                        && (currentPoint.Name != nextPoint.Name))
                    {
                        if (isPointInCircle(currentPoint.X,currentPoint.Y, nextPoint.X,nextPoint.Y))
                        {

                            //nextPoint.Neighbour = true;
                            n++;
                            temp.Add(nextPoint);
                        }
                    }
                }
                
                SelectNearHole(temp);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDataUploaded && e.LeftButton == MouseButtonState.Pressed)
            {
                mouseUpPoint = e.GetPosition(sender as UIElement);

                if (DrawItemsControl.Items.Count != 0)
                {
                    foreach (Canvas oilWell in DrawItemsControl.Items)
                    {
                        if (oilWell != null)
                            oilWell.RenderTransform = new TranslateTransform(
                                oilWell.RenderTransform.Value.OffsetX + mouseUpPoint.X - mouseDownPoint.X,
                                oilWell.RenderTransform.Value.OffsetY + mouseUpPoint.Y - mouseDownPoint.Y);
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

                for (int i = 0; i < OxValuesItemsControl.Items.Count; i++)
                    OxValuesItemsControl.Items[i] = xLabels[i] -= (mouseUpPoint.X - mouseDownPoint.X);
                for (int i = 0; i < OyValuesItemsControl.Items.Count; i++)
                    OyValuesItemsControl.Items[i] = yLabels[i] -= (mouseUpPoint.Y - mouseDownPoint.Y);

                mouseDownPoint = mouseUpPoint;
            }
        }
        #endregion

        #region Scaling
        private void Canvas_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scaleValue = 1.5;

            var canvas = sender as Canvas;
            if (e.Delta > 0)
            {
                canvas.LayoutTransform = new ScaleTransform(
                   canvas.LayoutTransform.Value.M11 * scaleValue,
                    canvas.LayoutTransform.Value.M22 * scaleValue);

                UserIrapMapCanvas.LayoutTransform = canvas.LayoutTransform;
            }
            else if (canvas.LayoutTransform.Value.M11 > 0.000001)
            {
                canvas.LayoutTransform = new ScaleTransform(
                    canvas.LayoutTransform.Value.M11 / scaleValue,
                    canvas.LayoutTransform.Value.M22 / scaleValue);

                UserIrapMapCanvas.LayoutTransform = canvas.LayoutTransform;
            }

            for (int i = 1; i < xN; i++)
            {
                xLabels[i] = xLabels[0] + i * gap / canvas.LayoutTransform.Value.M11;
                OxValuesItemsControl.Items[i] = xLabels[i];
            }
            for (int i = yN - 2; i >= 0; i--)
            {
                yLabels[i] = yLabels[yN - 1] + (yN - 1 - i) * gap / -canvas.LayoutTransform.Value.M22;
                OyValuesItemsControl.Items[i] = yLabels[i];
            }
        }
        #endregion

        public void OilWellMap_Checked(object sender, RoutedEventArgs e)
        {
            
            if (!isDataUploaded)
            {
                SetStartPoint();
            }
            else
                SetOilWell();
        }

        public void Map_Checked(object sender, RoutedEventArgs e)
        {
            (DataContext as MainVM).SelectedMap = (sender as RadioButton).DataContext as MapVM;
            if (!isDataUploaded)
                SetStartPoint();
            else
                SetMap();
        }

        public void MapVMsItemsControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((DataContext as MainVM).SelectedMap != null)
            {
                if (!isDataUploaded)
                    SetStartPoint();
                else
                    SetMap();
            }
        }

        static public bool isPointInCircle(double centerX, double centerY, double x, double y, double R = 500)
        {
            if (isInRectangle(centerX, centerY, R, x, y))
            {
                double dx = centerX - x;
                double dy = centerY - y;
                dx *= dx;
                dy *= dy;
                double distanceSquared = dx + dy;
                double radiusSquared = R * R;
                return distanceSquared <= radiusSquared;
            }
            return false;
        }
        static public bool isInRectangle(double centerX, double centerY, double x, double y, double R = 500)
        {
            return (x >= centerX - R && x <= centerX + R && y >= centerY - R && y <= centerY + R);
        }
        public void SelectNearHole(List<OilWellVM> temp) 
        {
            foreach (var point in (DataContext as MainVM).OilWellVMs)
            {
                point.isNeighbour = false;
            }
            foreach (var point in temp)
            {
                point.isNeighbour = true;
                point.DrawNearHole();               
            }
            
                    
        }
                    
    }
}
    

