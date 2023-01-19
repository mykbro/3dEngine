using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace _3dGraphics.Graphics
{
    public readonly struct AABBox
    {
        //Implementing an Axis-Aligned BoundingBox (AABB) only minXYZ and maxXYZ are needed
        private readonly Vector4 _minPoint;
        private readonly Vector4 _maxPoint;

        public float MinX => _minPoint.X;
        public float MaxX => _maxPoint.X;
        public float MinY => _minPoint.Y;
        public float MaxY => _maxPoint.Y;
        public float MinZ => _minPoint.Z;
        public float MaxZ => _maxPoint.Z;

        public Vector4 MinPoint => _minPoint;
        public Vector4 MaxPoint => _maxPoint;

        public Vector4[] Points
        {
            get
            {
                return new Vector4[]{   
                                        _minPoint, new Vector4(MinX, MinY, MaxZ, 1), new Vector4(MinX, MaxY, MinZ, 1), new Vector4(MinX, MaxY, MaxZ, 1),
                                        _maxPoint, new Vector4(MaxX, MaxY, MinZ, 1), new Vector4(MaxX, MinY, MaxZ, 1), new Vector4(MaxX, MinY, MinZ, 1)
                                    };
            }
        }

        public AABBox(Vector4 minPoint, Vector4 maxPoint)
        {
            _minPoint = minPoint;
            _maxPoint = maxPoint;
        }
        
        public AABBox(float minX, float maxX, float minY, float maxY, float minZ, float maxZ) : this(new Vector4(minX, minY, minZ, 1) , new Vector4(maxX, maxY, maxZ, 1))
        {            
        }       

    }
}
