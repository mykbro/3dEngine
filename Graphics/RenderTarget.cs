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
            byte redLvl = fragmentColor.R;
            byte greenLvl = fragmentColor.G;
            byte blueLvl = fragmentColor.B;

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

                            _data[pixelStartingByte] = blueLvl;
                            _data[pixelStartingByte + 1] = greenLvl;
                            _data[pixelStartingByte + 2] = redLvl;
                            //_data[pixelStartingByte + 3] = 0;   //alpha, we spare the write
                        }
                    }
                }                                    
            }
        }
       
        public void RenderFragment2(Fragment3D fragment)
        {   
            Vector3[] points = new Vector3[] { fragment.P1, fragment.P2, fragment.P3 };
            
            int highestPointIndex = HighestYPoint(points);
            int lowestPointIndex = LowestYPoint(points);
            int middlePointIndex = MiddleYPoint(highestPointIndex, lowestPointIndex);

            Vector3 highestPoint = points[highestPointIndex];
            Vector3 lowestPoint = points[lowestPointIndex];
            Vector3 middlePoint= points[middlePointIndex];            
            
            
            if((int) highestPoint.Y == (int) middlePoint.Y)      //top triangle, includes the line case
            {
                RenderTopTriangle(highestPoint, middlePoint, lowestPoint, points[0], points[1], points[2], fragment.Color);
            }
            else if((int) middlePoint.Y == (int) lowestPoint.Y)     //bottom triangle
            {
                RenderBottomTriangle(highestPoint, middlePoint, lowestPoint, points[0], points[1], points[2], fragment.Color);
            }
            else                    //general case
            {
                
                //we interpolate X and Z for the point opposite to middlePoint (the Y is the same)
                float highestToLowestDX_DY = (highestPoint.X - lowestPoint.X) / (highestPoint.Y - lowestPoint.Y);
                float highestToLowestDZ_DY = (highestPoint.Z - lowestPoint.Z) / (highestPoint.Y - lowestPoint.Y);
                float oppositeToMidX = highestPoint.X + highestToLowestDX_DY * (middlePoint.Y - highestPoint.Y);
                float oppositeToMidZ = highestPoint.Z + highestToLowestDZ_DY * (middlePoint.Y - highestPoint.Y);

                Vector3 oppositeMiddlePoint = new Vector3(oppositeToMidX, middlePoint.Y, oppositeToMidZ);
                //we need to determine where to "place" the oppositeMiddlePoint keeping the clockwise point order:
                //first we need to know if the triangle is right or left faced: 1 is right faced, 2 is left faced
                int highestToLowIndexDiff = highestPointIndex - lowestPointIndex;
                

                if(highestToLowIndexDiff == 1 || highestToLowIndexDiff == -2)  //right-faced
                {
                    RenderBottomTriangle(highestPoint, middlePoint, oppositeMiddlePoint, highestPoint, middlePoint, oppositeMiddlePoint, fragment.Color);
                    RenderTopTriangle(middlePoint, oppositeMiddlePoint, lowestPoint, middlePoint, lowestPoint, oppositeMiddlePoint, fragment.Color);
                }
                else   //highestToMidIndexDiff == 2 || -1, can't be anything else, left-faced
                {
                    RenderBottomTriangle(highestPoint, middlePoint, oppositeMiddlePoint, highestPoint, oppositeMiddlePoint, middlePoint, fragment.Color);
                    RenderTopTriangle(middlePoint, oppositeMiddlePoint, lowestPoint, oppositeMiddlePoint, lowestPoint, middlePoint, fragment.Color);
                }
                
            }
        }

        private void RenderTopTriangle(Vector3 highestPoint, Vector3 middlePoint, Vector3 lowestPoint, Vector3 p1, Vector3 p2, Vector3 p3, Color triangleColor) 
        {
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

            //
            float minY = middlePoint.Y;
            float maxY = lowestPoint.Y;
            float dY = maxY - minY;

            float topLeftX = Math.Min(highestPoint.X, middlePoint.X);
            float topRightX = Math.Max(highestPoint.X, middlePoint.X);
            float bottomX = lowestPoint.X;

            float dXFromLeftToBottom = (bottomX - topLeftX) / dY;
            float dXFromRightToBottom = (bottomX - topRightX) / dY;

            int minYInt = (int)minY;
            int maxYInt = (int)maxY;
           
            bool isLine = (minYInt == maxYInt);

            for (int y = minYInt, i = 0; y <= maxYInt; y++, i++)
            {
                float yAndHalf = y + 0.5f;  //cache result

                int lineMinX;
                int lineMaxX;

                if (y != maxYInt)
                {
                    lineMinX = (int) (topLeftX + i * dXFromLeftToBottom) - 1;
                    lineMaxX = (int) (topRightX + i * dXFromRightToBottom) + 1;
                }
                else if (isLine)
                {
                    lineMinX = (int)topLeftX;
                    lineMaxX = (int)topRightX;
                }
                else //for the last line if we're not a single line triangle
                {
                    lineMinX = (int) bottomX;
                    lineMaxX = (int) bottomX;
                }

                //we search for the starting point
                bool startingXFound = false;
                bool endingXFound = false;
                int startingX = lineMaxX + 1;   //initialize over Max so that if we do not found any X startingX will be > than endingX and we won't draw anything
                int endingX = lineMinX - 1;     //see above

                Vector3 p = new Vector3(0f, yAndHalf, 0f);

                for (int x = lineMinX; x <= lineMaxX && !startingXFound; x++)
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
                        startingX = x;
                        startingXFound = true;
                    }
                }

                //we search the ending point
                if (startingXFound)      //we skip the search for endingX if we already scanned the line and did not find any startingX
                {
                    for (int x = lineMaxX; x >= lineMinX && !endingXFound; x--)     //decreasing loop
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
                    //Vector3 p = new Vector3(x + 0.5f, yAndHalf, 0f);
                    p.X = x + 0.5f;  
              
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

                            _data[pixelStartingByte] = triangleColor.B;
                            _data[pixelStartingByte + 1] = triangleColor.G;
                            _data[pixelStartingByte + 2] = triangleColor.R;
                            //_data[pixelStartingByte + 3] = 0;   //alpha, we spare the write
                        }
                    }
                }               
            }
        }

        private void RenderBottomTriangle(Vector3 highestPoint, Vector3 middlePoint, Vector3 lowestPoint, Vector3 p1, Vector3 p2, Vector3 p3, Color triangleColor)
        {
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

            //
            float minY = highestPoint.Y;
            float maxY = middlePoint.Y;
            float dY = maxY - minY;

            float bottomLeftX = Math.Min(lowestPoint.X, middlePoint.X);
            float bottomRightX = Math.Max(lowestPoint.X, middlePoint.X);
            float topX = (int) highestPoint.X;

            float dXFromTopToBottomLeft = (bottomLeftX - topX) / dY;
            float dXFromTopToBottomRight = (bottomRightX - topX) / dY;

            int minYInt = (int)minY;
            int maxYInt = (int)maxY;


            for (int y = minYInt, i = 0; y <= maxYInt; y++, i++)
            {
                float yAndHalf = y + 0.5f;  //cache result

                int lineMinX;
                int lineMaxX;

                if (y != maxYInt)
                {
                    lineMinX = (int)(topX + i * dXFromTopToBottomLeft) - 1;
                    lineMaxX = (int)(topX + i * dXFromTopToBottomRight) + 1;
                }
                else
                {
                    lineMinX = (int)bottomLeftX;
                    lineMaxX = (int)bottomRightX;
                }

                //we search for the starting point
                bool startingXFound = false;
                bool endingXFound = false;
                int startingX = lineMaxX + 1;   //initialize over Max so that if we do not found any X startingX will be > than endingX and we won't draw anything
                int endingX = lineMinX - 1;     //see above

                Vector3 p = new Vector3(0f, yAndHalf, 0f);

                for (int x = lineMinX; x <= lineMaxX && !startingXFound; x++)
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
                        startingX = x;
                        startingXFound = true;
                    }
                }

                //we search the ending point
                if (startingXFound)      //we skip the search for endingX if we already scanned the line and did not find any startingX
                {
                    for (int x = lineMaxX; x >= lineMinX && !endingXFound; x--)     //decreasing loop
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
                    //Vector3 p = new Vector3(x + 0.5f, yAndHalf, 0f);
                    p.X = x + 0.5f;

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

                            _data[pixelStartingByte] = triangleColor.B;
                            _data[pixelStartingByte + 1] = triangleColor.G;
                            _data[pixelStartingByte + 2] = triangleColor.R;
                            //_data[pixelStartingByte + 3] = 0;   //alpha, we spare the write
                        }
                    }
                }
            }
        }

        private int HighestYPoint(Vector3[] points)
        {
            int highest = 0;
            
            for(int i = 1; i < 3; i++)  //we start from the second point
            {
                if ((int) points[i].Y < (int) points[highest].Y )
                    highest = i;
            }

            return highest;
        }

        private int LowestYPoint(Vector3[] points)
        {
            //with lowest we mean by screen reference not absolute number. A point with Y = 5 is "lower" that one with Y = 0
            int lowest = 2;

            for (int i = 1; i >= 0; i--)  //we start from the second point
            {
                if ((int)points[i].Y > (int) points[lowest].Y)
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
