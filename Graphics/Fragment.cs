using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dGraphics.Graphics
{
    public struct Fragment
    {
        private readonly Point _p1;
        private readonly Point _p2;
        private readonly Point _p3;
        private readonly float _lightIntensity;

        public Point P1 => _p1;
        public Point P2 => _p2;
        public Point P3 => _p3;
        public float LightIntensity => _lightIntensity;

        public Fragment(Point p1, Point p2, Point p3, float lightIntensity)
        {
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _lightIntensity = lightIntensity;
        }
    }
}
