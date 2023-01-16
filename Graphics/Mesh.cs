using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _3dGraphics.Graphics
{
    public class Mesh
    {
        private readonly Vector3[] _vertices;
        private readonly Vector2[] _texels;
        private readonly Triangle[] _triangles;
        private readonly Vector3[] _triangleNormals;

        // PROPERTIES

        public IEnumerable<Vector3> Vertices => _vertices;
        public IEnumerable<Vector2> TextureCoords => _texels;
        public IEnumerable<Triangle> Triangles => _triangles;
        public IEnumerable<Vector3> Normals => _triangleNormals;

        public int VertexCount => _vertices.Length;
        public int TriangleCount => _triangles.Length;
        public int TexelCount => _texels.Length;

        // CONSTRUCTORS

        public Mesh(IEnumerable<Vector3> vertices, IEnumerable<Vector2> textureCoords, IEnumerable<Triangle> triangles)
        {
            _vertices = vertices.ToArray();     //copy
            _texels = textureCoords.ToArray();   //copy
            _triangles = triangles.ToArray();   //copy

            _triangleNormals = new Vector3[_triangles.Length];
            GenerateNormals();
        }

        public Mesh(Mesh m)
        {
            _vertices = m._vertices.ToArray();
            _texels = m._texels.ToArray();
            _triangles = m._triangles.ToArray();
            _triangleNormals = m._triangleNormals.ToArray();
        }

        // METHODS

        public Vector3 GetVertex(int index)
        {
            return _vertices[index];
        }

        public Vector2 GetTextureCoord(int index)
        {
            return _texels[index];
        }
        
        public Triangle GetTriangle(int index)
        {
            return _triangles[index];
        }


        public Vector3 GetNormal(int index)
        {
            return _triangleNormals[index];
        }

        private Vector3 CalculateTriangleNormal(int triangleIndex)
        {
            //we follow the clockwise vertices declaration  
            Triangle t = _triangles[triangleIndex];

            Vector3 firstEdge = _vertices[t.V2Index] - _vertices[t.V1Index];
            Vector3 secondEdge = _vertices[t.V3Index] - _vertices[t.V1Index];
            Vector3 crossP = Vector3.Cross(firstEdge, secondEdge);

            return Vector3.Normalize(crossP);
        }

        private void GenerateNormals()
        {
            for (int i = 0; i < _triangles.Length; i++)
            {
                _triangleNormals[i] = CalculateTriangleNormal(i);
            }
        }        
    }
}
