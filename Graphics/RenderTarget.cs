using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _3dGraphics.Graphics
{
    internal class RenderTarget
    {
        private byte[] _data;        
        private readonly float[] _zBuffer;
        private readonly Object[] _pixelLocks;
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
            _pixelLocks = new Object[width * height];

            InitPixelLocks();
        }

        public void RenderFragment(Fragment3D fragment)
        {
            Vector3 p1 = fragment.P1;
            Vector3 p2 = fragment.P2;
            Vector3 p3 = fragment.P3;            

            //we determine the screen area we need to check by calculating the rectangle that contains the triangle (we're not checking the whole screen...)
            int maxX = (int) Math.Max(Math.Max(p1.X, p2.X), p3.X);
            int maxY = (int) Math.Max(Math.Max(p1.Y, p2.Y), p3.Y);
            int minX = (int) Math.Min(Math.Min(p1.X, p2.X), p3.X);
            int minY = (int) Math.Min(Math.Min(p1.Y, p2.Y), p3.Y);

            //we cache some vector operation
            Vector3 p2_p3 = p2 - p3;    //vector A in AxB
            Vector3 p1_p2 = p1 - p2;
            Vector3 p3_p1 = p3 - p1;

            
            //we project them on the Z=0 plane for the inside/outside check cross product
            
            Vector3 projP2_P3 = ProjectTo2D(p2_p3);
            Vector3 projP1_P2 = ProjectTo2D(p1_p2);
            Vector3 projP3_P1 = ProjectTo2D(p3_p1);

            Vector3 projP1 = ProjectTo2D(p1);
            Vector3 projP2 = ProjectTo2D(p2);
            Vector3 projP3 = ProjectTo2D(p3);
            

            for (int x = minX; x < maxX + 1; x++)   //x < maxX + 1 is equal to x <= maxX
            {
                for(int y = minY; y < maxY + 1; y++)
                {   
                    //we check if we're inside the screen
                    bool pointInsideScreen = (0 <= x && x < Width && 0 <= y && y < Height);                    

                    if (pointInsideScreen)
                    {
                        Vector3 p = PointToVector3(new Point(x, y));    //already projected on Z=0 plane

                        
                        //we check if we're inside the triangle using cross products
                        
                        Vector3 projP_P3 = p - projP3;
                        Vector3 projP_P2 = p - projP2;
                        Vector3 projP_P1 = p - projP1;
                        
                        /*
                        Vector3 a = Vector3.Cross(projP2_P3, projP_P3);
                        Vector3 b = Vector3.Cross(projP1_P2, projP_P2);
                        Vector3 c = Vector3.Cross(projP3_P1, projP_P1);
                        
                        bool pointInsideTriangle = (a.Z <= 0 && b.Z <= 0 && c.Z <= 0);
                        */

                        bool pointInsideTriangle = (Vector3.Cross(projP2_P3, projP_P3).Z <= 0 &&    //early reject using && properties
                                                    Vector3.Cross(projP1_P2, projP_P2).Z <= 0 &&
                                                    Vector3.Cross(projP3_P1, projP_P1).Z <= 0);
                        
                        
                        //bool pointInsideTriangle = PointInTriangle(p, p1, p2, p3);

                        if (pointInsideTriangle)
                        {
                            
                            //we interpolate the point Z using the plane equation (we use P2 and the norm (P1-P2 X P3-P2) to describe the triangle plane)
                            //we then use the equation [(P1-P2 X P3-P2)]*(P-P2) = 0 to derive P.Z
                            Vector3 p3_p2 = p3 - p2;
                            Vector3 p_p2 = p - p2;

                            float interpolatedZ = -((p1_p2.Y * p3_p2.Z - p1_p2.Z * p3_p2.Y) * p_p2.X + (p1_p2.Z * p3_p2.X - p1_p2.X * p3_p2.Z) * p_p2.Y) / (p1_p2.X * p3_p2.Y - p1_p2.Y * p3_p2.X) + p2.Z;
                            
                            //float interpolatedZ = (p1.Z + p2.Z + p3.Z) / 3;   //simple implementation
                            
                            //we calculate the pixel number
                            int pixelNr = y * Width + x;

                            lock (_pixelLocks[pixelNr])
                            {
                                if (interpolatedZ < _zBuffer[pixelNr])       //we're now using the interpolated Z
                                {
                                    _zBuffer[pixelNr] = interpolatedZ;

                                    int pixelStartingByte = pixelNr * Stride;

                                    _data[pixelStartingByte] = fragment.Color.B;
                                    _data[pixelStartingByte + 1] = fragment.Color.G;
                                    _data[pixelStartingByte + 2] = fragment.Color.R;
                                    //_data[pixelStartingByte + 3] = 0;   //alpha, we spare the write
                                }
                            }                            
                        }
                    }                   
                }
            }   


        }
        private static Vector3 PointToVector3(Point p)
        {
            //return new Vector3((2 * p.X + 1) / 2f, (2 * p.Y + 1) / 2f, 0f);
            return new Vector3(p.X + 0.5f, p.Y + 0.5f, 0f);
        }   
        
        private static Vector3 ProjectTo2D(Vector3 p)
        {
            return new Vector3(p.X, p.Y, 0f);
        }

        private static bool PointInTriangle(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2)
        {            
            //not much advantage using this over crossproduct with early reject
            float s = (p0.X - p2.X) * (p.Y - p2.Y) - (p0.Y - p2.Y) * (p.X - p2.X);
            float t = (p1.X - p0.X) * (p.Y - p0.Y) - (p1.Y - p0.Y) * (p.X - p0.X);

            if ((s < 0) != (t < 0) && s != 0 && t != 0)
                return false;

            float d = (p2.X - p1.X) * (p.Y - p1.Y) - (p2.Y - p1.Y) * (p.X - p1.X);
            return d == 0 || (d < 0) == (s + t <= 0);                       
        }

        public void Clear()
        {
            /*
            for(int i=0; i<_data.Length; i++)
            {
                _data[i] = 0;                
            }
            */
            _data = new byte[_width * _height * Stride];            

            ClearZBuffer();
        }

        public void ClearZBuffer()
        {
            for (int i = 0; i < _zBuffer.Length; i++)
            {
                _zBuffer[i] = float.PositiveInfinity;
            }
        }

        private void InitPixelLocks()
        {
            for(int i=0; i< _pixelLocks.Length; i++) 
            {
                _pixelLocks[i] = new Object();
            }
        }
       



    }
}
