using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace BubbleChartOilWells.Views.Resource.GenericResource.Converters
{
    [ValueConversion(typeof(int[]), typeof(string))]
    public class ArrayToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = string.Empty;
            var arr = value as int[];
            for (int i = 0; i < arr.Length; i++)
            {
                result += arr[i].ToString();
                if (i + 1 != arr.Length)
                {
                    result += ", ";
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value.ToString();
            var result = new [] {16, 16};
            try
            {
                result = Array.ConvertAll(str.Split(new string[] {", "}, StringSplitOptions.None), x => int.Parse(x));
            }
            catch
            {
                MessageBox.Show("Введенные данные неверные, количество нейронов изменено не будет");
            }

            return result;
        }
    }
}
