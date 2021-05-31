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
        public static Bitmap GetMapBitmap(int pixelWidth, int pixelHeight, List<double> z, double nullValue = 9999900)
        {
            var bitmap = new Bitmap(pixelWidth, pixelHeight);
            var x = 0;
            var y = 0;

            var maxValue = z.Where(z => z < nullValue).Max();
            var minValue = z.Min();

            for (int i = 0; i < z.Count; i++)
            {
                if (z[i] != nullValue)
                {
                    bitmap.SetPixel(x, y, ConvertToColor.GetColor(z[i], minValue, maxValue));
                }
                else
                {
                    bitmap.SetPixel(x, y, Color.Transparent);
                }
                
                x++;
                if (x % pixelWidth == 0)
                {
                    x = 0;
                    y++;
                }
            }
            return bitmap;
        }
    }
}
