using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace _3dGraphics.Graphics
{
    public struct Fragment
    {
        private readonly PointF _p1;
        private readonly PointF _p2;
        private readonly PointF _p3;
        private readonly float _lightIntensity;

        public PointF P1 => _p1;
        public PointF P2 => _p2;
        public PointF P3 => _p3;
        public float LightIntensity => _lightIntensity;

        public Fragment(PointF p1, PointF p2, PointF p3, float lightIntensity)
        {
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _lightIntensity = lightIntensity;
        }
    }
}
