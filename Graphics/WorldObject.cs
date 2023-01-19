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
        private DoubleVector3 _position;        
        private float _scaleFactor;
        //private Quaternion _orientation;        

        // PROPERTIES

        public Mesh Mesh { get => _objectMeshRef; set => _objectMeshRef = value; }        

        public Vector3 Position
        {
            get => _position.ToVector3();
            set
            {
                _position = new DoubleVector3(value);               
            }
        }

        public float ScaleFactor { get => _scaleFactor; set => _scaleFactor = value; }               

        public Matrix4x4 LocalToWorldMatrix
        {
            get
            {
                Vector3 pos = Position;
                Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(_scaleFactor);
                Matrix4x4 translMatrix = Matrix4x4.CreateTranslation(pos.X, pos.Y, pos.Z);

                //return translMatrix * scaleMatrix;  //LOCAL -> SCALE -> TRANSL -> WORLD
                return scaleMatrix * translMatrix;  //LOCAL -> SCALE -> TRANSL -> WORLD
            }
        }

        public Matrix4x4 WorldToLocalMatrix
        {
            get
            {
                Vector3 pos = Position;
                Matrix4x4 unscaleMatrix = Matrix4x4.CreateScale(1.0f/_scaleFactor);
                Matrix4x4 untranslMatrix = Matrix4x4.CreateTranslation(-pos.X, -pos.Y, -pos.Z);

                //return unscaleMatrix * untranslMatrix;  //WORLD -> UNTRANSL -> UNSCALE -> LOCAL
                return untranslMatrix * unscaleMatrix;  //WORLD -> UNTRANSL -> UNSCALE -> LOCAL
            }
        }

        // CONSTRUCTORS

        public WorldObject(Mesh mRef, Vector3 pos, float scale)
        {
            _objectMeshRef = mRef;  //we copy the reference
            _position = new DoubleVector3(pos);          
            _scaleFactor = scale;
        }

        public WorldObject(WorldObject wObj)
        {
            _objectMeshRef = wObj._objectMeshRef;   //we copy the reference
            _position = wObj._position;           
            _scaleFactor = wObj._scaleFactor;
        }
        
        // METHODS

        public void MoveBy(Vector3 deltaPos)
        {
            _position += new DoubleVector3(deltaPos);
        } 
    }
}
