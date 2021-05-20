using System.Windows;
using System.Windows.Input;

namespace BubbleChartOilWells.Views.Functional
{
    public partial class ExitConfirmationWindow : Window
    {
        public ExitConfirmationWindow(object dataContext, Window mainWindow)
        {
            Owner = mainWindow;
            DataContext = dataContext;
            InitializeComponent();
        }
        
        #region close buttons
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }
        #endregion

        private void NoSaveButtonClick(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
            SystemCommands.CloseWindow(Owner as MainWindow);
        }
    }
}