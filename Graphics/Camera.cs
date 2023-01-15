using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Windows.Navigation;

namespace _3dGraphics.Graphics
{
    public class Camera
    {
        private int _width;
        private int _height;
        private float _fov;
        private float _zNear;
        private float _zFar;
        private DoubleVector3 _position;       
        private DoubleVector3 _orientation;           //thetaX -> Pitch, thetaY -> Yaw, thetaZ -> Roll        

        //PROPERTIES
        public int ScreenWidth { get => _width; set => _width = value; }
        public int ScreenHeight { get => _height; set => _height = value; }
        public float AspectRatio => (_width * 1f) / _height;
        public float FOV { get => _fov; set => _fov = value; }
        public float ZNear { get => _zNear; set => _zNear = value; }
        public float ZFar { get => _zFar; set => _zFar = value; }
        public Vector3 Orientation { get => _orientation.ToVector3(); set => _orientation = new DoubleVector3(Utility.NormalizeAngles(value)); }
        public Vector3 Position { get => _position.ToVector3(); set => _position = new DoubleVector3(value); }           
                

        public Matrix4x4 WorldToCameraMatrix
        {
            get
            {
                Vector3 pos = Position;
                Matrix4x4 invTanslMatrix = Matrix4x4.CreateTranslation(-pos.X, -pos.Y, -pos.Z);
                Matrix4x4 invPitchMatrix = Matrix4x4.CreateRotationX( -Orientation.X);
                Matrix4x4 invYawMatrix = Matrix4x4.CreateRotationY( -Orientation.Y);
                Matrix4x4 invRollMatrix = Matrix4x4.CreateRotationZ( -Orientation.Z);

                return invTanslMatrix * invRollMatrix * invYawMatrix * invPitchMatrix;   
            }
        }

        public Matrix4x4 RotationXMatrix => Matrix4x4.CreateRotationX(Orientation.X);
        public Matrix4x4 RotationYMatrix => Matrix4x4.CreateRotationY(Orientation.Y);
        public Matrix4x4 RotationZMatrix => Matrix4x4.CreateRotationZ(Orientation.Z);

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
                temp.M43 = -_zFar * _zNear / deltaZ;                              

                return temp;                
            }
        }

        public Matrix4x4 ViewPortTransformMatrix
        {
            get
            {
                float halfWidth = (_width - 1) / 2;
                float halfHeight = (_height - 1) / 2;
                Matrix4x4 temp = Matrix4x4.Identity;   
                temp.M11 = halfWidth;
                temp.M41 = halfWidth;
                temp.M22 = -halfHeight;
                temp.M42 = halfHeight;

                return temp;
            }
        }

        //CONSTRUCTORS
        public Camera(int w, int h, float fov, float zNear, float zFar, Vector3 pos, Vector3 orientation) 
        {
            _width = w;
            _height = h;
            _fov = fov;
            _zNear = zNear;
            _zFar = zFar;
            _position = new DoubleVector3(pos);
            _orientation = new DoubleVector3(orientation);
        }

        public Camera(int w, int h, float fov, float zNear, float zFar) : this(w, h, fov, zNear, zFar, Vector3.Zero, Vector3.Zero) { }

        //COPY CONSTRUCTOR
        public Camera(Camera c) 
        {
            _width = c._width;
            _height = c._height;
            _fov = c._fov;
            _zNear = c._zNear;
            _zFar = c._zFar;
            _position = c._position;            
            _orientation = c._orientation;
        }

        public void MoveBy(Vector3 deltaPos)
        {
            _position += new DoubleVector3(deltaPos);
        }

        public void RotateBy(Vector3 deltaTheta)
        {
            _orientation = Utility.NormalizeAngle(_orientation + new DoubleVector3(deltaTheta));
            
        }

    }
}
