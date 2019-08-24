using System;
using System.Drawing;

namespace LICC
{
    public static class ConsoleColorExtensions
    {
        private static int[] Colors = {
            0x000000, //Black = 0
            0x000080, //DarkBlue = 1
            0x008000, //DarkGreen = 2
            0x008080, //DarkCyan = 3
            0x800000, //DarkRed = 4
            0x800080, //DarkMagenta = 5
            0x808000, //DarkYellow = 6
            0xC0C0C0, //Gray = 7
            0x808080, //DarkGray = 8
            0x0000FF, //Blue = 9
            0x00FF00, //Green = 10
            0x00FFFF, //Cyan = 11
            0xFF0000, //Red = 12
            0xFF00FF, //Magenta = 13
            0xFFFF00, //Yellow = 14
            0xFFFFFF  //White = 15
        };

        public static Color ToRGB(this ConsoleColor c) => Color.FromArgb(Colors[(int)c]);

        public static ConsoleColor ToConsoleColor(this Color color)
        {
            ConsoleColor ret = 0;
            double rr = color.R, gg = color.G, bb = color.B, delta = double.MaxValue;

            foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor)))
            {
                var n = Enum.GetName(typeof(ConsoleColor), cc);
                var c = Color.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
                var t = Math.Pow(c.R - rr, 2.0) + Math.Pow(c.G - gg, 2.0) + Math.Pow(c.B - bb, 2.0);
                if (t == 0.0)
                    return cc;
                if (t < delta)
                {
                    delta = t;
                    ret = cc;
                }
            }
            return ret;
        }
    }
}
