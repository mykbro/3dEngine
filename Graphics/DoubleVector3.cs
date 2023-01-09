using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace _3dGraphics.Graphics
{
    internal struct DoubleVector3
    {
        private readonly Vector<double> _v;

        public double X => _v[0];
        public double Y => _v[1];
        public double Z => _v[2];

        public DoubleVector3(float x, float y, float z)
        {
            _v = new Vector<double>(new double[] {x ,y , z, 0});            
        }

        public DoubleVector3(Vector<double> v)
        {
            _v = v;
        }

        public DoubleVector3(Vector3 v3) : this(v3.X, v3.Y, v3.Z)
        {            
        }  

        public Vector3 ToVector3()
        {
            return new Vector3((float)_v[0], (float)_v[1], (float)_v[2]);
        }

        public static DoubleVector3 operator +(DoubleVector3 a, DoubleVector3 b)
        {            
            return new DoubleVector3(a._v + b._v);
        } 
    }
}
