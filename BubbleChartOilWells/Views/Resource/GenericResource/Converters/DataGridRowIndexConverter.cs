using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BubbleChartOilWells.Views.Resource.GenericResource.Converters
{
    public class DataGridRowIndexConverter : IValueConverter
    {
        #region IValueConverter Members
        //Convert the Item to an Index
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                //Get the CollectionView from the DataGrid that is using the converter
                DataGrid dg = (DataGrid)Application.Current.MainWindow.FindName(parameter.ToString());
                if (dg != null)
                {
                    CollectionView cv = (CollectionView)dg.Items;
                    //Get the index of the item from the CollectionView
                    int rowindex = cv.IndexOf(value) + 1;

                    return rowindex.ToString();
                }
                else
                    return 0;
            }
            catch (Exception e)
            {
                throw new NotImplementedException(e.Message);
            }
        }
        //One way binding, so ConvertBack is not implemented
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
