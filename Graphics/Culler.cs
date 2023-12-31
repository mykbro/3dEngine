﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _3dGraphics.Graphics
{
    public enum CullResult { Outside, Partial, Inside };

    public static class Culler
    {
        private const int NumPlanes = 6;

        public static CullResult IsOBBoxInsideClipSpace(OBBox box)
        {
            Vector4[] boxPoints = box.Points;
            
            bool outsideOnePlane = false;
            bool partialOnePlane = false;

            //if we found an outside result for a plane we can immediately return, otherwise we need to continue until we found an outside result
            //if at the end we've always been inside we can say that we're totally inside
            for (int i = 0; i < NumPlanes && !outsideOnePlane; i++)
            {
                CullResult result = IsOBBoxInsidePlane(boxPoints, (PlaneId) i);
                
                switch (result)
                {
                    case CullResult.Outside:
                        outsideOnePlane = true;
                        break;
                    case CullResult.Partial:
                        partialOnePlane = true;
                        break;                   
                }                             
            }

            if (outsideOnePlane)
            {
                return CullResult.Outside;
            }
            else if (partialOnePlane)
            {
                return CullResult.Partial;
            }
            else
            {
                return CullResult.Inside;
            }            
        }       

        private static CullResult IsOBBoxInsidePlane(Vector4[] points, PlaneId planeId) 
        {
            bool insideFound = false;
            bool outsideFound = false;
            bool bothFound = false;
            
            //we iterate all the points until the end or until we found at least a point inside and one outside
            for (int i = 0; i < points.Length && !bothFound; i++)
            {
                bool result = IsPointInsidePlane(points[i], planeId);   //we could've reused Clipper.IsInsidePlane(...)
                if (result)
                {
                    insideFound = true;
                }
                else
                {
                    outsideFound = true;
                }               
               
                bothFound = insideFound && outsideFound;
            }

            if (bothFound)
            {
                return CullResult.Partial;
            }
            else if(insideFound)
            {
                return CullResult.Inside;
            }
            else
            {
                return CullResult.Outside;
            }
            
        }

        private static bool IsPointInsidePlane(Vector4 point, PlaneId planeId)
        {
            switch (planeId)
            {
                case PlaneId.Near:
                    return point.Z >= 0;
                case PlaneId.Far:
                    return point.Z <= point.W;
                case PlaneId.Left:
                    return point.X >= -point.W;
                case PlaneId.Right:
                    return point.X <= point.W;
                case PlaneId.Top:
                    return point.Y <= point.W;
                case PlaneId.Bottom:
                    return point.Y >= -point.W;
                default:
                    return false;
            }
        }
   
        public static List<WorldObject> GetVisibleObjectsFromOctree(Octree<WorldObject> octree, Matrix4x4 worldToProjMatrix)
        {
            List<WorldObject> visibleObjects = new List<WorldObject>();
            GetVisibleObjectsFromOctreeHelper(octree.RootNode, octree.RootCube, worldToProjMatrix, visibleObjects);

            return visibleObjects;
        }

        private static void GetVisibleObjectsFromOctreeHelper(OctreeNode<WorldObject> node, OctCube cube, Matrix4x4 worldToProjMatrix, List<WorldObject> outputList)
        {
           
            OBBox nodeBox = new OBBox(new AABBox(cube.MinX, cube.MaxX, cube.MinY, cube.MaxY, cube.MinZ, cube.MaxZ));
            OBBox projBox = OBBox.TranformOBBox(worldToProjMatrix, nodeBox);
            CullResult cullResult = IsOBBoxInsideClipSpace(projBox);

            switch (cullResult)
            {
                case CullResult.Partial:
                    //we add to the cull list all the items on this level and recursively call the function on our children
                    outputList.AddRange(node.NodeOnlyItems);

                    if (node.HasChildren)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            OctreeNode<WorldObject>? childNode = node.GetChildren(i);
                            if (childNode != null)
                            {
                                OctCube childCube = OctreeNode<WorldObject>.GetChildCube(cube, i);
                                GetVisibleObjectsFromOctreeHelper(childNode, childCube, worldToProjMatrix, outputList);
                            }
                        }
                    }
                    break;

                case CullResult.Inside:
                    //if the node is totally inside we're done, just need to recursively add all of its items to the renderlist (they'll all be totally inside too)
                    outputList.AddRange(node.AllItems);
                    break;

                case CullResult.Outside:
                    //we're done, do nothing
                    break;
            }

        }
    }

    
}
