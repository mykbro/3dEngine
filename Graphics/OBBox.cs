using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _3dGraphics.Graphics
{
    public readonly struct OBBox
    {
        private readonly Vector4 _origin;
        private readonly Vector4 _i;
        private readonly Vector4 _j;
        private readonly Vector4 _k;

        public Vector4 Origin => _origin;
        public Vector4 I => _i;
        public Vector4 J => _j;
        public Vector4 K => _k;

        public Vector4[] Points
        {
            get
            {                
                Vector4 dJ = _j - _origin;
                Vector4 dK = _k - _origin;
                Vector4 dJdK = dJ + dK; 

                return new Vector4[]{ _origin, _i, _j, _k, _i + dJ, _i + dK, _origin + dJdK, _i + dJdK };
            }
        }

        public OBBox(Vector4 origin, Vector4 i, Vector4 j, Vector4 k)
        {
            _origin = origin;
            _i = i;
            _j = j;
            _k = k;
        }

        public OBBox(AABBox aabbox)
        {
            _origin = aabbox.MinPoint;
            _i = new Vector4(aabbox.MaxX, aabbox.MinY, aabbox.MinZ, 1f);
            _j = new Vector4(aabbox.MinX, aabbox.MaxY, aabbox.MinZ, 1f);
            _k = new Vector4(aabbox.MinX, aabbox.MinY, aabbox.MaxZ, 1f);
        }

        public AABBox GetSurroundingAxisAlignedBoundingBox()
        {
            Vector4[] points = this.Points;
            float minX, maxX, minY, maxY, minZ, maxZ;      
         
            minX = points[0].X;
            maxX = points[0].X;
            minY = points[0].Y;
            maxY = points[0].Y;
            minZ = points[0].Z;
            maxZ = points[0].Z;

            for (int i = 1; i < points.Length; i++)
            {
                Vector4 temp = points[i];

                minX = MathF.Min(temp.X, minX);
                maxX = MathF.Max(temp.X, maxX);
                minY = MathF.Min(temp.Y, minY);
                maxY = MathF.Max(temp.Y, maxY);
                minZ = MathF.Min(temp.Z, minZ);
                maxZ = MathF.Max(temp.Z, maxZ);
            }

            return new AABBox(minX, maxX, minY, maxY, minZ, maxZ);
        }

        public static OBBox TranformOBBox(Matrix4x4 matrix, OBBox box)
        {
            return new OBBox(Vector4.Transform(box._origin, matrix),
                                Vector4.Transform(box._i, matrix),
                                Vector4.Transform(box._j, matrix),
                                Vector4.Transform(box._k, matrix));
        }
    }
}
