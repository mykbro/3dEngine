using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using _3dGraphics.Graphics;

namespace _3dGraphics.Tests
{
    internal static class CameraTest
    {
        public static void Run()
        {
            float p = 4000.0f;
            float delta = 27.12f;
            int numCycles = 100000;

            Vector3 pos = Vector3.One * p;
            Camera c = new Camera(16, 9, 45f, 0.5f, 50f);
            float deltaForEachCycle = delta / numCycles;
            Vector3 deltaVec = Vector3.One * deltaForEachCycle;

            for (int i = 0; i < numCycles; i++)
            {
                c.MoveBy(deltaVec);
            }

            Vector3 expected = pos + deltaVec * numCycles;
        }
    }
}
