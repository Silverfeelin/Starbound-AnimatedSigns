using System.Drawing;

namespace AnimatedSigns
{
    public static class ColorExt
    {
        /// <summary>
        /// Returns a RRGGBBAA hexadecimal representation of the given <see cref="Color"/>.
        /// </summary>
        /// <param name="c">Color</param>
        /// <returns>String representing the hexadecimal value of the color, formatted RRGGBBAA.</returns>
        public static string ToRGBAHexString(this Color c)
        {
            string r = c.R.ToString("X2");
            string g = c.G.ToString("X2");
            string b = c.B.ToString("X2");
            string a = c.A.ToString("X2");

            return r + g + b + a;
        }

        public static Color[,] CreateTemplate()
        {
            Color[,] templateColors = new Color[32, 8];

            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int x = i;
                    if (i >= 9)
                        x += 6;
                    if (i >= 19)
                        x += 6;
                    if (i >= 29)
                        x += 6;

                    templateColors[i, j] = Color.FromArgb(1, x + 1, 0, j + 1);
                }
            }

            return templateColors;
        }
    }
}
