using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BubbleChartOilWells.BusinessLogic.Utils
{
    public static class ConvertToBitmap
    {
        /// <summary>
        /// Creates and returns bitmap by converting values to colors
        /// </summary>
        /// <param name="pictureWidth"></param>
        /// <param name="pictureHeight"></param>
        /// <param name="mapValues"></param>
        /// <param name="mapNullValue"></param>
        /// <returns></returns>
        public static Bitmap GetBitmap(int pictureWidth, int pictureHeight, List<double> mapValues, double mapNullValue = 9999900)
        {
            var bitmap = new Bitmap(pictureWidth, pictureHeight);
            var x = 0;
            var y = 0;

            var maxValue = mapValues.Where(z => z != mapNullValue).Max();
            var minValue = mapValues.Where(z => z != mapNullValue).Min();

            for (int i = 0; i < mapValues.Count; i++)
            {
                if (mapValues[i] != mapNullValue)
                {
                    bitmap.SetPixel(x, y, ConvertToColor.GetColor(mapValues[i], minValue, maxValue));
                }
                else
                {
                    bitmap.SetPixel(x, y, Color.Transparent);
                }

                x++;
                if (x % pictureWidth == 0)
                {
                    x = 0;
                    y++;
                }
            }
            return bitmap;
        }
    }
}
