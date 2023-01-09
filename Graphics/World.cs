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
        private float _cameraSpeedKmh;

        //public IEnumerable<Mesh> Meshes => _meshes.AsEnumerable();
        //public Camera Camera => new Camera(_camera);
        public List<Mesh> Meshes => _meshes;
        public Camera Camera => _camera;
        public float CameraSpeedKmh { get => _cameraSpeedKmh; set => _cameraSpeedKmh = value; }

        public World(int screenWidth, int screenHeight, float cameraFov, float cameraZNear, float cameraZFar, float cameraSpeedKmh)
        {
            _meshes = new List<Mesh>();
            _camera = new Camera(screenWidth, screenHeight, cameraFov, cameraZNear, cameraZFar);
            _cameraSpeedKmh = cameraSpeedKmh;
        }
        
        //Copy Constructor
        public World(World w)
        {                
            _meshes = new List<Mesh>(w._meshes);
            _camera = new Camera(w._camera);
            _cameraSpeedKmh = w._cameraSpeedKmh;
        }
        
        public void Update(float dTimeInSecs, Vector3 cameraVelocityNormalized)
        {                        
            Vector3 deltaP = cameraVelocityNormalized * (dTimeInSecs * _cameraSpeedKmh / 3.6f);
            _camera.MoveBy(deltaP);
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
