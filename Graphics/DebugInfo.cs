using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dGraphics.Graphics
{
    public class DebugInfo      //not a struct because we need to share this by reference
    {
        public int ObjectsRendered;         
        public int NumVerticesFromObjects;
        public int NumTrianglesFromObjects;
        public int NumTrianglesSentToClip;
        public int NumTrianglesSentToRender;

        public DebugInfo() 
        {
            NumVerticesFromObjects = 0;
            NumTrianglesFromObjects = 0;
            NumTrianglesSentToClip = 0;
            NumTrianglesSentToRender = 0;
        }
    }
}
