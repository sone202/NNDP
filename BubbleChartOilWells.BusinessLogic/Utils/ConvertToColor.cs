using System.Drawing;

namespace BubbleChartOilWells.BusinessLogic.Utils
{
    public static class ConvertToColor
    {
        public static Color GetColor(double value, double minValue, double maxValue)
        {
            var valueStep = (maxValue - minValue) / 360;
            var normValue = value - minValue;
            var angle = (int)(normValue / valueStep);

            var hsl = new Hsl(angle, 1f, 0.45f);
            var rgb = ConvertToRgb.FromHsl(hsl);
            return Color.FromArgb(rgb.R, rgb.G, rgb.B);
        }
    }
}
