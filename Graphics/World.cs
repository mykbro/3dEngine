using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace _3dGraphics.Graphics
{
    internal class World
    {
        private readonly List<Mesh> _meshes;
        private readonly Camera _camera;
        private float _cameraSpeedMetersSec;
        private float _cameraRotSpeedRadSec;
        private float _fovIncreaseSpeedDegSec = 10f;

        //public IEnumerable<Mesh> Meshes => _meshes.AsEnumerable();
        //public Camera Camera => new Camera(_camera);
        public List<Mesh> Meshes => _meshes;
        public Camera Camera => _camera;
        public float CameraSpeedKmh { get => _cameraSpeedMetersSec * 3.6f; set => _cameraSpeedMetersSec = value / 3.6f; }
        public float CameraRotSpeedDegSec { get => Utility.RadToDeg(_cameraRotSpeedRadSec); set => Utility.DegToRad(value); }

        public int TotalVertexCount
        {
            get
            {
                return _meshes.Select<Mesh, int>((m) => m.VertexCount).Sum();
            }
        }

        public int TotalTriangleCount
        {
            get
            {
                return _meshes.Select<Mesh, int>((m) => m.TriangleCount).Sum();
            }
        }

        public World(int screenWidth, int screenHeight, float cameraFov, float cameraZNear, float cameraZFar, float cameraSpeedKmh, float cameraRotSpeedDegSec, float fovIncSpeedDegSec)
        {
            _meshes = new List<Mesh>();
            _camera = new Camera(screenWidth, screenHeight, cameraFov, cameraZNear, cameraZFar);
            _cameraSpeedMetersSec = cameraSpeedKmh / 3.6f;
            _cameraRotSpeedRadSec = Utility.DegToRad(cameraRotSpeedDegSec);
            _fovIncreaseSpeedDegSec = fovIncSpeedDegSec;
        }
        
        //Copy Constructor
        public World(World w)
        {                
            _meshes = new List<Mesh>(w._meshes);
            _camera = new Camera(w._camera);
            _cameraSpeedMetersSec = w._cameraSpeedMetersSec;
            _cameraRotSpeedRadSec = w._cameraRotSpeedRadSec;
            _fovIncreaseSpeedDegSec = w._fovIncreaseSpeedDegSec;
        }
        
        public void Update(float dTimeInSecs, Vector3 cameraVelocityNormalized, Vector3 cameraRotation, float fovIncrease)
        {                        
            Vector3 deltaP = cameraVelocityNormalized * (dTimeInSecs * _cameraSpeedMetersSec);
            Matrix4x4 XYRotationMatrix = _camera.RotationXMatrix * _camera.RotationYMatrix * _camera.RotationZMatrix;
            deltaP = Vector3.Transform(deltaP, XYRotationMatrix);
            _camera.MoveBy(deltaP);

            Vector3 deltaTheta = cameraRotation * (dTimeInSecs * _cameraRotSpeedRadSec); 
            _camera.RotateBy(deltaTheta);

            float deltaFov = fovIncrease * dTimeInSecs * _fovIncreaseSpeedDegSec;
            _camera.FOV += deltaFov;
        }

        /*
        public void AddMesh(Mesh m) 
        { 
            _meshes.Add(m);
        }

        public void RemoveMesh(int i)
        {
            _meshes.RemoveAt(i);
        }

        
        public Mesh GetMesh(int i)
        {
            return _meshes[i];
        }      
        */

        /*
        public void SetMeshPosition(int meshIndex, Vector3 position)
        {
            _meshes[meshIndex].Position = position;
        }

        public void MoveMeshBy(int meshIndex, Vector3 deltaP)
        {
            _meshes[meshIndex].MoveBy(deltaP);
        }

        public void SetMeshScale(int meshIndex, float scale)
        {
            _meshes[meshIndex].ScaleFactor = scale;
        }
        */
    }
}
