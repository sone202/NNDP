using BubbleChartOilWells.BusinessLogic.Mappers;
using BubbleChartOilWells.Contracts.Models.ViewModels;
using BubbleChartOilWells.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using LiveCharts;
using BubbleChartOilWells.Views.Functional;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace BubbleChartOilWells
{
    public partial class MainWindow : Window
    {
        static double radius = 300;
        static string radiusString = "";
       


        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainVM();                       

            AutoMapperConfigurator.Initialize();
        }
        

        public void OilWellMap_Checked(object sender, RoutedEventArgs e)
        {
            ProdMapRadioButton.IsEnabled = true;
            ProdSumMapRadioButton.IsEnabled = true;
            BubbleEnableCheckBox.IsEnabled = true;

            DrawingAreaUserControl.OilWellMap_Checked(sender, e);
        }

        private void MapVMsItemsControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawingAreaUserControl.MapVMsItemsControl_SizeChanged(sender, e);
        }

        public void Map_Checked(object sender, RoutedEventArgs e)
        {
            DrawingAreaUserControl.Map_Checked(sender, e);
        }
        public void MoveTo(object sender, MouseButtonEventArgs e)
        {
            DrawingAreaUserControl.MoveTo(sender, e);
        }

        private void ExportOilWellMapValuesButtonClick(object sender, RoutedEventArgs e)
        {
            ExportOilMapValuesWindow ExportOilMapValuesWindow = new ExportOilMapValuesWindow(DataContext);
            ExportOilMapValuesWindow.Show();
            MainWindowCloseButton.IsEnabled = false;
            ExportOilWellMapValuesButton.IsEnabled = false;
        }

        #region mininmize / maximize / close buttons
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e) => SystemCommands.MinimizeWindow(this);
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
            {
                SystemCommands.MaximizeWindow(this);
            }
            else
            {
                SystemCommands.RestoreWindow(this);
            }
        }
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e) => SystemCommands.CloseWindow(this);
        #endregion       
        
        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);
        }

       

        public void SearchNearHole_new(object sender, RoutedEventArgs e)
        {
                             
        }        

        private void InputRadius(object sender, TextCompositionEventArgs e)
        {           
            if ((e.Text).All(char.IsDigit) && (e.Text) != null)
            {
                radiusString += e.Text.ToString();
                radius = Convert.ToDouble(radiusString);               
                DrawingAreaUserControl.ChangeRadius(radius);
            }
            else
            {
                MessageBox.Show("Необходимо вводить только цифры ");
            }
        }        

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Back || e.Key == Key.Delete)
                {
                    radiusString = radiusString.Remove(radiusString.Length - 1);
                }
            }
            catch
            {
                radiusString = "";
            }
        }
    }
}