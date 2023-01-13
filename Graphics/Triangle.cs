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
        private readonly Color _color; 

        public int V1Index => _v1Index;
        public int V2Index => _v2Index;
        public int V3Index => _v3Index;    
        public Color Color => _color;


        public Triangle(int v1Index, int v2Index, int v3Index, Color color)
        {
            _v1Index = v1Index;
            _v2Index = v2Index;
            _v3Index = v3Index;
            _color = color;
        }
        
    }
}
