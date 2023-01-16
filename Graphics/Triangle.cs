using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _3dGraphics.Graphics
{
    public struct Triangle
    {
        /* Clockwise ordering */
        private readonly int _v1Index;
        private readonly int _v2Index;
        private readonly int _v3Index;
        //texel indexes associated to the points
        private readonly int _t1Index;
        private readonly int _t2Index;
        private readonly int _t3Index;

        private readonly float _lightIntensity; 

        //
        public int V1Index => _v1Index;
        public int V2Index => _v2Index;
        public int V3Index => _v3Index;
        public int T1Index => _t1Index;
        public int T2Index => _t2Index;
        public int T3Index => _t3Index;

        public float LightIntensity => _lightIntensity;


        public Triangle(int v1Index, int v2Index, int v3Index, int t1Index, int t2Index, int t3Index, float lightInt)
        {
            _v1Index = v1Index;
            _v2Index = v2Index;
            _v3Index = v3Index; 
            _t1Index = t1Index;
            _t2Index = t2Index;
            _t3Index = t3Index;
            _lightIntensity = lightInt;
        }
        
    }
}
