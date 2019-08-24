using System;
using System.Collections.Generic;

namespace LICC
{
    public readonly partial struct CColor
    {
        private static readonly CColor[] ConsoleColors = {
            new CColor(0x0C0C0C), //Black = 0
            new CColor(0x0037DA), //DarkBlue = 1
            new CColor(0x13A10E), //DarkGreen = 2
            new CColor(0x3A96DD), //DarkCyan = 3
            new CColor(0xC50F1F), //DarkRed = 4
            new CColor(0x891798), //DarkMagenta = 5
            new CColor(0xC19A00), //DarkYellow = 6
            new CColor(0xCCCCCC), //Gray = 7
            new CColor(0x767676), //DarkGray = 8
            new CColor(0x3B79FF), //Blue = 9
            new CColor(0x15C60C), //Green = 10
            new CColor(0x61D6D6), //Cyan = 11
            new CColor(0xE74856), //Red = 12
            new CColor(0xB4009F), //Magenta = 13
            new CColor(0xF9F1A5), //Yellow = 14
            new CColor(0xF2F2F2)  //White = 15
        };

        private static readonly IDictionary<CColor, ConsoleColor> ColorCache = new Dictionary<CColor, ConsoleColor>();

        public readonly byte R, G, B;

        public CColor(byte r, byte g, byte b)
        {
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public CColor(int hex)
        {
            this.R = (byte)((hex & 0xFF0000) >> 16);
            this.G = (byte)((hex & 0xFF00) >> 8);
            this.B = (byte)(hex & 0xFF);
        }

        public static implicit operator CColor(ConsoleColor consoleColor) => FromConsoleColor(consoleColor);

        public static CColor FromConsoleColor(ConsoleColor consoleColor) => ConsoleColors[(int)consoleColor];

        public ConsoleColor ToConsoleColor()
        {
            var @this = this;

            if (!ColorCache.TryGetValue(this, out var retClr))
            {
                ColorCache[this] = retClr = Get();
            }

            return retClr;

            ConsoleColor Get()
            {
                ConsoleColor ret = 0;
                double rr = @this.R, gg = @this.G, bb = @this.B, delta = double.MaxValue;

                foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor)))
                {
                    var n = Enum.GetName(typeof(ConsoleColor), cc);
                    var c = CColor.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
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
