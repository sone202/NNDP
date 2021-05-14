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

namespace BubbleChartOilWells
{
    public partial class MainWindow : Window
    {
    public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainVM();

            AutoMapperConfigurator.Initialize();
        }

        public void OilWellMapChecked(object sender, RoutedEventArgs e)
        {
            ProdMapRadioButton.Visibility = Visibility.Visible;
            ProdSumMapRadioButton.Visibility = Visibility.Visible;
            BubbleEnableCheckBox.IsEnabled = true;

            DrawingAreaUserControl.OilWellMapChecked(sender, e);
        }

        private void MapVMsItemsControlSizeChanged(object sender, SizeChangedEventArgs e) => DrawingAreaUserControl.MapVMsItemsControlSizeChanged(sender, e);

        public void MapChecked(object sender, RoutedEventArgs e)
        {
            DrawingAreaUserControl.MapChecked(sender, e);
        }
        public void MoveTo(object sender, MouseButtonEventArgs e) => DrawingAreaUserControl.MoveTo(sender, e);

        private void ExportOilWellMapValuesButtonClick(object sender, RoutedEventArgs e)
        {
            ExportOilMapValuesWindow exportOilMapValuesWindow = new ExportOilMapValuesWindow(DataContext);
            exportOilMapValuesWindow.Show();
            MainWindowCloseButton.IsEnabled = false;
            ExportOilWellMapValuesButton.IsEnabled = false;
        }
        private void SaveMapButtonClick(object sender, RoutedEventArgs e)
        {
            SaveMapWindow saveMapWindow = new SaveMapWindow(DataContext);
            saveMapWindow.Show();
            MainWindowCloseButton.IsEnabled = false;
            SaveMapButton.IsEnabled = false;
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
        private void RadiusInput_method(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IstextNumeric(e.Text);
        }
        private static bool IstextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);
        }



        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e) => SystemCommands.CloseWindow(this);
        #endregion
    }
}