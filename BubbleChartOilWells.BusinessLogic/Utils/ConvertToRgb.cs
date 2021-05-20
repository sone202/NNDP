// TODO:
// 1. куда перенести модели ?

namespace BubbleChartOilWells.BusinessLogic.Utils
{
    public static class ConvertToRgb
    {
        public static Rgb FromHsl(Hsl hsl)
        {
            byte r = 0;
            byte g = 0;
            byte b = 0;

            if (hsl.S == 0)
            {
                r = g = b = (byte)(hsl.L * 255);
            }
            else
            {
                float v1, v2;
                float hue = (float)hsl.H / 360;

                v2 = (hsl.L < 0.5) ? (hsl.L * (1 + hsl.S)) : ((hsl.L + hsl.S) - (hsl.L * hsl.S));
                v1 = 2 * hsl.L - v2;

                r = (byte)(255 * FromHue(v1, v2, hue + (1.0f / 3)));
                g = (byte)(255 * FromHue(v1, v2, hue));
                b = (byte)(255 * FromHue(v1, v2, hue - (1.0f / 3)));
            }

            return new Rgb(r, g, b);
        }

        private static float FromHue(float v1, float v2, float vH)
        {
            if (vH < 0)
                vH += 1;

            if (vH > 1)
                vH -= 1;

            if ((6 * vH) < 1)
                return (v1 + (v2 - v1) * 6 * vH);

            if ((2 * vH) < 1)
                return v2;

            if ((3 * vH) < 2)
                return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

            return v1;
        }
    }

    public class Rgb
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public Rgb(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public bool Equals(Rgb rgb)
        {
            return (R == rgb.R) && (G == rgb.G) && (B == rgb.B);
        }
    }
    public class Hsl
    {
        public int H { get; set; }
        public float S { get; set; }
        public float L { get; set; }

        public Hsl(int h, float s, float l)
        {
            H = h;
            S = s;
            L = l;
        }

        public bool Equals(Hsl hsl)
        {
            return (H == hsl.H) && (S == hsl.S) && (L == hsl.L);
        }
    }
}
