using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace BubbleChartOilWells.Views.Resource.GenericResource.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class FilepathToFilenameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString().Split('\\').Last();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Trace.TraceError("StringFormatConverter: does not support TwoWay or OneWayToSource bindings.");
            return DependencyProperty.UnsetValue;
        }
    }
}