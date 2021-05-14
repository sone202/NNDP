using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BubbleChartOilWells.Views.Resource.GenericResource.Converters
{
    [ValueConversion(typeof(object), typeof(string))]
    public class NumberToAxisValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double number = Double.Parse(value.ToString());
            if (Math.Abs(number) >= 1000)
                return Math.Abs(number) >= 1000000 ? (number/ 1000000).ToString("#0.###") + "M" : (number / 1000).ToString("#0.###") + "k";
            else
                return number.ToString("#0.###");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Trace.TraceError("StringFormatConverter: does not support TwoWay or OneWayToSource bindings.");
            return DependencyProperty.UnsetValue;
        }
    }
}