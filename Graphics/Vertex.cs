using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _3dGraphics.Graphics
{
    public struct Vertex
    {
        private readonly Vector3 _position;

        public Vector3 Position3D => _position;
        public Vector4 Position4D => new Vector4(_position, 1.0f);

        public Vertex(Vector3 pos) 
        {
            _position = pos;
        }

        public override string ToString()
        {
            return _position.ToString();
        }
    }
}
