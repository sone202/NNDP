using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BubbleChartOilWells.Views.Resource.GenericResource.Converters
{
    public class ToCanvasConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var oilWellVMs = (value as IEnumerable<BubbleChartOilWells.Contracts.Models.ViewModels.OilWellVM>);
                var oilWellViews = oilWellVMs.Select(x => x.OilWellView).AsEnumerable();

                return oilWellViews;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
