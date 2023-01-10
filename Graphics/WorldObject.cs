using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace _3dGraphics.Graphics
{
    public class WorldObject
    {
        private Mesh _objectMeshRef;   //we do not make it readonly... we may wish to change it at runtime
        private Vector3 _position;
        private Vector3 _positionLowBits;
        private float _scaleFactor;
        //private Quaternion _orientation;        

        // PROPERTIES

        public Mesh Mesh { get => _objectMeshRef; set => _objectMeshRef = value; }

        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                _positionLowBits = Vector3.Zero;
            }
        }

        public float ScaleFactor { get => _scaleFactor; set => _scaleFactor = value; }               

        public Matrix4x4 LocalToWorldMatrix
        {
            get
            {
                Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(_scaleFactor);
                Matrix4x4 translMatrix = Matrix4x4.CreateTranslation(_position.X, _position.Y, _position.Z);

                //return translMatrix * scaleMatrix;  //LOCAL -> SCALE -> TRANSL -> WORLD
                return scaleMatrix * translMatrix;  //LOCAL -> SCALE -> TRANSL -> WORLD
            }
        }

        public Matrix4x4 WorldToLocalMatrix
        {
            get
            {
                Matrix4x4 unscaleMatrix = Matrix4x4.CreateScale(1.0f/_scaleFactor);
                Matrix4x4 untranslMatrix = Matrix4x4.CreateTranslation(- _position.X, - _position.Y, - _position.Z);

                //return unscaleMatrix * untranslMatrix;  //WORLD -> UNTRANSL -> UNSCALE -> LOCAL
                return untranslMatrix * unscaleMatrix;  //WORLD -> UNTRANSL -> UNSCALE -> LOCAL
            }
        }

        // CONSTRUCTORS

        public WorldObject(Mesh mRef, Vector3 pos, float scale)
        {
            _objectMeshRef = mRef;  //we copy the reference
            _position = pos;
            _positionLowBits = Vector3.Zero;
            _scaleFactor = scale;
        }

        public WorldObject(WorldObject wObj)
        {
            _objectMeshRef = wObj._objectMeshRef;   //we copy the reference
            _position = wObj._position;
            _positionLowBits = wObj._positionLowBits;
            _scaleFactor = wObj._scaleFactor;
        }
        
        // METHODS

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
