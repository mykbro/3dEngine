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
        private readonly Vector4[] _vertices;      
        private readonly Triangle[] _triangles;
        private readonly Vector3[] _triangleNormals;
        private readonly AABBox _boundingBox;

        // PROPERTIES

        public IEnumerable<Vector4> Vertices => _vertices;        
        public IEnumerable<Triangle> Triangles => _triangles;
        public IEnumerable<Vector3> Normals => _triangleNormals;

        public int VertexCount => _vertices.Length;
        public int TriangleCount => _triangles.Length; 
        public AABBox AxisAlignedBoundingBox => _boundingBox;

        // CONSTRUCTORS

        public Mesh(IEnumerable<Vector4> vertices, IEnumerable<Triangle> triangles)
        {
            _vertices = vertices.ToArray();     //copy          
            _triangles = triangles.ToArray();   //copy

            _triangleNormals = new Vector3[_triangles.Length];
            GenerateNormals();

            _boundingBox = GenerateAxisAlignedBoundingBox();
        }

        public Mesh(Mesh m)
        {
            _vertices = m._vertices.ToArray();            
            _triangles = m._triangles.ToArray();
            _triangleNormals = m._triangleNormals.ToArray();
        }

        // METHODS

        public Vector4 GetVertex(int index)
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

        private Vector3 CalculateTriangleNormal(int triangleIndex)
        {
            //we follow the clockwise vertices declaration  
            Triangle t = _triangles[triangleIndex];

            Vector3 firstEdge = Vec4ToVec3(_vertices[t.V2Index]) - Vec4ToVec3(_vertices[t.V1Index]);
            Vector3 secondEdge = Vec4ToVec3(_vertices[t.V3Index]) - Vec4ToVec3(_vertices[t.V1Index]);
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

        private AABBox GenerateAxisAlignedBoundingBox()
        {
            float minX, maxX, minY, maxY, minZ, maxZ;
            Vector4 minPoint, maxPoint;

            if(_vertices.Length > 0) 
            {
                minX = _vertices[0].X;
                maxX = _vertices[0].X;                
                minY = _vertices[0].Y;
                maxY = _vertices[0].Y;
                minZ = _vertices[0].Z;
                maxZ = _vertices[0].Z;

                for (int i = 1; i < _vertices.Length; i++)
                {
                    Vector4 temp = _vertices[i];

                    minX = MathF.Min(temp.X, minX);
                    maxX = MathF.Max(temp.X, maxX);
                    minY = MathF.Min(temp.Y, minY);
                    maxY = MathF.Max(temp.Y, maxY);
                    minZ = MathF.Min(temp.Z, minZ);
                    maxZ = MathF.Max(temp.Z, maxZ);
                }

                minPoint = new Vector4(minX, minY, minZ, 1f);
                maxPoint = new Vector4(maxX, maxY, maxZ, 1f);
            }
            else
            {
                minPoint = Vector4.Zero; 
                maxPoint = Vector4.Zero;
            }

            return new AABBox(minPoint, maxPoint);            
        }
        
        private Vector3 Vec4ToVec3(Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}
