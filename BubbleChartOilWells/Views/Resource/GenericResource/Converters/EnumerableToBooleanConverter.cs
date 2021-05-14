using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;
using System.Collections.Generic;

namespace BubbleChartOilWells.Views.Resource.GenericResource.Converters
{
    [ValueConversion(typeof(IEnumerable<object>), typeof(bool))]
    public class EnumerableToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is IEnumerable<object>))
                return false;

            return (value as IEnumerable<object>).Count() != 0 ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Trace.TraceError("EnumerableToBooleanConverter: does not support TwoWay or OneWayToSource bindings.");
            return DependencyProperty.UnsetValue;
        }
    }
}
