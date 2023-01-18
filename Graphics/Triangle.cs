using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _3dGraphics.Graphics
{
    //probably better as a struct
    public class Triangle
    {
        /* Clockwise ordering */
        private int _v1Index;
        private int _v2Index;
        private int _v3Index;
        //texel associated to the points
        private Vector3 _t1;
        private Vector3 _t2;
        private Vector3 _t3;

        private float _lightIntensity; 

        //
        public int V1Index { get => _v1Index; set => _v1Index = value; }
        public int V2Index { get => _v2Index; set => _v2Index = value; }
        public int V3Index { get => _v3Index; set => _v3Index = value; }
        public Vector3 T1 { get => _t1; set => _t1 = value; }
        public Vector3 T2 { get => _t2; set => _t2 = value; }
        public Vector3 T3 { get => _t3; set => _t3 = value; }
        

        public float LightIntensity { get => _lightIntensity; set => _lightIntensity = value; }


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
        
        public Triangle(Triangle t) : this(t._v1Index, t._v2Index, t._v3Index, t._t1, t._t2, t._t3, t._lightIntensity)
        { 
        }
        
    }
}
