﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace _3dGraphics.Graphics
{
    public static class Utility
    {
        public static float DegToRad(float degrees)
        {
            return (MathF.PI / 180) * degrees;
        }

        public static float RadToDeg(float radians)
        {
            return (radians / 180) * MathF.PI;
        }

        public static Vector3 NormalizeAngles(Vector3 vector) 
        { 
            return new Vector3(NormalizeAngle(vector.X), NormalizeAngle(vector.Y), NormalizeAngle(vector.Z));
        }

        public static DoubleVector3 NormalizeAngle(DoubleVector3 vector)
        {
            return new DoubleVector3(NormalizeAngle(vector.X), NormalizeAngle(vector.Y), NormalizeAngle(vector.Z));
        }

        public static float NormalizeAngle(float angle)
        {
            float TwoPI = 2f * MathF.PI;
            return (angle + TwoPI) % TwoPI;
        }

        public static double NormalizeAngle(double angle)
        {
            double TwoPI = 2 * Math.PI;
            return (angle + TwoPI) % TwoPI;
        }

        //extension method for Vector4
        public static Vector3 ToVector3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

    }
}
