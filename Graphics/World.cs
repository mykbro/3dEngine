﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace _3dGraphics.Graphics
{
    internal class World
    {
        private readonly List<WorldObject> _worldObjects;
        private readonly Camera _camera;
        private float _cameraSpeedMetersSec;
        private float _cameraRotSpeedRadSec;
        private float _fovIncreaseSpeedDegSec = 10f;

        //public IEnumerable<Mesh> Meshes => _meshes.AsEnumerable();
        //public Camera Camera => new Camera(_camera);
        public List<WorldObject> Objects => _worldObjects;
        public Camera Camera => _camera;
        public float CameraSpeedKmh { get => _cameraSpeedMetersSec * 3.6f; set => _cameraSpeedMetersSec = value / 3.6f; }
        public float CameraRotSpeedDegSec { get => Utility.RadToDeg(_cameraRotSpeedRadSec); set => Utility.DegToRad(value); }
        

        public World(int screenWidth, int screenHeight, float cameraFov, float cameraZNear, float cameraZFar, float cameraSpeedKmh, float cameraRotSpeedDegSec, float fovIncSpeedDegSec)
        {
            _worldObjects = new List<WorldObject>();
            _camera = new Camera(screenWidth, screenHeight, cameraFov, cameraZNear, cameraZFar);
            _cameraSpeedMetersSec = cameraSpeedKmh / 3.6f;
            _cameraRotSpeedRadSec = Utility.DegToRad(cameraRotSpeedDegSec);
            _fovIncreaseSpeedDegSec = fovIncSpeedDegSec;
        }
        
        //Copy Constructor
        public World(World w)
        {                
            _worldObjects = new List<WorldObject>(w._worldObjects);
            _camera = new Camera(w._camera);
            _cameraSpeedMetersSec = w._cameraSpeedMetersSec;
            _cameraRotSpeedRadSec = w._cameraRotSpeedRadSec;
            _fovIncreaseSpeedDegSec = w._fovIncreaseSpeedDegSec;
        }
        
        public void Update(float dTimeInSecs, Vector3 cameraVelocityNormalized, Vector3 cameraRotation, float fovIncrease)
        {                        
            Vector3 deltaP = cameraVelocityNormalized * (dTimeInSecs * _cameraSpeedMetersSec);
            //Matrix4x4 XYRotationMatrix = _camera.RotationXMatrix * _camera.RotationYMatrix * _camera.RotationZMatrix;
            Matrix4x4 XYRotationMatrix = _camera.RotationYMatrix;   //using only the Y rotation replicates the "normal" leveled movement with freelook
            deltaP = Vector3.Transform(deltaP, XYRotationMatrix);
            _camera.MoveBy(deltaP);

            Vector3 deltaTheta = cameraRotation * (dTimeInSecs * _cameraRotSpeedRadSec); 
            _camera.RotateBy(deltaTheta);

            float deltaFov = fovIncrease * dTimeInSecs * _fovIncreaseSpeedDegSec;
            _camera.FOV += deltaFov;
        }

      
    }
}
