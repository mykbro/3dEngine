using _3dGraphics.Graphics;
using _3dGraphics.Tests;
using _3dGraphics.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Threading;
using System.Text;

namespace _3dGraphics
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private World _world;
        private MainWindow _mainWindow;
        private ConsoleWindow _console;
        private Vector3 _cameraForwardMovement;     // 0 or 1
        private Vector3 _cameraBackwardMovement;    // 0 or 1
        private Vector3 _cameraPositiveRotation;
        private Vector3 _cameraNegativeRotation;
        private float _fovIncrease; //0 or 1
        private float _fovDecrease;

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            _mainWindow = new MainWindow(StartMovingCameraLeft, StopMovingCameraLeft, StartMovingCameraRight, StopMovingCameraRight,
                                        StartMovingCameraForward, StopMovingCameraForward, StartMovingCameraBackward, StopMovingCameraBackward,
                                        StartMovingCameraUp, StopMovingCameraUp, StartMovingCameraDown, StopMovingCameraDown,
                                        StartPitchingCameraDown, StopPitchingCameraDown, StartPitchingCameraUp, StopPitchingCameraUp,
                                        StartYawingCameraRight, StopYawingCameraRight, StartYawingCameraLeft, StopYawingCameraLeft,
                                        StartRollingCameraLeft, StopRollingCameraLeft, StartRollingCameraRight, StopRollingCameraRight,
                                        StartIncreasingFov, StopIncreasingFov, StartDecreasingFov, StopDecreasingFov);
            _console = new ConsoleWindow();

            Canvas windowCanvas = _mainWindow.Content as Canvas;
            CreateWorld((int) windowCanvas.Width, (int) windowCanvas.Height);

            _mainWindow.Show();
            _console.Show();

            //CameraTest.Run();
            StartEngineLoopAsync();
        }

        private void CreateWorld(int screenWidth, int screenHeight)
        {
            Vector3[] positions = {
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 1f, 0f),
                new Vector3(1f, 1f, 0f),
                new Vector3(0f, 0f, 1f),
                new Vector3(1f, 0f, 1f),
                new Vector3(0f, 1f, 1f),
                new Vector3(1f, 1f, 1f),
            };

            Triangle[] triangles = {
                new Triangle(0, 2 ,3),
                new Triangle(3, 1, 0),
                new Triangle(2, 6, 7),
                new Triangle(7, 3, 2),
                new Triangle(1, 3, 7),
                new Triangle(7, 5, 1),
                new Triangle(4, 0, 1),
                new Triangle(1, 5, 4),
                new Triangle(5, 7, 6),
                new Triangle(6, 4, 5),
                new Triangle(4, 6, 2),
                new Triangle(2, 0, 4),
            };

            //we create Vertex[] from Vector3[]
            var vertices = positions.Select<Vector3, Vertex>((pos) => new Vertex(pos));

            Mesh cube = new Mesh(vertices, triangles, Vector3.Zero, 1f);

            float FOV = 90f;
            float zNear = 0.1f;
            float zFar = 10f;
            float speedKmh = 5f;
            float rotSpeedDegSec = 60f;
            float fovIncSpeedDegSec = 10f;

            _world = new World(screenWidth, screenHeight, FOV, zNear, zFar, speedKmh, rotSpeedDegSec, fovIncSpeedDegSec);
            _world.Meshes.Add(cube);
            _world.Meshes.Add(cube);
            _world.Meshes.Add(cube);
            _world.Meshes.Add(cube);
            //_world.Camera.VelocityDirection = Vector3.Normalize(new Vector3(0.1f, -0.1f, -1f));
            _world.Meshes[1].Position = new Vector3(0, 0f, 5f);
            _world.Meshes[2].Position = new Vector3(5, 0f, 5f);
            _world.Meshes[3].Position = new Vector3(5, 0f, 0f);
            //_world.Camera.MoveBy(new Vector3(0f, 0f, -2f));
            //Render();
        }

        private void Render()
        {       

            Matrix4x4 worldToCamera = _world.Camera.WorldToCameraMatrix;
            Matrix4x4 projMatrix = _world.Camera.ProjectionMatrix;
            Matrix4x4 viewportMatrix = _world.Camera.ViewPortTransformMatrix;

            Matrix4x4 worldToProj = worldToCamera * projMatrix;

            /*
            //prepare the list of ALL vertices
            List<Vector4> vertices4D = new List<Vector4>(_world.TotalVertexCount);
            for (int i = 0; i < _world.Meshes.Count; i++)
            {
                Mesh m = _world.Meshes[i];
                for(int j=0; j < m.VertexCount; j++)
                {
                    vertices4D.Add(m.GetVertex(j).Position4D);
                }
            }
            */

            for (int i = 0; i < _world.Meshes.Count; i++)
            {
                Mesh mesh = _world.Meshes[i];

                //initialize the Vec4
                Vector4[] vertices4D = new Vector4[mesh.VertexCount];
                for (int v = 0; v < mesh.VertexCount; v++)
                {
                    vertices4D[v] = mesh.GetVertex(v).Position4D;
                }

                /*
                Triangle[] triangles = new Triangle[mesh.TriangleCount];
                for (int t = 0; t < mesh.TriangleCount; t++)
                {
                    triangles[t] = mesh.GetTriangle(t);
                }
                */

                //calculate global Matrix
                Matrix4x4 localToWorld = mesh.LocalToWorldMatrix;
                Matrix4x4 globalMatrix = localToWorld * worldToProj;

                //projection
                for (int v = 0; v < vertices4D.Length; v++)
                {
                    vertices4D[v] = Vector4.Transform(vertices4D[v], globalMatrix);
                    /*
                    vertices4D[v] = Vector4.Transform(vertices4D[v], localToWorld);
                    vertices4D[v] = Vector4.Transform(vertices4D[v], worldToCamera);
                    vertices4D[v] = Vector4.Transform(vertices4D[v], projMatrix);
                    */
                }

                //division
                for (int v = 0; v < vertices4D.Length; v++)
                {
                    vertices4D[v] = vertices4D[v] / vertices4D[v].W;
                }

                //transformation to viewport                
                for (int v = 0; v < vertices4D.Length; v++)
                {
                    vertices4D[v] = Vector4.Transform(vertices4D[v], viewportMatrix);
                }

                //preparing string to display in console
                StringBuilder consoleSB = new StringBuilder();
                consoleSB.AppendLine(String.Format("X: {0:F3}", _world.Camera.Position.X));
                consoleSB.AppendLine(String.Format("Y: {0:F3}", _world.Camera.Position.Y));
                consoleSB.AppendLine(String.Format("Z: {0:F3}", _world.Camera.Position.Z));
                consoleSB.AppendLine();
                consoleSB.AppendLine(String.Format("thetaX: {0:F3}", _world.Camera.Orientation.X));
                consoleSB.AppendLine(String.Format("thetaY: {0:F3}", _world.Camera.Orientation.Y));
                consoleSB.AppendLine(String.Format("thetaZ: {0:F3}", _world.Camera.Orientation.Z));
                consoleSB.AppendLine();
                consoleSB.AppendLine(String.Format("FOV: {0:F3}", _world.Camera.FOV));

                //draw
                Dispatcher.Invoke(() =>
                    {
                        _mainWindow.ClearCanvas();
                        for (int t = 0; t < mesh.TriangleCount; t++)
                        {
                            Triangle tempTri = mesh.GetTriangle(t);
                            Point p1 = new Point(vertices4D[tempTri.V1Index].X, vertices4D[tempTri.V1Index].Y);
                            Point p2 = new Point(vertices4D[tempTri.V2Index].X, vertices4D[tempTri.V2Index].Y);
                            Point p3 = new Point(vertices4D[tempTri.V3Index].X, vertices4D[tempTri.V3Index].Y);
                             
                            _mainWindow.DrawTriangle(p1, p2, p3);
                            _console.Clear();
                            _console.WriteLine(consoleSB.ToString());
                            
                        }
                    }, DispatcherPriority.Background);
            }

        }

        public Task RenderAsync()
        {
            return Task.Run(() => Render());
        }

        public async void StartEngineLoopAsync()
        {
            Stopwatch globalWatch = Stopwatch.StartNew();
            double lastCycleTimeInSecs = 0.0;
            

            while (true)
            {
                double timeInSecs = (globalWatch.ElapsedTicks * 1.0) / Stopwatch.Frequency;
                float deltaTimeInSecs = (float) (timeInSecs - lastCycleTimeInSecs);

                Vector3 cameraMovement = _cameraForwardMovement - _cameraBackwardMovement; 
                if (cameraMovement != Vector3.Zero)
                    cameraMovement = Vector3.Normalize(cameraMovement);

                Vector3 cameraRotation = _cameraPositiveRotation - _cameraNegativeRotation;
                float fovChange = _fovIncrease - _fovDecrease;

                _world.Update(deltaTimeInSecs, cameraMovement, cameraRotation, fovChange);
                await RenderAsync(); 
                await Task.Delay(1);        //to keep the FPS at bay

                int fps = (int) (1.0f / deltaTimeInSecs);
                _mainWindow.Title = "FPS: " + fps;

                lastCycleTimeInSecs = timeInSecs;
            }
        }

        #region MOVEMENT
        /// START MOVING
        private void StartMovingCameraRight() 
        {
            _cameraForwardMovement.X = 1f;
        }

        private void StartMovingCameraLeft()
        {
            _cameraBackwardMovement.X = 1f;
        }

        private void StartMovingCameraUp()
        {
            _cameraForwardMovement.Y = 1f;
        }

        private void StartMovingCameraDown()
        {
            _cameraBackwardMovement.Y = 1f;
        }

        private void StartMovingCameraForward()
        {
            _cameraForwardMovement.Z = 1f;
        }

        private void StartMovingCameraBackward()
        {
            _cameraBackwardMovement.Z = 1f;
        }

        /// STOP MOVING
        
        private void StopMovingCameraRight()
        {
            _cameraForwardMovement.X = 0f;
        }

        private void StopMovingCameraLeft()
        {
            _cameraBackwardMovement.X = 0f;
        }

        private void StopMovingCameraUp()
        {
            _cameraForwardMovement.Y = 0f;
        }

        private void StopMovingCameraDown()
        {
            _cameraBackwardMovement.Y = 0f;
        }

        private void StopMovingCameraForward()
        {
            _cameraForwardMovement.Z = 0f;
        }
                
        private void StopMovingCameraBackward()
        {
            _cameraBackwardMovement.Z = 0f;
        }
        #endregion

        #region ROTATION
        private void StartPitchingCameraDown()  //positive rotation is clockwise around the X axis
        {
            _cameraPositiveRotation.X = 1f;
        }

        private void StartPitchingCameraUp()
        {
            _cameraNegativeRotation.X = 1f;
        }

        private void StartYawingCameraRight()    //positive rotation is clockwise around the Y axis
        {
            _cameraPositiveRotation.Y = 1f;
        }

        private void StartYawingCameraLeft()
        {
            _cameraNegativeRotation.Y = 1f;
        }

        private void StartRollingCameraLeft()  //positive rotation is clockwise around the Z axis but Z axis is reversed
        {
            _cameraPositiveRotation.Z = 1f;
        }

        private void StartRollingCameraRight()
        {
            _cameraNegativeRotation.Z = 1f;
        }

        private void StopPitchingCameraDown()
        {
            _cameraPositiveRotation.X = 0f;
        }

        private void StopPitchingCameraUp()
        {
            _cameraNegativeRotation.X = 0f;
        }

        private void StopYawingCameraRight()
        {
            _cameraPositiveRotation.Y = 0f;
        }

        private void StopYawingCameraLeft()
        {
            _cameraNegativeRotation.Y = 0f;
        }   

        private void StopRollingCameraLeft()
        {
            _cameraPositiveRotation.Z = 0f;
        }

        private void StopRollingCameraRight()
        {
            _cameraNegativeRotation.Z = 0f;
        }

        #endregion

        #region FOV
        public void StartIncreasingFov()
        {
            _fovIncrease = 1;
        }

        public void StartDecreasingFov()
        {
            _fovDecrease = 1;
        }

        public void StopIncreasingFov()
        {
            _fovIncrease = 0;
        }

        public void StopDecreasingFov()
        {
            _fovDecrease = 0;
        }
        #endregion

    }
}
