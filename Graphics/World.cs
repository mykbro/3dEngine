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

        //public IEnumerable<Mesh> Meshes => _meshes.AsEnumerable();
        //public Camera Camera => new Camera(_camera);
        public List<Mesh> Meshes => _meshes;
        public Camera Camera => _camera;
        public float CameraSpeedKmh { get => _cameraSpeedMetersSec * 3.6f; set => _cameraSpeedMetersSec = value / 3.6f; }
        public float CameraRotSpeedDegSec { get => Utility.RadToDeg(_cameraRotSpeedRadSec); set => Utility.DegToRad(value); }

        public World(int screenWidth, int screenHeight, float cameraFov, float cameraZNear, float cameraZFar, float cameraSpeedKmh, float cameraRotSpeedDegSec)
        {
            _meshes = new List<Mesh>();
            _camera = new Camera(screenWidth, screenHeight, cameraFov, cameraZNear, cameraZFar);
            _cameraSpeedMetersSec = cameraSpeedKmh / 3.6f;
            _cameraRotSpeedRadSec = Utility.DegToRad(cameraRotSpeedDegSec);
        }
        
        //Copy Constructor
        public World(World w)
        {                
            _meshes = new List<Mesh>(w._meshes);
            _camera = new Camera(w._camera);
            _cameraSpeedMetersSec = w._cameraSpeedMetersSec;
            _cameraRotSpeedRadSec = w._cameraRotSpeedRadSec;
        }
        
        public void Update(float dTimeInSecs, Vector3 cameraVelocityNormalized, Vector3 cameraRotation)
        {                        
            Vector3 deltaP = cameraVelocityNormalized * (dTimeInSecs * _cameraSpeedMetersSec);
            Matrix4x4 XYRotationMatrix = _camera.InverseRotationXMatrix * _camera.InverseRotationYMatrix * _camera.InverseRotationZMatrix;
            deltaP = Vector3.Transform(deltaP, XYRotationMatrix);
            _camera.MoveBy(deltaP);

            Vector3 deltaTheta = cameraRotation * (dTimeInSecs * _cameraRotSpeedRadSec); 
            _camera.RotateBy(deltaTheta);
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
