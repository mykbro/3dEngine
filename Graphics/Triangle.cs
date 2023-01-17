using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _3dGraphics.Graphics
{
    public class Triangle
    {
        /* Clockwise ordering */
        private readonly int _v1Index;
        private readonly int _v2Index;
        private readonly int _v3Index;
        //texel associated to the points
        private readonly Vector3 _t1;
        private readonly Vector3 _t2;
        private readonly Vector3 _t3;

        private readonly float _lightIntensity; 

        //
        public int V1Index => _v1Index;
        public int V2Index => _v2Index;
        public int V3Index => _v3Index;
        public Vector3 T1 => _t1;
        public Vector3 T2 => _t2;
        public Vector3 T3 => _t3;

        public float LightIntensity => _lightIntensity;


        public Triangle(int v1Index, int v2Index, int v3Index, Vector3 t1, Vector3 t2, Vector3 t3, float lightInt)
        {
            _v1Index = v1Index;
            _v2Index = v2Index;
            _v3Index = v3Index;
            _t1 = t1;
            _t2 = t2;
            _t3 = t3;
            _lightIntensity = lightInt;
        }
        
    }
}
