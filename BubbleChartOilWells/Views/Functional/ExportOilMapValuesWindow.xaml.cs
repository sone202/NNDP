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
using System.Windows.Shapes;

namespace BubbleChartOilWells.Views.Functional
{
    /// <summary>
    /// Interaction logic for ExportOilMapValuesWindow.xaml
    /// </summary>
    public partial class ExportOilMapValuesWindow : Window
    {
        public ExportOilMapValuesWindow(object dataContext, Window mainWindow)
        {
            Owner = mainWindow;
            DataContext = dataContext;

            InitializeComponent();
        }

        #region close buttons
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e) => SystemCommands.CloseWindow(this);

        #endregion
    }
}