using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace _3dGraphics.Graphics
{
    //probably better as a class
    public class Fragment3D 
    {
        private readonly Vector3 _p1;
        private readonly Vector3 _p2;
        private readonly Vector3 _p3;

        private readonly Vector3 _t1;
        private readonly Vector3 _t2;
        private readonly Vector3 _t3;

        private readonly float _lightIntensity;

        // 
        public Vector3 P1 => _p1;
        public Vector3 P2 => _p2;
        public Vector3 P3 => _p3;
        public Vector3 T1 => _t1;
        public Vector3 T2 => _t2;
        public Vector3 T3 => _t3;
        public float LightIntensity => _lightIntensity;

        //
        public Fragment3D(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 t1, Vector3 t2, Vector3 t3, float lightIntensity)
        {
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _t1 = t1;
            _t2 = t2;
            _t3 = t3;
            _lightIntensity = lightIntensity;
        }
    }
}
