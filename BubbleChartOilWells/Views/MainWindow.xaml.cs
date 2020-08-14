using BubbleChartOilWells.ViewModels;
using Microsoft.Build.Tasks;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace BubbleChartOilWells
{

    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }

        // mininmize / maximize / close buttons
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) { e.CanExecute = true; }
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e) { SystemCommands.MinimizeWindow(this); }
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized)
            {
                SystemCommands.MaximizeWindow(this);
                icon_maximize.Source = new BitmapImage(new Uri("Icons/restore.png", UriKind.Relative));
            }
            else
            {
                SystemCommands.RestoreWindow(this);
                icon_maximize.Source = new BitmapImage(new Uri("Icons/maximize.png", UriKind.Relative));
            }
        }
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e) { SystemCommands.CloseWindow(this); }
        private void CommandBinding_Executed_MouseDown(object sender, ExecutedRoutedEventArgs e)
        {
            var data = e;
        }


        //// drawing area events
        //private void drawing_area_MouseDown(object sender, MouseButtonEventArgs e)
        //{

        //    bool path_is_found = false;
        //    for (int i = 0; i < drawing_area.Children.Count; i++)
        //    {
        //        if (drawing_area.Children[i] is Path)
        //        {
        //            if (drawing_area.Children[i].IsMouseOver)
        //            {
        //                // info about selected oil well
        //                border_info.Visibility = Visibility.Visible;
        //                textblock_info.Text =
        //                    "Номер скважины: " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.ID +
        //                    "\nX = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.X +
        //                    "\nY = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.Y +
        //                    "\nТекущий дебит нефти = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.Oil_Debit + " т/сут" +
        //                    "\nТекущий дебит жидкости = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.Liquid_Debit + " т/сут" +
        //                    "\nНакопленная добыча нефти = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.Oil_Production + " тыс. т" +
        //                    "\nНакопленная добыча жидкости = " + _data_path_bubble[(drawing_area.Children[i] as Path)]._data.Liquid_Production + " тыс. т";

        //                // unselecting previous selected oilwell
        //                if (_current_selected != null)
        //                    _current_selected.Unselect();

        //                // selecting current well
        //                _data_path_bubble[(drawing_area.Children[i] as Path)].Select();
        //                _current_selected = _data_path_bubble[(drawing_area.Children[i] as Path)];


        //                path_is_found = true;
        //                break;
        //            }

        //        }
        //    }
        //    if (!path_is_found)
        //    {
        //        // unselecting previous selected oilwell
        //        if (_current_selected != null)
        //            _current_selected.Unselect();

        //        // hiding info
        //        border_info.Visibility = Visibility.Hidden;
        //    }
        //}

        private void drawing_area_MouseMove(object sender, MouseEventArgs e) { label_coordinates.Content = e.GetPosition(drawing_area); }

    }
}
