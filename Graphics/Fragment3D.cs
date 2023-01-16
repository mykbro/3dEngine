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

        private readonly Vector2 _t1;
        private readonly Vector2 _t2;
        private readonly Vector2 _t3;

        private readonly Color _color;

        // 
        public Vector3 P1 => _p1;
        public Vector3 P2 => _p2;
        public Vector3 P3 => _p3;
        public Vector2 T1 => _t1;
        public Vector2 T2 => _t2;
        public Vector2 T3 => _t3;
        public Color Color => _color;

        //
        public Fragment3D(Vector3 p1, Vector3 p2, Vector3 p3, Vector2 t1, Vector2 t2, Vector2 t3, Color color)
        {
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _t1 = t1;
            _t2 = t2;
            _t3 = t3;
            _color= color;
        }
    }
}
