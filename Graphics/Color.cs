﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dGraphics.Graphics
{
    public struct Color
    {
        private readonly byte _alpha;
        private readonly byte _red;
        private readonly byte _green;
        private readonly byte _blue;

        public byte A => _alpha;
        public byte R => _red;
        public byte G => _green;
        public byte B => _blue;

        public Color(byte a, byte r, byte g, byte b)
        {
            _alpha = a;
            _red = r;
            _green = g;
            _blue = b;
        }

        public static Color FromRgb(int r, int g, int b)
        {
            return new Color(255, (byte) r, (byte) g, (byte) b);
        }
    }
}