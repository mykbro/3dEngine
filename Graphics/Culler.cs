using System;
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

        public static void FillCullAndRenderListsFromQuadtree(Quadtree<WorldObject> qTree, Matrix4x4 worldToProjMatrix, List<WorldObject> objectsThatNeedCulling, List<WorldObject> objectsReadyForRender)
        {
            FillCullAndRenderListsHelper(qTree.RootNode, qTree.RootTile, worldToProjMatrix, objectsThatNeedCulling, objectsReadyForRender);
        }

        private static void FillCullAndRenderListsHelper(QuadtreeNode<WorldObject> node, QuadTile tile, Matrix4x4 worldToProjMatrix, List<WorldObject> objectsThatNeedCulling, List<WorldObject> objectsReadyForRender)
        {
            int halfSize = tile.HalfSize;

            //we construct a 3D box for the tile, using Z for Y (and Y for Z ofc) with a height of halfSize
            //(by not using an octree we have to use "columns" that spans the whole 3D space in the Y axis)
            //however in this way a tile would never be TOTALLY contained inside the clip volume
            //for this reason we choose to limit our space to -1 and 1 in the Y axis in our tests
            OBBox nodeBox = new OBBox(new AABBox(tile.MinX, tile.MaxX, -1, 1, tile.MinY, tile.MaxY));

            OBBox projBox = OBBox.TranformOBBox(worldToProjMatrix, nodeBox);
            CullResult cullResult = IsOBBoxInsideClipSpace(projBox);

            switch (cullResult)
            {
                case CullResult.Partial:
                    //we add to the cull list all the items on this level and recursively call the function on our children
                    objectsThatNeedCulling.AddRange(node.NodeOnlyItems);
                    if (node.HasChildren)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            QuadtreeNode<WorldObject>? childNode = node.GetChildren(i);
                            if (childNode != null)
                            {
                                QuadTile childTile = QuadtreeNode<WorldObject>.GetChildTile(tile, i);
                                FillCullAndRenderListsHelper(childNode, childTile, worldToProjMatrix, objectsThatNeedCulling, objectsReadyForRender);
                            }
                        }
                    }
                    break;

                case CullResult.Inside:
                    //if the node is totally inside we're done, just need to recursively add all of its items to the renderlist (they'll all be totally inside too)
                    objectsReadyForRender.AddRange(node.AllItems);
                    break;

                case CullResult.Outside:
                    //we're done, do nothing
                    break;
            }

        }
    }

    
}
