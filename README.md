# 3dGraphics

## Disclaimer
This codebase is made for self-teaching and educational purposes only.
Many features like input validation, object disposed checks, some exception handling, etc... are mostly missing.
As such this codebase cannot be considered production ready.

## What's this ?
This is a WPF app implementing a parallelizable software 3D engine including perspective-corrected texturing, basic lightning, z-buffering and octree space partitioning for object pruning.  
The parallelization is implemented both at the mesh level and at the fragment level. For every single mesh each fragment is also rendered concurrently (using a double-check lock tecnique for buffer and z-buffer concurrent access).  
The parallelization is achieved using the Parallel library and the Task CLR infrastucture. 

Classes like Vector3 and Matrix4x4 from System.Numerics are also used for additional parallelization at the instruction level leveraging SIMD instructions.

The app can read meshes from .obj files containing vertexes/triangles information and can load images as textures.

## How does it work ?

### What are the conventions used in this project ?
We use a positive Z axis pointing away from the camera.  
We use a clockwise vertex ordering for triangle definition.  
Our NDC space (Normalized Device Coordinates) goes from -1 to 1 for X and Y axis and from 0 to 1 for the Z axis.

The terms WorldObject, object and mesh will be used interchangebly. 

The following coordinate systems (aka "spaces") will be mentioned:
* Local space (also called Object space or Model space, centered on the object persepective)
* World space (the global reference system where the camera and the objects are placed)	
* View space (also called Camera space, centered on the camera perspective)
* Clip space (also called Projection space, an intermediate space used for computationally efficient culling and clipping triangles)
* NDC space (Normalized Device Coordinates space, obtained after the "Perspective division" from the Clip space)
* Viewport space (the 2D space of the screen)

### What are the main classes ? 
At the high level the app implements a World consisting of a List\<WorldObjects>, an Octree\<WorldObjects> containing the references to all the world objects for space partitioning and a Camera object containing all the necessary info for the rendering including its position relative to the World.

The World is defined by its size (or halfSize) and the origin is placed at the center of it. The size only matters for the Octree definition; WorldObjects and the Camera can be placed and move freely outside of this size.  
Ofc floating point arithmetic and loss of precision must be taken into account when "far from the origin".

A WorldObject has a reference to a Mesh and a position and scale relative to the world.

A Mesh is defined by a list of Vertexes represented as a 4D vector in homogeneous coordinates (x, y, z, w), a list of triangles using indexing for vertex attribution, a list of triangle normals (for triangle culling and illumination) and a mesh bounding box (Axis Aligned in our case) used for early coarse frustum culling.

A Triangle beyond the vertex indexes also contains its texture coordinates info.

The Octree partitions the whole World in a "multilevel 3D cube grid" up to N levels deep where WorldObjects are placed.  
A WorldObject will be placed inside the smallest "cube" that can wholly contain it without the object crossing any bound. For example even a small sphere placed exactly at the origin will be held at the level 0 cube (the cube that spans the whole World).

The RenderTarget class represents an imaginary "canvas" on which we can draw our fragments. This class will take care of the z-buffer and the concurrency control.  
At the end of the render pipeline, each frame, we take the raw bytes representing our drawing from the RenderTarget and call the MainWindow .Draw(:byte[]) method. This method will "draw" these bytes on an internal WritableBitmap and then update an Image with this WritableBitmap in order to finally display the frame on the screen.

The Culler static class implements the WorldObject hierarchical pruning using the octree.  
This pruning throws away all the WorldObjects inside the "cubes" that can't be seen from the camera perspective in that particular situation. When dealing with thousands of meshes this greatly speeedup the process otherwise every mesh's bounding box would have to be checked for culling against the view frustum.  
This class also implements the aforementioned frustum culling for objects that are in visible "cubes". Each mesh bounding box is checked against the frustum volume and if there's a partial intersection (which doesn't mean that the object would be visible) the object is sent to the rendering pipeline.

The Clipper static class implements triangle clipping (triangle is inside clip space, triangle is outside clip space, or new triangle(s) must be created) and texture coordinate interpolation.

DoubleVector3 is a custom class that encapsulates a Vector of doubles. We use doubles instead of floats for values like positions where many small quantities might be added (small dX when moving at small speeds over small dT) to prevent floating point arithmetic errors.  
Kahan algorithm could have been used instead (using 2 floats instead of a double) but it would have been slower.

### What's the rendering pipeline used in this project ? 	 

1. #### Pruning stage
	We check every "cube" (space partition) of the octree starting from the root cube (level 0) to see if it's totally inside, outside or partially inside the frustum.
	* If it's totally inside we recursively select all the cube's and sub-cube's WorldObjects for the next stage.
	* It it's outside we do nothing and advance to the next cube.
	* If it's partially inside we select all the cube's WorldObjects for the next stage (but NOT the sub-cube's ones) and repeat the analysis recursively for the cube sub-cubes (if not at the last level).
2. #### Frustum culling stage
	For each mesh we take its AABB (Axis-Aligned Bounding Box) in local space and transform it to clip space obtaining a generic OBB (Oriented Bounding Box). The transformation is done for faster culling and can be reused later.  
	We check if the OBB is inside the frustum. It's not if it's totally outside at least one plane of the frustum. However this check can generate false positives (objects that are outside but are calculated as inside) but thankfully no negatives (we never throw away objects that must be displayed).  
    To solve the "false positives" problem we should also do an "inverse check" also checking the frustum volume against the OBB but this would be too slow and we chose to throw away false positives at the clipping stage.
3. #### Backface culling and illumination stage
	For each object we transform the camera to the object's local space for early backface culling and light intensity calculations (using triangle normals dot product for both the operations). The non-culled triangles are marked for the next stage.
4. #### Projection stage
	We transform each vertex from the selected triangles from local to projection (clip) space using the LocalToWorldMatrix and WorldToProjMatrix multiplied together.
5. #### Clipping stage
	For each triangle, based on the number of its vertexes inside the view frustum, we choose if we have to:
	* Throw it away (with 0 vertexes inside)
	* Reduce it to a new smaller triangle totally inside the frustum (with 1 vertex inside)
	* Split it into 2 new smaller triangles totally inside the frustum (with 2 vertexes inside)
	* Keep it (with 3 vertexes inside)

	The process will be repeated for all the 6 planes of the frustum leading to situations where a single original triangle can be split into many smaller new triangles.

6. #### Perspective dision stage
	For each vertex we divide its coordinates by the W component obtaining the Normal Device Coordinates (NDC).
7. #### Viewport transform stage
	For each vertex we transform its coordinates from NDC to viewport space.

### What's the program flow ? 
This app is a WPF app. We subscribe an Application_Startup() method to the app Startup event in the XAML. In this method we create our whole World and asynchronously starts the main loop calling StartEngineLoopAsync().  
In this loop we do 3 main things:

*	Acquire the user inputs
*	Update the world based on the inputs (camera movement, but object movement would be implemented here too)
*	Render the frame

The RenderAsync() method asynchronously calls Render().  

Render() is an important method that setups some data that will be reused multiple times, clear the frame buffer, prune all the world objects and call CullAndRenderObject() using the Parallel library for each visible object and after all the parallel tasks end it finally draws the frame on the screen using Dispatcher.Invoke(...) as we're not on the main GUI thread.

CullAndRenderObject() as the name says obviously calls both the bounding box culling using Culler.IsOBBoxInsideClipSpace() and, in case the mesh passes the culling stage, the RenderObject() method. The two operations are linked together because they both reuse a matrix that we want to calculate only once for performance reasons: the LocalToProjMatrix.  

RenderObject() is where the rendering pipeline is actually implemented. At the end it concurrently calls _renderTarget.RenderFragment() for each fragment using the Parallel library. Please refer to the source code for more details.

RenderTarget.RenderFragment() implements the rasterization process. For more details please check its source code.  
As we didn't quite manage to implement the scanline algorithm correctly (an attempt can be found in the 'scanlineRenderingAttempt' branch) we used a custom algorithm that for each row we determine where the first and last pixel to draw are, starting from the beginning and from the end. 


## How should I use this ?

Please check the 'Resources' package for some samples to test this app.

### User inputs

The camera controls are:

* Arrow keys for forward/backward and lateral movement
* PageUp/PageDown for lifting up/down
* W/S for camera pitch
* A/D for camera yaw
* Q/E for camera roll
* +/- for FOV and zoom control

these controls are hardwired and can be found in MainWindow.xaml.cs.

### App configuration

The World parameters are defined in App.CreateWorld():

	private void CreateWorld(int screenWidth, int screenHeight)
    {  
        //## World parameters
        float FOV = 90f;
        float zNear = 0.05f;
        float zFar = 75;
            
        float speedKmh = 6f;
        float rotSpeedDegSec = 60f;
        float fovIncSpeedDegSec = 30f;
            
        float worldspaceHalfSizeMeters = 512f;
        int octreeMaxDepth = 8;

        // We create the world and populate it with objects
        _world = new World(screenWidth, screenHeight, FOV, zNear, zFar, speedKmh, rotSpeedDegSec, fovIncSpeedDegSec, worldspaceHalfSizeMeters, octreeMaxDepth);		

the viewport width and height can be changed in the MainWindow.xaml definition file:

	<Image Name="_canvas" Height="1024" Width="1280"></Image>

WorldObjects can be loaded and added to the World in the section:

    //## WorldObject creation
    Mesh objToLoad = LoadMeshFromObjFile(@"D:\Objs\Ganesha\Ganesha.obj.txt", true);
    //Mesh objToLoad = LoadMeshFromObjFile(@"D:\Objs\alduin\alduin.obj.txt", true);
    //Mesh objToLoad = LoadMeshFromObjFile(@"D:\Objs\teapot.txt", false);
    //Mesh objToLoad = LoadMeshFromObjFile(@"D:\Objs\dragon.txt", false);

    _world.AddWorldObject(new WorldObject(objToLoad, Vector3.Zero, 1f));

please provide a correct path for the file to load.  
The boolean value must be 'true' for .obj files containing vt info (vertex texture) and 'false' if not, otherwise LoadMeshFromObjFile() will fail.  
The .AddWorldObject(obj, pos, scale) method add the object in the 'pos' position in the world scaling it by the 'scale' factor.


A GenerateManyObjects() method exists to spawn multiple objects regularly spaced in the World.  
If you want to use this method the whole 'WorldObject creation' section should be commented out. Please check the method's source code for more details.

After this the camera can be configured:
        
    //## Camera placement
    _world.Camera.MoveBy(new Vector3(0f, 3f, -6f));
    //_world.Camera.MoveBy(new Vector3(0.5f, 3f, 0.5f));
    //_world.Camera.RotateBy(new Vector3(1.5f, 4f, 0));

and finally the texture loaded. Please note that only a single texture is supported.

    //## Texture loading
    //_myTexture = new Texture(@"D:\Objs\alduin\alduin.jpg");
    _myTexture = new Texture(@"D:\Objs\Ganesha\Ganesha.png");
    //_myTexture = new Texture(@"D:\Objs\_textures\white.bmp");
    //_myTexture = new Texture(@"D:\Objs\_textures\smile.bmp");

for meshes without textures please use the 'white.bmp' file. This will load a totally white texture.

### A note on .obj files loading
As the open standard for .obj files uses different conventions for the Z axis (negative instead of positive) and the vertex ordering (counter-clockwise instead of clockwise) compared to our app, the loading process must take this into account especially for texture mapping.

In the App.LoadMeshFromObjFile() method we correct the Z axis discrepancy here:

     vertices.Add(new Vector4(x, y, -z, 1f));

the texture mapping discrepancy here:

    textureCoords.Add(new Vector3(u, 1-v, 1f));

and the rotation discrepancy here:

    triangles.Add(new Triangle(v3 - 1, v2 - 1, v1 - 1, t3, t2, t1, 1f));

PLEASE NOTE that however, for some meshes, sometimes the final texturing is not working correctly and especially the line

    textureCoords.Add(new Vector3(u, 1-v, 1f));

must be tuned using (u,v) or (1-u, 1-v). Try different settings if the result is not satisfying.

### I cannot see my mesh or the texturing is wrong
If you cannot see anything please check:
* The camera position 
* The object(s) position
* The object(s) scaling (e.g. the Alduin mesh requires a scaling of 0.01f)

If the texturing is wrong please check the previous section and tune the textureCoords.Add(new Vector3(u, 1-v, 1f)) call accordingly.








