using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _3dGraphics.Graphics
{
    internal class RenderTarget
    {
        private readonly byte[] _data;        
        private readonly float[] _zBuffer;
        private readonly int _width;
        private readonly int _height;

        public int Width => _width;
        public int Height => _height;
        public int Stride => 4;
        public byte[] Data => _data;

        public RenderTarget(int width, int height)
        {
            _width = width;
            _height = height;
            _data = new byte[width * height * Stride];
            _zBuffer = new float[width * height];            
        }

        public void RenderFragment(Fragment3D fragment)
        {
            Point p1_2D = new Point((int)fragment.P1.X, (int)fragment.P1.Y);
            Point p2_2D = new Point((int)fragment.P2.X, (int)fragment.P2.Y);
            Point p3_2D = new Point((int)fragment.P3.X, (int)fragment.P3.Y);

            int maxX = Math.Max(Math.Max(p1_2D.X, p2_2D.X), p3_2D.X);
            int maxY = Math.Max(Math.Max(p1_2D.Y, p2_2D.Y), p3_2D.Y);
            int minX = Math.Min(Math.Min(p1_2D.X, p2_2D.X), p3_2D.X);
            int minY = Math.Min(Math.Min(p1_2D.Y, p2_2D.Y), p3_2D.Y);

            //these 3 used for the cross product to check point inside/outside
            Vector3 p1 = PointToVector3(p1_2D);
            Vector3 p2 = PointToVector3(p2_2D);
            Vector3 p3 = PointToVector3(p3_2D);

            Vector3 p2_p3 = p2 - p3;    //vector A in AxB
            Vector3 p1_p2 = p1 - p2;
            Vector3 p3_p1 = p3 - p1;

            for (int x = minX; x < maxX + 1; x++)
            {
                for(int y = minY; y < maxY + 1; y++)
                {
                    Vector3 p = PointToVector3(new Point(x, y));

                    bool insideScreen = (0 <= x && x < Width && 0 <= y && y < Height);

                    if (insideScreen)
                    {
                        Vector3 a = Vector3.Cross(p2_p3, p - p3);
                        Vector3 b = Vector3.Cross(p1_p2, p - p2);
                        Vector3 c = Vector3.Cross(p3_p1, p - p1);

                        bool insideTriangle = a.Z <= 0 && b.Z <= 0 && c.Z <= 0;

                        if (insideTriangle)
                        {
                            int pixelNr = (y * Width + x) * Stride;
                            
                            _data[pixelNr] = fragment.Color.B;
                            _data[pixelNr+1] = fragment.Color.G;
                            _data[pixelNr+2] = fragment.Color.R;
                            _data[pixelNr + 3] = 0;   //alpha
                        }
                    }                   
                }
            }   


        }
        private static Vector3 PointToVector3(Point p)
        {
            return new Vector3((2 * p.X + 1) / 2, (2 * p.Y + 1) / 2, 0f);
        }       

        public void Clear()
        {
            for(int i=0; i<_data.Length; i++)
            {
                _data[i] = 0;                
            }            
        }

        public void ClearZBuffer()
        {
            for (int i = 0; i < _zBuffer.Length; i++)
            {
                _zBuffer[i] = float.PositiveInfinity;
            }
        }
       



    }
}
