using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace _3dGraphics.Graphics
{
    public struct Fragment3D
    {
        private readonly Vector3 _p1;
        private readonly Vector3 _p2;
        private readonly Vector3 _p3;
        private readonly Color _color;

        public Vector3 P1 => _p1;
        public Vector3 P2 => _p2;
        public Vector3 P3 => _p3;
        public Color Color => _color;

        public Fragment3D(Vector3 p1, Vector3 p2, Vector3 p3, Color color)
        {
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _color= color;
        }
    }
}
