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
using System.Windows.Media;
using System.Windows.Shapes;
using System.Reflection;
using System.IO;
using System.Globalization;
using PointF = System.Drawing.PointF;
using DrawingColor = System.Drawing.Color;


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
        private float _fovDecrease; //0 or 1
        private RenderTarget _renderTarget;

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            _mainWindow = new MainWindow(StartMovingCameraLeft, StopMovingCameraLeft, StartMovingCameraRight, StopMovingCameraRight,
                                        StartMovingCameraForward, StopMovingCameraForward, StartMovingCameraBackward, StopMovingCameraBackward,
                                        StartMovingCameraUp, StopMovingCameraUp, StartMovingCameraDown, StopMovingCameraDown,
                                        StartPitchingCameraUp, StopPitchingCameraUp, StartPitchingCameraDown, StopPitchingCameraDown,
                                        StartYawingCameraRight, StopYawingCameraRight, StartYawingCameraLeft, StopYawingCameraLeft,
                                        StartRollingCameraLeft, StopRollingCameraLeft, StartRollingCameraRight, StopRollingCameraRight,
                                        StartIncreasingFov, StopIncreasingFov, StartDecreasingFov, StopDecreasingFov);
            _console = new ConsoleWindow();
            
            CreateWorld(_mainWindow.ScreenWidth, _mainWindow.ScreenHeight);
            _renderTarget = new RenderTarget(_mainWindow.ScreenWidth, _mainWindow.ScreenHeight);

            _mainWindow.Show();
            _console.Show();
            
            StartEngineLoopAsync();
        }

        private void CreateWorld(int screenWidth, int screenHeight)
        {  
            
            float FOV = 90f;
            float zNear = 0.05f;
            float zFar = 100f;
            float speedKmh = 6f;
            float rotSpeedDegSec = 60f;
            float fovIncSpeedDegSec = 30f;

            //we create the world and populate it with objects
            _world = new World(screenWidth, screenHeight, FOV, zNear, zFar, speedKmh, rotSpeedDegSec, fovIncSpeedDegSec);

            //Generate100Cubes();            
            Mesh objToLoad = LoadMeshFromObjFile(@"D:\Objs\teapot.txt");            

            _world.Objects.Add(new WorldObject(objToLoad, Vector3.Zero, 1f));
            //_world.Objects.Add(new WorldObject(objToLoad, new Vector3(10f, 0f, 0f), 1f));
            //_world.Objects.Add(new WorldObject(objToLoad, new Vector3(10f, 0f, 10f), 1f));
            //_world.Objects.Add(new WorldObject(objToLoad, new Vector3(0f, 0f, 10f), 1f));

            _world.Camera.MoveBy(new Vector3(0f, 3f, -6f));
            //_world.Camera.RotateBy(new Vector3(1.5f, 4f, 0));

            /*
            //big distances test            
            float d = 2e6f;
            Vector3 movement = Vector3.One * d;
            _world.Camera.MoveBy(movement);
            foreach (WorldObject obj in _world.Objects)
            {
                obj.MoveBy(movement);
            }
            */
        }

        private void Generate100Cubes()
        {
            Mesh objToLoad = LoadMeshFromObjFile(@"D:\Objs\teapot.txt");

            for(int i=0; i<3; i++)
            {
                for(int j=0; j<3; j++)
                {
                    _world.Objects.Add(new WorldObject(objToLoad, new Vector3(i*10, 0, j*10), 1f));
                }
            }
        }

        private void Render()
        {           

            Matrix4x4 worldToCamera = _world.Camera.WorldToCameraMatrix;
            Matrix4x4 projMatrix = _world.Camera.ProjectionMatrix;
            Matrix4x4 viewportMatrix = _world.Camera.ViewPortTransformMatrix; 
            Matrix4x4 worldToProj = worldToCamera * projMatrix;            

            DebugInfo debugInfo = new DebugInfo();

            //we clear our RenderTarget for the new pass
            _renderTarget.Clear();

            //we render each object concurrently (every task will also render each fragment concurrently)
            Parallel.ForEach(_world.Objects, (wObj) => RenderObject(wObj, worldToProj, viewportMatrix, debugInfo));            

            //preparing text to display in the console
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
            consoleSB.AppendLine();
            consoleSB.AppendLine(String.Format("Vertices: {0}", debugInfo.NumVerticesFromObjects));
            consoleSB.AppendLine(String.Format("Triangles (meshes): {0}", debugInfo.NumTrianglesFromObjects));
            consoleSB.AppendLine(String.Format("Triangles (facing): {0}", debugInfo.NumTrianglesSentToClip));
            consoleSB.AppendLine(String.Format("Triangles (render): {0}", debugInfo.NumTrianglesSentToRender));

            String consoleText = consoleSB.ToString();
                       
            //draw
            Dispatcher.Invoke(() =>
            {
                //_mainWindow.DrawFragments(fragments);
                _mainWindow.Draw(_renderTarget.Data, _renderTarget.Stride);
                _console.Clear();
                _console.WriteLine(consoleText);
            }, DispatcherPriority.Background);
        }

        private Task RenderAsync()
        {
            return Task.Run(() => Render());
        }

        private async void StartEngineLoopAsync()
        {
            Stopwatch globalWatch = Stopwatch.StartNew();
            double lastCycleTimeInSecs = 0.0;
            long numFrames = 0;
            

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
                //await Task.Delay(1);        //to keep the FPS at bay

                int fps = (int)(1.0f / deltaTimeInSecs);
                int avgFps = (int)(numFrames / timeInSecs);
                _mainWindow.Title = String.Format("FPS: {0} | AVG: {1}", fps, avgFps);

                numFrames++;

                lastCycleTimeInSecs = timeInSecs;
            }
        }

        private void RenderObject(WorldObject wObject, Matrix4x4 worldToProj, Matrix4x4 viewportMatrix, DebugInfo debugInfo) 
        {            
            Mesh mesh = wObject.Mesh;                        //store to avoid repetitive calls
            int numVertices = mesh.VertexCount;             //store to avoid repetitive calls
            int numTriangles = mesh.TriangleCount;          //store to avoid repetitive calls

            //we create an empty List of Vector4 sized to the total nr of Vertices...
            //we'll fill it all but we'll avoid relocations and we're going to create new Vertices during the Clipping stage so an Array would not work
            //we'll also create a companion list of bools that will keep track of the Vertices we need to transform proceeding in the pipeline (initialized to false)
            List<Vector4> vertices4D = new List<Vector4>(numVertices);
            List<bool> verticesMask = new List<bool>(numVertices);
            //we also create a list for the Triangles that we want to clip sizing it to TriangleCount... we'll probably fill half of it
            List<Triangle> trianglesToClip = new List<Triangle>(numTriangles);


            //we populate the lists
            for (int vIndex = 0; vIndex < numVertices; vIndex++)
            {
                vertices4D.Add(mesh.GetVertex(vIndex).Position4D);
                verticesMask.Add(false);
            }

            //we transform the camera from World to Object space for backface culling and illumination using normals 
            Matrix4x4 worldToLocalMatrix = wObject.WorldToLocalMatrix;
            Vector3 cameraPosInObjSpace = Vector3.Transform(_world.Camera.Position, worldToLocalMatrix);

            //...and we check each mesh's triangle asserting the vertices' flags and adding the triangle to the processTriangles list
            for (int tIndex = 0; tIndex < numTriangles; tIndex++)
            {
                Triangle tempTriangle = mesh.GetTriangle(tIndex);
                Vector3 triangleBarycenter = (mesh.GetVertex(tempTriangle.V1Index).Position3D + mesh.GetVertex(tempTriangle.V2Index).Position3D + mesh.GetVertex(tempTriangle.V3Index).Position3D) / 3f;
                Vector3 pointToCameraVec = cameraPosInObjSpace - triangleBarycenter;
                Vector3 pointToCameraVecNormalized = Vector3.Normalize(pointToCameraVec);
                float scalarProd = Vector3.Dot(pointToCameraVecNormalized, mesh.GetNormal(tIndex));

                if (scalarProd > 0)
                {
                    trianglesToClip.Add(new Triangle(tempTriangle.V1Index, tempTriangle.V2Index, tempTriangle.V3Index, scalarProd));    //we calculate the illumination                        
                    verticesMask[tempTriangle.V1Index] = true;
                    verticesMask[tempTriangle.V2Index] = true;
                    verticesMask[tempTriangle.V3Index] = true;
                }
            }

            //calculate global Matrix
            Matrix4x4 localToWorld = wObject.LocalToWorldMatrix;
            Matrix4x4 globalMatrix = localToWorld * worldToProj;

            //projection
            for (int vIndex = 0; vIndex < vertices4D.Count; vIndex++)
            {
                if (verticesMask[vIndex])
                {
                    Vector4 temp = Vector4.Transform(vertices4D[vIndex], globalMatrix);
                    vertices4D[vIndex] = temp;
                    if (Clipper.IsPointInsideViewVolume(temp))
                    {
                        //we leave the vertexMash to true to signal that the vertex is totally inside (for the next clip stage)
                    }
                    else
                    {
                        verticesMask[vIndex] = false;  //we reset the mask for the clipping stage
                    }

                }
            }

            //we create a new List<Triangle> for the triangles that passes the clip stage initializing it to trianglesToClip.Count
            List<Triangle> trianglesToRender = new List<Triangle>(trianglesToClip.Count);

            //triangle clipping
            for (int tIndex = 0; tIndex < trianglesToClip.Count; tIndex++)
            {
                List<Triangle> clipResults = Clipper.ClipTriangleAndAppendNewVerticesAndTriangles(trianglesToClip[tIndex], vertices4D, verticesMask);
                trianglesToRender.AddRange(clipResults);
            }

            //division and transformation to viewport
            for (int vIndex = 0; vIndex < vertices4D.Count; vIndex++)
            {
                if (verticesMask[vIndex])
                {
                    Vector4 temp = vertices4D[vIndex];
                    Vector4 dividedVertex = temp / temp.W;

                    vertices4D[vIndex] = Vector4.Transform(dividedVertex, viewportMatrix);
                }
            }

            //creating the fragments to display
            /*
            for (int tIndex = 0; tIndex < trianglesToRender.Count; tIndex++)
            {
                Triangle tempTri = trianglesToRender[tIndex];

                Vector4 v1 = vertices4D[tempTri.V1Index];
                Vector4 v2 = vertices4D[tempTri.V2Index];
                Vector4 v3 = vertices4D[tempTri.V3Index];

                int colorLevel = (int)(tempTri.LightIntensity * 255);
                DrawingColor col = DrawingColor.FromArgb(colorLevel, colorLevel, colorLevel);

                Fragment3D frag = new Fragment3D(new Vector3(v1.X, v1.Y, v1.Z), new Vector3(v2.X, v2.Y, v2.Z), new Vector3(v3.X, v3.Y, v3.Z), col);

                _renderTarget.RenderFragment(frag);
            }
            */
            Parallel.ForEach(trianglesToRender, (t) => RenderFragment(t, vertices4D));

            lock (debugInfo)
            {
                debugInfo.NumVerticesFromObjects += numVertices;
                debugInfo.NumTrianglesFromObjects += numTriangles;
                debugInfo.NumTrianglesSentToClip += trianglesToClip.Count;
                debugInfo.NumTrianglesSentToRender += trianglesToRender.Count;
            }        
        }

        private void RenderFragment(Triangle tempTri, List<Vector4> vertices4D)
        {
            Vector4 v1 = vertices4D[tempTri.V1Index];
            Vector4 v2 = vertices4D[tempTri.V2Index];
            Vector4 v3 = vertices4D[tempTri.V3Index];

            int colorLevel = (int)(tempTri.LightIntensity * 255);
            DrawingColor col = DrawingColor.FromArgb(colorLevel, colorLevel, colorLevel);

            Fragment3D frag = new Fragment3D(new Vector3(v1.X, v1.Y, v1.Z), new Vector3(v2.X, v2.Y, v2.Z), new Vector3(v3.X, v3.Y, v3.Z), col);

            //_renderTarget.RenderFragment(frag);
            _renderTarget.RenderFragmentUsingScanLine(frag);
        }

        private static Mesh LoadMeshFromObjFile(string filename)
        {
            List<Vertex> vertices = new List<Vertex>();
            List<Triangle> triangles = new List<Triangle>();

            using (FileStream fs = File.OpenRead(filename))
            {
                using (StreamReader objReader = new StreamReader(fs))
                {
                    while (!objReader.EndOfStream)
                    {
                        string text = objReader.ReadLine();
                        string[] parts = text.Split();
                        switch (parts[0])
                        {
                            case "v":
                                float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                                float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                                float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                                vertices.Add(new Vertex(new Vector3(x, y, z)));
                                break;
                            case "f":
                                int v1 = Int32.Parse(parts[1].Split('/')[0]);
                                int v2 = Int32.Parse(parts[2].Split('/')[0]);
                                int v3 = Int32.Parse(parts[3].Split('/')[0]);
                                triangles.Add(new Triangle(v1 - 1, v2 - 1, v3 - 1, 1f));    //obj file indexes count from 1; we also initialize to MAX luminosity
                                break;
                            default:
                                break;
                        }

                    }
                }

                return new Mesh(vertices, triangles);
            }
        }



        #region MOVEMENT CMDs
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

        #region ROTATION CMDs
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

        #region FOV CMDs
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
