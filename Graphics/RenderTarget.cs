using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace _3dGraphics.Graphics
{
    internal class RenderTarget
    {
        private byte[] _data;        
        private float[] _zBuffer;
        private readonly Object[] _pixelLocks;
        private readonly int _width;
        private readonly int _height;

        public int Width => _width;
        public int Height => _height;
        public int PixelStride => 4;
        public byte[] Data => _data;

        public RenderTarget(int width, int height)
        {
            _width = width;
            _height = height;
            _data = new byte[width * height * PixelStride];
            _zBuffer = new float[width * height];
            _pixelLocks = new Object[width * height];

            InitPixelLocks();
        }

        public void RenderFragment(Triangle fragment, List<Vector4> vertices, Texture texture)
        {
            Vector3 p1 = vertices[fragment.V1Index].ToVector3();
            Vector3 p2 = vertices[fragment.V2Index].ToVector3();
            Vector3 p3 = vertices[fragment.V3Index].ToVector3();

            Vector3 t1 = fragment.T1;
            Vector3 t2 = fragment.T2;
            Vector3 t3 = fragment.T3;            

            //we determine the screen area we need to check by calculating the rectangle that contains the triangle (we're not checking the whole screen...)
            int maxX = (int) Math.Max(Math.Max(p1.X, p2.X), p3.X);
            int maxY = (int) Math.Max(Math.Max(p1.Y, p2.Y), p3.Y);
            int minX = (int) Math.Min(Math.Min(p1.X, p2.X), p3.X);
            int minY = (int) Math.Min(Math.Min(p1.Y, p2.Y), p3.Y);

            //we cache some vector operation
            Vector3 p2_p3 = p2 - p3;    //vector A in AxB
            Vector3 p1_p2 = p1 - p2;
            Vector3 p3_p1 = p3 - p1;
            Vector3 p3_p2 = p3 - p2;
            //these are for barycentric coords using Cramer's rule
            float d00 = p1_p2.X * p1_p2.X + p1_p2.Y * p1_p2.Y;
            float d01 = p1_p2.X * p3_p2.X + p1_p2.Y * p3_p2.Y;
            float d11 = p3_p2.X * p3_p2.X + p3_p2.Y * p3_p2.Y;
            float invDenom = 1f / (d00 * d11 - d01 * d01);
           
            //we project them on the Z=0 plane for the inside/outside check cross product            
            Vector3 projP2_P3 = ProjectTo2D(p2_p3);
            Vector3 projP1_P2 = ProjectTo2D(p1_p2);
            Vector3 projP3_P1 = ProjectTo2D(p3_p1);

            Vector3 projP1 = ProjectTo2D(p1);
            Vector3 projP2 = ProjectTo2D(p2);
            Vector3 projP3 = ProjectTo2D(p3);

            float lightIntensity = fragment.LightIntensity;           

            for (int y = minY; y <= maxY; y++)
            {
                float yAndHalf = y + 0.5f;  //cache result

                bool startingXFound = false;
                bool endingXFound = false;
                int startingX = maxX + 1;   //initialize over Max so that if we do not found any X startingX will be > than endingX and we won't draw anything
                int endingX = minX - 1;     //see above

                Vector3 p = new Vector3(0f, yAndHalf, 0f);

                for (int x = minX; x <= maxX && !startingXFound; x++)
                {
                    //Vector3 p = new Vector3(x + 0.5f, yAndHalf, 0f);
                    p.X = x + 0.5f;

                    //we check if we're inside the triangle
                    //bool pointInsideTriangle = IsPointInTriangleCrossProduct(p, projP1, projP2, projP3, projP1_P2, projP2_P3, projP3_P1);
                    //bool pointInsideTriangle = IsPointInTriangleBarycentric(p, projP1, projP2, projP3, projP1_P2, projP2_P3, projP3_P1);
                    Vector3 projP_P1 = p - projP1;
                    Vector3 projP_P2 = p - projP2;
                    Vector3 projP_P3 = p - projP3;
                    
                    //we inline the crossproduct check calculating only the Z term and early rejecting thanks to the && property
                    bool pointInsideTriangle =  (projP_P1.X * projP3_P1.Y - projP_P1.Y * projP3_P1.X) >= 0 &&
                                                (projP_P2.X * projP1_P2.Y - projP_P2.Y * projP1_P2.X) >= 0 &&
                                                (projP_P3.X * projP2_P3.Y - projP_P3.Y * projP2_P3.X) >= 0;
                   
                    if (pointInsideTriangle)
                    {
                        startingX = x;
                        startingXFound = true;
                    }
                }

                if(startingXFound)      //we skip the search for endingX if we already scanned the line and did not find any startingX
                {
                    for (int x = maxX; x >= minX && !endingXFound; x--)     //decreasing loop
                    {
                        //Vector3 p = new Vector3(x + 0.5f, yAndHalf, 0f);
                        p.X = x + 0.5f;

                        //we check if we're inside the triangle
                        //bool pointInsideTriangle = IsPointInTriangleCrossProduct(p, projP1, projP2, projP3, projP1_P2, projP2_P3, projP3_P1);
                        //bool pointInsideTriangle = IsPointInTriangleBarycentric(p, projP1, projP2, projP3, projP1_P2, projP2_P3, projP3_P1);                        
                        Vector3 projP_P1 = p - projP1;
                        Vector3 projP_P2 = p - projP2;
                        Vector3 projP_P3 = p - projP3;

                        //we inline the crossproduct check calculating only the Z term and early rejecting thanks to the && property                        
                        bool pointInsideTriangle = (projP_P1.X * projP3_P1.Y - projP_P1.Y * projP3_P1.X) >= 0 &&
                                                    (projP_P2.X * projP1_P2.Y - projP_P2.Y * projP1_P2.X) >= 0 &&
                                                    (projP_P3.X * projP2_P3.Y - projP_P3.Y * projP2_P3.X) >= 0;
                        
                        if (pointInsideTriangle)
                        {
                            endingX = x;
                            endingXFound = true;
                        }
                    }
                }

                //we finally draw from startingX to endingX               
                for (int x = startingX; x <= endingX; x++)
                {
                    //we interpolate the point Z using the plane equation (we use P2 and the norm (P1-P2 X P3-P2) to describe the triangle plane)
                    //we then use the equation [(P1-P2 X P3-P2)]*(P-P2) = 0 to derive P.Z                           
                    //Vector3 p = new Vector3(x + 0.5f, yAndHalf, 0f);
                    p.X = x + 0.5f;

                    Vector3 p_p2 = p - p2;

                    //we interpolate the Z coord for the zBuffer check
                    float a = (p1_p2.Y * p3_p2.Z - p1_p2.Z * p3_p2.Y);
                    float b = (p1_p2.Z * p3_p2.X - p1_p2.X * p3_p2.Z);
                    float c = (p1_p2.X * p3_p2.Y - p1_p2.Y * p3_p2.X);
                    //float interpolatedZ = - (a * p_p2.X + b * p_p2.Y) / c + p2.Z;                        
                    //float invertedInterZ = 1 / interpolatedZ;
                    float invertedInterZ = -c / (a * p_p2.X + b * p_p2.Y - c * p2.Z);      //one more mult, but one less division                   

                    //we calculate the pixel number
                    int pixelNr = y * _width + x;

                    //we calculate the barycentric coords with a speculative double-check to avoid wasting time if we're already behind
                    //we take the second locked check only if the first succeeded 
                    if (invertedInterZ > _zBuffer[pixelNr])
                    {
                        float d20 = p_p2.X * p1_p2.X + p_p2.Y * p1_p2.Y;
                        float d21 = p_p2.X * p3_p2.X + p_p2.Y * p3_p2.Y;

                        float v = (d11 * d20 - d01 * d21) * invDenom;           //associated to P1
                        float w = (d00 * d21 - d01 * d20) * invDenom;           //associated to P3
                        float u = 1.0f - v - w;                                 //associated to P2

                        float invertedCorrFactor = 1 / (t1.Z * v + t2.Z * u + t3.Z * w);

                        //we interpolate the texture coordinates
                        //Vector2 pointTexel = v * t1 + u * t2 + w * t3;
                        float texelX = v * t1.X + u * t2.X + w * t3.X;
                        float texelY = v * t1.Y + u * t2.Y + w * t3.Y;

                        //we calculate the perspective correction based on: [1 /(v*T1.Z + u*T2.Z + w*T3.Z)] * (v*T1+ u*T2 + w*T3)
                        float perspCorrectedtX = texelX * invertedCorrFactor;
                        float perspCorrectedtY = texelY * invertedCorrFactor;

                        //we sample the texture at the corrected coords
                        Color pointColor = texture.GetColorNormalizedCoords(perspCorrectedtX, perspCorrectedtY);

                        //lock is A LOT faster than .net Spinlock. Same speed as my AsyncStuff.Spinlock
                        //Using this lock loses around 10fps but is necessary to avoid artifacts                  
                        lock (_pixelLocks[pixelNr])
                        {
                            //we need to check if we're still in front after the first check
                            if (invertedInterZ > _zBuffer[pixelNr])       
                            {
                                _zBuffer[pixelNr] = invertedInterZ;

                                int pixelStartingByte = pixelNr * PixelStride;

                                _data[pixelStartingByte] = (byte) (pointColor.B * lightIntensity);
                                _data[pixelStartingByte + 1] = (byte) (pointColor.G * lightIntensity);
                                _data[pixelStartingByte + 2] = (byte) (pointColor.R * lightIntensity);
                                //_data[pixelStartingByte + 3] = 0;   //alpha, we spare the write
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

        private static bool IsPointInTriangleBarycentric(Vector3 p, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p1_p2, Vector3 p2_p3, Vector3 p3_p1)
        {
            Vector3 p_p3 = p - p3;
            Vector3 p_p2 = p - p2;
            Vector3 p_p1 = p - p1;

            float s = p3_p1.X * p_p1.Y - p3_p1.Y * p_p1.X;
            float t = p1_p2.X * p_p2.Y - p1_p2.Y * p_p2.X;

            if ((s < 0) != (t < 0) && s != 0 && t != 0)
                return false;

            float d = p2_p3.X * p_p3.Y - p2_p3.Y * p_p3.X;
            return d == 0 || (d < 0) == (s + t <= 0);                       
        }

        private static bool IsPointInTriangleCrossProduct(Vector3 p, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p1_p2, Vector3 p2_p3, Vector3 p3_p1)
        {
            Vector3 p_p3 = p - p3;
            Vector3 p_p2 = p - p2;
            Vector3 p_p1 = p - p1;


            return Vector3.Cross(p2_p3, p_p3).Z <= 0 &&    //early reject using && properties
                    Vector3.Cross(p1_p2, p_p2).Z <= 0 &&
                    Vector3.Cross(p3_p1, p_p1).Z <= 0;
        }

        public void Clear()
        {
            /*
            for(int i=0; i<_data.Length; i++)
            {
                _data[i] = 0;                
            }
            */
            _data = new byte[_width * _height * PixelStride];            

            ClearZBuffer();
        }

        public void ClearZBuffer()
        {
            /*
            for (int i = 0; i < _zBuffer.Length; i++)
            {
                _zBuffer[i] = float.PositiveInfinity;
            }
            */
            _zBuffer = new float[_width * _height];
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
