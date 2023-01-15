using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
            Vector3 p3_p2 = p3 - p2;

            //we project them on the Z=0 plane for the inside/outside check cross product            
            Vector3 projP2_P3 = ProjectTo2D(p2_p3);
            Vector3 projP1_P2 = ProjectTo2D(p1_p2);
            Vector3 projP3_P1 = ProjectTo2D(p3_p1);

            Vector3 projP1 = ProjectTo2D(p1);
            Vector3 projP2 = ProjectTo2D(p2);
            Vector3 projP3 = ProjectTo2D(p3);


            Color fragmentColor = fragment.Color;

            for (int y = minY; y <= maxY; y++)
            {
                float yAndHalf = y + 0.5f;  //cache result

                bool startingXFound = false;
                bool endingXFound = false;
                int startingX = maxX;   //initialize to Max so that if we do not found any X startingX will be > than endingX and we won't draw anything
                int endingX = minX;     //see above

                for (int x = minX; x <= maxX && !startingXFound; x++)
                {
                    Vector3 p = new Vector3(x + 0.5f, yAndHalf, 0f);

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
                        Vector3 p = new Vector3(x + 0.5f, yAndHalf, 0f);

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
                for(int x = startingX; x <= endingX; x++)
                {
                    //we interpolate the point Z using the plane equation (we use P2 and the norm (P1-P2 X P3-P2) to describe the triangle plane)
                    //we then use the equation [(P1-P2 X P3-P2)]*(P-P2) = 0 to derive P.Z                           
                    Vector3 p = new Vector3(x + 0.5f, yAndHalf, 0f);

                    Vector3 p_p2 = p - p2;

                    float a = (p1_p2.Y * p3_p2.Z - p1_p2.Z * p3_p2.Y);
                    float b = (p1_p2.Z * p3_p2.X - p1_p2.X * p3_p2.Z);
                    float c = (p1_p2.X * p3_p2.Y - p1_p2.Y * p3_p2.X);
                    //float interpolatedZ = - (a * p_p2.X + b * p_p2.Y) / c + p2.Z;                        
                    //float invertedInterZ = 1 / interpolatedZ;

                    float invertedInterZ = -c / (a * p_p2.X + b * p_p2.Y - c * p2.Z);      //one more mult, but one less division

                    //we calculate the pixel number
                    int pixelNr = y * _width + x;

                    //lock is A LOT faster than .net Spinlock. Same speed as my AsyncStuff.Spinlock
                    //Using this lock loses around 10fps but is necessary to avoid artifacts
                    lock (_pixelLocks[pixelNr])
                    {
                        if (invertedInterZ > _zBuffer[pixelNr])       //we're now using the interpolated Z
                        {
                            _zBuffer[pixelNr] = invertedInterZ;

                            int pixelStartingByte = pixelNr * Stride;

                            _data[pixelStartingByte] = fragmentColor.B;
                            _data[pixelStartingByte + 1] = fragmentColor.G;
                            _data[pixelStartingByte + 2] = fragmentColor.R;
                            //_data[pixelStartingByte + 3] = 0;   //alpha, we spare the write
                        }
                    }
                }              
            }
        }

        public void RenderFragmentUsingScanLine(Fragment3D fragment)
        {
            Vector3[] pointsFloat = new Vector3[]{ fragment.P1, fragment.P2, fragment.P3 };           
            Color fragmentColor = fragment.Color;

            Point[] pointsInt = new Point[] { new Point((int) pointsFloat[0].X, (int)pointsFloat[0].Y), new Point((int)pointsFloat[1].X, (int)pointsFloat[1].Y), new Point((int)pointsFloat[2].X, (int)pointsFloat[2].Y) }; 
            
            int highestPointIndex = HighestYPoint(pointsInt);
            int lowestPointIndex = LowestYPoint(pointsInt);
            int middlePointIndex = MiddleYPoint(highestPointIndex, lowestPointIndex);

            //some caching
            Vector3 highestPoint = pointsFloat[highestPointIndex];
            Vector3 middlePoint = pointsFloat[middlePointIndex];
            Vector3 lowestPoint = pointsFloat[lowestPointIndex];

            Point highestPointInt = pointsInt[highestPointIndex];
            Point middlePointInt = pointsInt[middlePointIndex];
            Point lowestPointInt = pointsInt[lowestPointIndex];

            //we calculate and cache some vectors for zBuff check
            Vector3 p1_p2 = pointsFloat[0] - pointsFloat[1];            
            Vector3 p3_p2 = pointsFloat[2] - pointsFloat[1];

            //we compare the rows
            if (highestPointInt.Y == lowestPointInt.Y)
            {   
                //we draw a line
                int y = highestPointInt.Y;
                int fromX = Math.Min(Math.Min(pointsInt[0].X, pointsInt[1].X), pointsInt[2].X);
                int toX = Math.Max(Math.Max(pointsInt[0].X, pointsInt[1].X), pointsInt[2].X);
                float yAndHalf = y + 0.5f;
               
                for (int x = fromX; x <= toX; x++)
                {
                    Vector3 p = new Vector3(x + 0.5f, yAndHalf, 0f);

                    Vector3 p_p2 = p - pointsFloat[1];

                    float a = (p1_p2.Y * p3_p2.Z - p1_p2.Z * p3_p2.Y);
                    float b = (p1_p2.Z * p3_p2.X - p1_p2.X * p3_p2.Z);
                    float c = (p1_p2.X * p3_p2.Y - p1_p2.Y * p3_p2.X);
                    //float interpolatedZ = - (a * p_p2.X + b * p_p2.Y) / c + p2.Z;                        
                    //float invertedInterZ = 1 / interpolatedZ;

                    float invertedInterZ = -c / (a * p_p2.X + b * p_p2.Y - c * pointsFloat[1].Z);      //one more mult, but one less division

                    //we calculate the pixel number
                    int pixelNr = y * _width + x;

                    //lock is A LOT faster than .net Spinlock. Same speed as my AsyncStuff.Spinlock
                    //Using this lock loses around 10fps but is necessary to avoid artifacts
                    lock (_pixelLocks[pixelNr])
                    {
                        if (invertedInterZ > _zBuffer[pixelNr])       //we're now using the interpolated Z
                        {
                            _zBuffer[pixelNr] = invertedInterZ;

                            int pixelStartingByte = pixelNr * Stride;

                            _data[pixelStartingByte] = fragmentColor.B;
                            _data[pixelStartingByte + 1] = fragmentColor.G;
                            _data[pixelStartingByte + 2] = fragmentColor.R;
                            //_data[pixelStartingByte + 3] = 0;   //alpha, we spare the write
                        }
                    }
                }                 
            }
            else if (middlePointInt.Y == lowestPointInt.Y)
            {
                //we draw a bottom triangle 
                int highestY = highestPointInt.Y;
                int lowestY = lowestPointInt.Y;               

                float lowestX = Math.Min(middlePoint.X, lowestPoint.X);
                float highestX = Math.Max(middlePoint.X, lowestPoint.X);

                float toMinDX = (highestPoint.X - lowestX) / (highestPoint.Y - middlePoint.Y);     //using middlePoint.Y or lowestPoint.Y is the same :)
                float toMaxDX = (highestPoint.X - highestX) / (highestPoint.Y - middlePoint.Y);

                float fromX = highestPointInt.X;
                float toX = highestPointInt.X;

                for (int y = highestY; y <= lowestY; y++)
                {
                    float yAndHalf = y + 0.5f;

                    if(y == lowestY)
                    {
                        fromX = lowestX;
                        toX = highestX;
                    }
                    
                    for (int x = (int) fromX; x <= (int) toX; x++)
                    {
                        Vector3 p = new Vector3(x + 0.5f, yAndHalf, 0f);

                        Vector3 p_p2 = p - pointsFloat[1];

                        float a = (p1_p2.Y * p3_p2.Z - p1_p2.Z * p3_p2.Y);
                        float b = (p1_p2.Z * p3_p2.X - p1_p2.X * p3_p2.Z);
                        float c = (p1_p2.X * p3_p2.Y - p1_p2.Y * p3_p2.X);
                        //float interpolatedZ = - (a * p_p2.X + b * p_p2.Y) / c + p2.Z;                        
                        //float invertedInterZ = 1 / interpolatedZ;

                        float invertedInterZ = -c / (a * p_p2.X + b * p_p2.Y - c * pointsFloat[1].Z);      //one more mult, but one less division

                        //we calculate the pixel number
                        int pixelNr = y * _width + x;

                        //lock is A LOT faster than .net Spinlock. Same speed as my AsyncStuff.Spinlock
                        //Using this lock loses around 10fps but is necessary to avoid artifacts
                        lock (_pixelLocks[pixelNr])
                        {
                            if (invertedInterZ > _zBuffer[pixelNr])       //we're now using the interpolated Z
                            {
                                _zBuffer[pixelNr] = invertedInterZ;

                                int pixelStartingByte = pixelNr * Stride;

                                _data[pixelStartingByte] = fragmentColor.B;
                                _data[pixelStartingByte + 1] = fragmentColor.G;
                                _data[pixelStartingByte + 2] = fragmentColor.R;
                                //_data[pixelStartingByte + 3] = 0;   //alpha, we spare the write
                            }
                        }
                    }

                    fromX += toMinDX;
                    toX += toMaxDX;
                }

            }
            else if (highestPointInt.Y == middlePointInt.Y)
            {
                //we draw a top triangle 
                int highestY = middlePointInt.Y;
                int lowestY = lowestPointInt.Y;

                float lowestX = Math.Min(highestPoint.X, middlePoint.X);
                float highestX = Math.Max(highestPoint.X, middlePoint.X);

                float fromMinDX = (lowestX - lowestPoint.X) / (middlePoint.Y - lowestPoint.Y);
                float fromMaxDX = (highestX - lowestPoint.X) / (middlePoint.Y - lowestPoint.Y);

                float fromX = lowestX;
                float toX = highestX;

                for (int y = highestY; y <= lowestY; y++)
                {
                    float yAndHalf = y + 0.5f;

                    if (y == lowestY)
                    {
                        fromX = lowestPoint.X;
                        toX = lowestPoint.X;
                    }

                    for (int x = (int) fromX; x <= (int) toX; x++)
                    {
                        Vector3 p = new Vector3(x + 0.5f, yAndHalf, 0f);

                        Vector3 p_p2 = p - pointsFloat[1];

                        float a = (p1_p2.Y * p3_p2.Z - p1_p2.Z * p3_p2.Y);
                        float b = (p1_p2.Z * p3_p2.X - p1_p2.X * p3_p2.Z);
                        float c = (p1_p2.X * p3_p2.Y - p1_p2.Y * p3_p2.X);
                        //float interpolatedZ = - (a * p_p2.X + b * p_p2.Y) / c + p2.Z;                        
                        //float invertedInterZ = 1 / interpolatedZ;

                        float invertedInterZ = -c / (a * p_p2.X + b * p_p2.Y - c * pointsFloat[1].Z);      //one more mult, but one less division

                        //we calculate the pixel number
                        int pixelNr = y * _width + x;

                        //lock is A LOT faster than .net Spinlock. Same speed as my AsyncStuff.Spinlock
                        //Using this lock loses around 10fps but is necessary to avoid artifacts
                        lock (_pixelLocks[pixelNr])
                        {
                            if (invertedInterZ > _zBuffer[pixelNr])       //we're now using the interpolated Z
                            {
                                _zBuffer[pixelNr] = invertedInterZ;

                                int pixelStartingByte = pixelNr * Stride;

                                _data[pixelStartingByte] = fragmentColor.B;
                                _data[pixelStartingByte + 1] = fragmentColor.G;
                                _data[pixelStartingByte + 2] = fragmentColor.R;
                                //_data[pixelStartingByte + 3] = 0;   //alpha, we spare the write
                            }
                        }
                    }

                    fromX += fromMinDX;
                    toX += fromMaxDX;                  
                }
            }
            else
            {
                //we draw both a bottom (minus one row) and a top triangle                   
                float toLowDX = (highestPoint.X - lowestPoint.X) / (highestPoint.Y - lowestPoint.Y);
                float oppositeMiddleX = highestPoint.X + toLowDX * (middlePoint.Y - highestPoint.Y);

                int highestY = highestPointInt.Y;
                int lowestY = middlePointInt.Y;

                float lowestX = Math.Min(middlePoint.X, oppositeMiddleX);
                float highestX = Math.Max(middlePoint.X, oppositeMiddleX);

                float toMinDX = (highestPoint.X - lowestX) / (highestPoint.Y - middlePoint.Y);     
                float toMaxDX = (highestPoint.X - highestX) / (highestPoint.Y - middlePoint.Y);

                float fromX = highestPointInt.X;
                float toX = highestPointInt.X;

                for (int y = highestY; y < lowestY; y++)        //we skip the last row
                {
                    float yAndHalf = y + 0.5f;

                    /* no need for this as we skip the last row
                    if (y == lowestY)
                    {
                        fromX = lowestX;
                        toX = highestX;
                    }
                    */

                    for (int x = (int)fromX; x <= (int)toX; x++)
                    {
                        Vector3 p = new Vector3(x + 0.5f, yAndHalf, 0f);

                        Vector3 p_p2 = p - pointsFloat[1];

                        float a = (p1_p2.Y * p3_p2.Z - p1_p2.Z * p3_p2.Y);
                        float b = (p1_p2.Z * p3_p2.X - p1_p2.X * p3_p2.Z);
                        float c = (p1_p2.X * p3_p2.Y - p1_p2.Y * p3_p2.X);
                        //float interpolatedZ = - (a * p_p2.X + b * p_p2.Y) / c + p2.Z;                        
                        //float invertedInterZ = 1 / interpolatedZ;

                        float invertedInterZ = -c / (a * p_p2.X + b * p_p2.Y - c * pointsFloat[1].Z);      //one more mult, but one less division

                        //we calculate the pixel number
                        int pixelNr = y * _width + x;

                        //lock is A LOT faster than .net Spinlock. Same speed as my AsyncStuff.Spinlock
                        //Using this lock loses around 10fps but is necessary to avoid artifacts
                        lock (_pixelLocks[pixelNr])
                        {
                            if (invertedInterZ > _zBuffer[pixelNr])       //we're now using the interpolated Z
                            {
                                _zBuffer[pixelNr] = invertedInterZ;

                                int pixelStartingByte = pixelNr * Stride;

                                _data[pixelStartingByte] = fragmentColor.B;
                                _data[pixelStartingByte + 1] = fragmentColor.G;
                                _data[pixelStartingByte + 2] = fragmentColor.R;
                                //_data[pixelStartingByte + 3] = 0;   //alpha, we spare the write
                            }
                        }
                    }

                    fromX += toMinDX;
                    toX += toMaxDX;
                }

                //we switch to a top triangle

                highestY = middlePointInt.Y;
                lowestY = lowestPointInt.Y;

                lowestX = Math.Min(middlePoint.X, oppositeMiddleX);
                highestX = Math.Max(middlePoint.X, oppositeMiddleX);

                float fromMinDX = (lowestX - lowestPoint.X) / (middlePoint.Y - lowestPoint.Y);
                float fromMaxDX = (highestX - lowestPoint.X) / (middlePoint.Y - lowestPoint.Y);

                fromX = lowestX;
                toX = highestX;

                for (int y = highestY; y <= lowestY; y++)
                {
                    float yAndHalf = y + 0.5f;

                    if (y == lowestY)
                    {
                        fromX = lowestPoint.X;
                        toX = lowestPoint.X;
                    }

                    for (int x = (int)fromX; x <= (int)toX; x++)
                    {
                        Vector3 p = new Vector3(x + 0.5f, yAndHalf, 0f);

                        Vector3 p_p2 = p - pointsFloat[1];

                        float a = (p1_p2.Y * p3_p2.Z - p1_p2.Z * p3_p2.Y);
                        float b = (p1_p2.Z * p3_p2.X - p1_p2.X * p3_p2.Z);
                        float c = (p1_p2.X * p3_p2.Y - p1_p2.Y * p3_p2.X);
                        //float interpolatedZ = - (a * p_p2.X + b * p_p2.Y) / c + p2.Z;                        
                        //float invertedInterZ = 1 / interpolatedZ;

                        float invertedInterZ = -c / (a * p_p2.X + b * p_p2.Y - c * pointsFloat[1].Z);      //one more mult, but one less division

                        //we calculate the pixel number
                        int pixelNr = y * _width + x;

                        //lock is A LOT faster than .net Spinlock. Same speed as my AsyncStuff.Spinlock
                        //Using this lock loses around 10fps but is necessary to avoid artifacts
                        lock (_pixelLocks[pixelNr])
                        {
                            if (invertedInterZ > _zBuffer[pixelNr])       //we're now using the interpolated Z
                            {
                                _zBuffer[pixelNr] = invertedInterZ;

                                int pixelStartingByte = pixelNr * Stride;

                                _data[pixelStartingByte] = fragmentColor.B;
                                _data[pixelStartingByte + 1] = fragmentColor.G;
                                _data[pixelStartingByte + 2] = fragmentColor.R;
                                //_data[pixelStartingByte + 3] = 0;   //alpha, we spare the write
                            }
                        }
                    }

                    fromX += fromMinDX;
                    toX += fromMaxDX;
                }

            }

            



        }

        private int HighestYPoint(Point[] points)
        {
            int highest = 0;
            
            for(int i = 1; i < 3; i++)  //we start from the second point
            {
                if (points[i].Y < points[highest].Y )
                    highest = i;
            }

            return highest;
        }

        private int LowestYPoint(Point[] points)
        {
            //with lowest we mean by screen reference not absolute number. A point with Y = 5 is "lower" that one with Y = 0
            int lowest = 0;

            for (int i = 1; i < 3; i++)  //we start from the second point
            {
                if (points[i].Y > points[lowest].Y)
                    lowest = i;
            }

            return lowest;
        }

        private int MiddleYPoint(int highest, int lowest)
        {
            int middle = 0; 
            
            for (int i = 0; i < 3; i++)  //we start from the second point
            {
                if (i != highest && i != lowest)
                {
                    middle = i;
                }
            }

            return middle;
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


            return  Vector3.Cross(p2_p3, p_p3).Z <= 0 &&    //early reject using && properties
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
            _data = new byte[_width * _height * Stride];            

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
