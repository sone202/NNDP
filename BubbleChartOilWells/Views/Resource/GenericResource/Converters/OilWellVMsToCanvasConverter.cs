using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace BubbleChartOilWells.Views.Resource.GenericResource.Converters
{
    [ValueConversion(typeof(IEnumerable<Contracts.Models.ViewModels.OilWellVM>), typeof(System.Windows.Controls.Canvas))]
    public class OilWellVMsToCanvasConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var oilWellVMs = (value as IEnumerable<Contracts.Models.ViewModels.OilWellVM>);
                var oilWellViews = oilWellVMs.Select(x => x.OilWellView).AsEnumerable();

                return oilWellViews;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Trace.TraceError("OilWellVMsToCanvasConverter: does not support TwoWay or OneWayToSource bindings.");
            return DependencyProperty.UnsetValue;
        }
    }
}
