using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace _3dGraphics.Graphics
{
    public class Octree<T>
    {
        private readonly OctreeNode<T> _lvlZeroNode;
        private readonly float _halfSize;           //width/length of the lvl0 node
        private readonly int _maxDepth;         //lowest level we want to reach (starts from 1)
        private readonly OctCube _lvlZeroCube; //precomputed from size        

        public List<T> Items => _lvlZeroNode.AllItems;
        public OctCube RootCube => _lvlZeroCube;
        public OctreeNode<T> RootNode => _lvlZeroNode;
        public float HalfSize => _halfSize;
        
        public Octree(float halfSize, int maxDepth)
        {
            _lvlZeroNode = new OctreeNode<T>();            
            _maxDepth = maxDepth;
            _halfSize = halfSize;
           
            _lvlZeroCube = new OctCube(0, 0, 0, halfSize);
        }

        public void Add(T obj, AABBox objBox)
        {
            _lvlZeroNode.Add(obj, objBox, _lvlZeroCube, _maxDepth - 1);
        }       
    }

    public class OctreeNode<T>
    {
        private List<T> _nodeObjects;
        private OctreeNode<T>[] _subtrees;        
        private const int NUM_CHILDREN = 8;

        public bool HasChildren => (_subtrees != null);        
        
        public List<T> NodeOnlyItems
        {
            get
            {
                if(_nodeObjects == null)
                {
                    return new List<T>();
                }
                else
                {
                    return _nodeObjects;
                }
            }
        }

        public List<T> AllItems
        {
            get
            {
                //we use a listbuilder in order to avoid lots of List instantiations
                List<T> toReturn = new List<T>();
                AppendNodeItemsHelper(toReturn);

                return toReturn;
            }
        }

        public OctreeNode()
        {
            //we do not instance our structures immediately but on demand for the first item
            _nodeObjects = null;              
            _subtrees = null;
        }

        public OctreeNode<T>? GetChildren(int i)
        {
            if(this.HasChildren && _subtrees[i] != null)
            {
                return _subtrees[i];
            }
            else 
            { 
                return null; 
            }
        }

        public void Add(T obj, AABBox objBox, OctCube cube, int levelsLeft)
        {
            if(levelsLeft == 0)
            {
                //lazy load
                if (_nodeObjects == null)
                {
                    _nodeObjects = new List<T>();
                }

                _nodeObjects.Add(obj);
            }
            else
            {
                bool placeFound = false;
                for(int i=0; i < NUM_CHILDREN && !placeFound; i++)
                {
                    OctCube childCube = GetChildCube(cube, i);
                    placeFound = IsBoxInsideTile(objBox, childCube);

                    if (placeFound)
                    {
                        //we first need to check if we have childs
                        if (!this.HasChildren)
                        {
                            _subtrees = new OctreeNode<T>[NUM_CHILDREN];
                        }

                        //we than have to ckeck if that specific child already exists
                        if (_subtrees[i] == null)
                        {
                            _subtrees[i] = new OctreeNode<T>();
                        }

                        //we can then proceed to recursively add the item subtracting one level of max depth
                        _subtrees[i].Add(obj, objBox, childCube, levelsLeft - 1);
                    } 
                }

                //if at the end of the loop we didn't find a subtile we add the item to this tile as it spans multiple children
                if (!placeFound)
                {                 
                    //lazy load
                    if (_nodeObjects == null)
                    {
                        _nodeObjects = new List<T>();
                    }

                    _nodeObjects.Add(obj);                    
                }                
            }
        }

        private void AppendNodeItemsHelper(List<T> listBuilder)
        {
            //we add the node items (if any)
            listBuilder.AddRange(NodeOnlyItems);

            //and we recursively append the children's nodes
            if (HasChildren)
            {
                for (int i = 0; i < NUM_CHILDREN; i++)
                {
                    if (_subtrees[i] != null)
                    {
                        _subtrees[i].AppendNodeItemsHelper(listBuilder);
                    }
                }
            }
        }

        private static bool IsBoxInsideTile(AABBox box, OctCube octCube)
        {            
            return (box.MinX >= octCube.MinX && box.MaxX <= octCube.MaxX && 
                    box.MinY >= octCube.MinY && box.MaxY <= octCube.MaxY &&
                    box.MinZ >= octCube.MinZ && box.MaxZ <= octCube.MaxZ);
        }

        public static OctCube GetChildCube(OctCube parentCube, int childrenNr)
        {
            float childSize = parentCube.HalfSize * 0.5f;
            
            //clockwise order starting from top-top-left = 0 ending to bottom-bottom-left=7
            switch (childrenNr)
            {
                //positive Z convention
                case 0:
                    return new OctCube(parentCube.CenterX - childSize, parentCube.CenterY + childSize, parentCube.CenterZ + childSize, childSize);
                case 1:
                    return new OctCube(parentCube.CenterX + childSize, parentCube.CenterY + childSize, parentCube.CenterZ + childSize, childSize);
                case 2:
                    return new OctCube(parentCube.CenterX + childSize, parentCube.CenterY + childSize, parentCube.CenterZ - childSize, childSize);
                case 3:
                    return new OctCube(parentCube.CenterX - childSize, parentCube.CenterY + childSize, parentCube.CenterZ - childSize, childSize);
                case 4:
                    return new OctCube(parentCube.CenterX - childSize, parentCube.CenterY - childSize, parentCube.CenterZ + childSize, childSize);
                case 5:
                    return new OctCube(parentCube.CenterX + childSize, parentCube.CenterY - childSize, parentCube.CenterZ + childSize, childSize);
                case 6:
                    return new OctCube(parentCube.CenterX + childSize, parentCube.CenterY - childSize, parentCube.CenterZ - childSize, childSize);
                case 7:
                    return new OctCube(parentCube.CenterX - childSize, parentCube.CenterY - childSize, parentCube.CenterZ - childSize, childSize);
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }
    }


    public readonly struct OctCube 
    {        
        private readonly float _centerX;
        private readonly float _centerY;
        private readonly float _centerZ;
        private readonly float _halfSize;

        public float CenterX => _centerX;
        public float CenterY => _centerY;
        public float CenterZ => _centerZ;
        public float HalfSize => _halfSize;

        public float MinX => (_centerX - _halfSize);
        public float MaxX => (_centerX + _halfSize);
        public float MinY => (_centerY - _halfSize);
        public float MaxY => (_centerY + _halfSize);
        public float MinZ => (_centerZ - _halfSize);
        public float MaxZ => (_centerZ + _halfSize);

        public OctCube(float centerX, float centerY, float centerZ, float halfSize)
        {
            _centerX = centerX;
            _centerY = centerY;
            _centerZ = centerZ;
            _halfSize = halfSize;
        }
    }
}
