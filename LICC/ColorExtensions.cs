using System;
using System.Collections.Generic;
using System.Drawing;

namespace LICC
{
    public static class ConsoleColorExtensions
    {
        private static readonly int[] Colors = {
            0x0C0C0C, //Black = 0
            0x0037DA, //DarkBlue = 1
            0x13A10E, //DarkGreen = 2
            0x3A96DD, //DarkCyan = 3
            0xC50F1F, //DarkRed = 4
            0x891798, //DarkMagenta = 5
            0xC19A00, //DarkYellow = 6
            0xCCCCCC, //Gray = 7
            0x767676, //DarkGray = 8
            0x3B79FF, //Blue = 9
            0x15C60C, //Green = 10
            0x61D6D6, //Cyan = 11
            0xE74856, //Red = 12
            0xB4009F, //Magenta = 13
            0xF9F1A5, //Yellow = 14
            0xF2F2F2  //White = 15
        };
        private static readonly IDictionary<Color, ConsoleColor> ColorCache = new Dictionary<Color, ConsoleColor>();

        public static Color ToRGB(this ConsoleColor c) => Color.FromArgb(Colors[(int)c]);

        public static ConsoleColor ToConsoleColor(this Color color)
        {
            if (!ColorCache.TryGetValue(color, out var retClr))
            {
                ColorCache[color] = retClr = Get();
            }

            return retClr;

            ConsoleColor Get()
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
}
