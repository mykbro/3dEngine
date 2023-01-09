using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dGraphics.Graphics
{
    internal static class Utility
    {
        public static float DegToRad(float angle)
        {
            return (MathF.PI / 180) * angle;
        }
    }
}
