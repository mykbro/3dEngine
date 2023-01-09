using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace _3dGraphics.Graphics
{
    public class Camera
    {
        private int _width;
        private int _height;       
        private float _fov;
        private float _zNear;
        private float _zFar;
        private Vector3 _position;
        private Vector3 _positionLowBits;   //used in kahan algorithm for moving by small deltas
        private Vector3 _angles;            //thetaX -> Pitch, thetaY -> Yaw, thetaZ -> Roll
        private Vector3 _upDirection;      

        //PROPERTIES
        public int ScreenWidth { get => _width; set => _width = value; }
        public int ScreenHeight { get => _height; set => _height = value; }
        public float AspectRatio => _width * 1f / _height;
        public float FOV { get => _fov; set => _fov = value; }
        public float ZNear { get => _zNear; set => _zNear = value; }
        public float ZFar { get => _zFar; set => _zFar = value; }


        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                _positionLowBits = Vector3.Zero;    //we need to reset this accumulator when moving to a specific position
            }
        }        

        public Matrix4x4 WorldToCameraMatrix
        {
            get
            {
                return Matrix4x4.CreateTranslation(-_position.X, -_position.Y, -_position.Z);  
            }
        }

        public Matrix4x4 ProjectionMatrix
        {
            get
            {
                float d = 1 / MathF.Atan(Utility.DegToRad(_fov / 2));
                float deltaZ = _zFar - _zNear;

                Matrix4x4 temp = new Matrix4x4();   //Zero Matrix
                temp.M11 = d / AspectRatio;
                temp.M22 = d;
                temp.M33 = _zFar / deltaZ;
                temp.M34 = 1.0f;
                temp.M43 = - ZFar * ZNear / deltaZ;                              

                return temp;                
            }
        }

        public Matrix4x4 ViewPortTransformMatrix
        {
            get
            {
                float halfWidth = _width/ 2;
                float halfHeight = _height / 2;
                Matrix4x4 temp = Matrix4x4.Identity;   
                temp.M11 = halfWidth;
                temp.M41 = halfWidth;
                temp.M22 = -halfHeight;
                temp.M42 = halfHeight;

                return temp;
            }
        }

        //CONSTRUCTORS
        public Camera(int w, int h, float fov, float zNear, float zFar, Vector3 pos, Vector3 forwardDir, Vector3 upDir) 
        {
            _width = w;
            _height = h;
            _fov = fov;
            _zNear = zNear;
            _zFar = zFar;
            _position = pos;
            _positionLowBits = Vector3.Zero;
            _forwardDirection = forwardDir;
            _upDirection = upDir;           
        }

        public Camera(int w, int h, float fov, float zNear, float zFar) : this(w, h, fov, zNear, zFar, Vector3.Zero, Vector3.UnitZ, Vector3.UnitY) { }

        //COPY CONSTRUCTOR
        public Camera(Camera c) 
        {
            _width = c._width;
            _height = c._height;
            _fov = c._fov;
            _zNear = c._zNear;
            _zFar = c._zFar;
            _position = c._position;
            _positionLowBits = c._positionLowBits;
            _forwardDirection = c._forwardDirection;
            _upDirection = c._upDirection;            
        }

        public void MoveBy(Vector3 deltaPos)
        {
            //we use kahan algorithm
            Vector3 adjustedDeltaPos = deltaPos - _positionLowBits;
            Vector3 newPos = _position + adjustedDeltaPos;
            _positionLowBits = (newPos - _position) - adjustedDeltaPos;
            _position = newPos;
        }

    }
}
