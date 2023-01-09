using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace _3dGraphics.Graphics
{
    public class Mesh
    {
        private readonly Vertex[] _vertices;
        private readonly Triangle[] _triangles;
        private readonly Vector3[] _triangleNormals;
        private Vector3 _position;
        private Vector3 _positionLowBits;
        private float _scaleFactor;
        //private Quaternion _orientation;

        public IEnumerable<Vertex> Vertices => _vertices;
        public IEnumerable<Triangle> Triangles => _triangles;
        public IEnumerable<Vector3> Normals => _triangleNormals;

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
        public int VertexCount => _vertices.Length;
        public int TriangleCount => _triangles.Length;

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


        #region CONSTRUCTORS
        public Mesh(IEnumerable<Vertex> vertices, IEnumerable<Triangle> triangles, Vector3 pos, float scale)
        {
            _vertices = vertices.ToArray();     //copy
            _triangles = triangles.ToArray();   //copy

            _triangleNormals = new Vector3[_triangles.Length];
            GenerateNormals();

            _position = pos;
            _positionLowBits = Vector3.Zero;
            _scaleFactor = scale;
        }

        public Mesh(Mesh m)
        {
            _vertices = m._vertices.ToArray();
            _triangles = m._triangles.ToArray();
            _triangleNormals = m._triangleNormals.ToArray();
            _position = m._position;
            _positionLowBits = m._positionLowBits;
            _scaleFactor = m._scaleFactor;            
        }
        #endregion

        private Vector3 CalculateTriangleNormal(int triangleIndex)
        {
            //we follow the clockwise vertices declaration  
            Triangle t = _triangles[triangleIndex];

            Vector3 firstEdge = _vertices[t.V2Index].Position3D - _vertices[t.V1Index].Position3D; 
            Vector3 secondEdge = _vertices[t.V3Index].Position3D - _vertices[t.V1Index].Position3D;
            Vector3 crossP = Vector3.Cross(firstEdge, secondEdge);

            return Vector3.Normalize(crossP);
        }

        private void GenerateNormals()
        {
            for(int i=0; i < _triangles.Length; i++) 
            {
                _triangleNormals[i] = CalculateTriangleNormal(i);
            }
        }

        public void MoveBy(Vector3 deltaPos)
        {
            //we use kahan algorithm
            Vector3 adjustedDeltaPos = deltaPos - _positionLowBits;
            Vector3 newPos = _position + adjustedDeltaPos;
            _positionLowBits = (newPos - _position) - adjustedDeltaPos;
            _position = newPos;
        }
        
        public Vertex GetVertex(int index)
        { 
            return _vertices[index];
        }

        public Triangle GetTriangle(int index)
        {
            return _triangles[index];
        }

        public Vector3 GetNormal(int index)
        {
            return _triangleNormals[index];
        }

    }
}
