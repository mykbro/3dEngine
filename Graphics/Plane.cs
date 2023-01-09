
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace _3dGraphics.Graphics
{
    public struct Plane
    {
        private readonly Vector3 _p0;
        private readonly Vector3 _normal;

        public Vector3 P0 => _p0;
        public Vector3 Normal => _normal;

        public Plane(Vector3 p, Vector3 normal)
        {
            _p0 = p;
            _normal = normal;
        }

        public static float GetDistanceFromPlane(Plane plane, Vector3 p)
        {
            Vector3 temp = plane.P0 - p;

            return Vector3.Dot(temp, plane.Normal);
        }

    }
}
