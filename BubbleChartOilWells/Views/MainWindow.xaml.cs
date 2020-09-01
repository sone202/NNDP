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

        #region mininmize / maximize / close buttons
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
        #endregion
       
        private void drawing_area_MouseMove(object sender, MouseEventArgs e) { label_coordinates.Content = e.GetPosition(drawing_area); }
    }
}
