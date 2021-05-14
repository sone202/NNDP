using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BubbleChartOilWells.Views.Functional;

namespace BubbleChartOilWells.Views.Resource.GenericResource.Converters
{
    /// <summary>
    /// converts Row index of datagrid to string
    /// </summary>
    [ValueConversion(typeof(DataGridRow), typeof(int))]
    public class DataGridRowIndexConverter : IValueConverter
    {
        
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                // TODO: refactor
                var userControls = new List<UserControl>();
                userControls.Add((UserControl)Application.Current.MainWindow.FindName("DrawingAreaUserControl"));
                userControls.Add((UserControl)Application.Current.MainWindow.FindName("CalculationsUserControl"));
                userControls.Add((UserControl)Application.Current.MainWindow.FindName("NeuralNetUserControl"));

                var dataGrid = new DataGrid();
                foreach (var userControl in userControls)
                {
                    if ((DataGrid)userControl.FindName(parameter.ToString()) != null)
                    {
                        dataGrid = (DataGrid)userControl.FindName(parameter.ToString());
                        break;
                    }
                }

                if (dataGrid != null)
                {
                    var collectionView = dataGrid.Items;
                    // Get the index of the item from the CollectionView
                    var rowIndex = collectionView.IndexOf(value) + 1;

                    return rowIndex.ToString();
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception e)
            {
                throw new NotImplementedException(e.Message);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
