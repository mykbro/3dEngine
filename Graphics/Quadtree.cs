using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace _3dGraphics.Graphics
{
    public class Quadtree<T>
    {
        private readonly QuadtreeNode<T> _lvlZeroNode;
        private readonly float _size;           //width/length of the lvl0 node
        private readonly int _maxDepth;         //lowest level we want to reach (starts from 1)
        private readonly QuadTile _lvlZeroTile; //precomputed from size        

        public List<T> Items => _lvlZeroNode.AllItems;
        public QuadTile RootTile => _lvlZeroTile;
        public QuadtreeNode<T> RootNode => _lvlZeroNode;
        
        public Quadtree(int size, int maxDepth)
        {
            _lvlZeroNode = new QuadtreeNode<T>();            
            _maxDepth = maxDepth;
            _size = size;

            int halfSize = size / 2;
            _lvlZeroTile = new QuadTile(0, 0, halfSize);
        }

        public void Add(T obj, AABBox objBox)
        {
            _lvlZeroNode.Add(obj, objBox, _lvlZeroTile, _maxDepth - 1);
        }       
    }

    public class QuadtreeNode<T>
    {
        private List<T> _nodeObjects;
        private QuadtreeNode<T>[] _subtrees;        
        private const int NUM_CHILDREN = 4;

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

        public QuadtreeNode()
        {
            //we do not instance our structures immediately but on demand for the first item
            _nodeObjects = null;              
            _subtrees = null;
        }

        public QuadtreeNode<T>? GetChildren(int i)
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

        public void Add(T obj, AABBox objBox, QuadTile tile, int levelsLeft)
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
                    QuadTile childTile = GetChildTile(tile, i);
                    placeFound = IsBoxInsideTile(objBox, childTile);

                    if (placeFound)
                    {
                        //we first need to check if we have childs
                        if (!this.HasChildren)
                        {
                            _subtrees = new QuadtreeNode<T>[NUM_CHILDREN];
                        }

                        //we than have to ckeck if that specific child already exists
                        if (_subtrees[i] == null)
                        {
                            _subtrees[i] = new QuadtreeNode<T>();
                        }

                        //we can then proceed to recursively add the item subtracting one level of max depth
                        _subtrees[i].Add(obj, objBox, childTile, levelsLeft - 1);
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

        private static bool IsBoxInsideTile(AABBox box, QuadTile tile)
        {
            //we need to check the Z component of the box with the Y of the tile (box footprint)
            return (box.MinX >= tile.MinX && box.MaxX <= tile.MaxX && box.MinZ >= tile.MinY && box.MaxZ <= tile.MaxY);
        }

        public static QuadTile GetChildTile(QuadTile parentTile, int childrenNr)
        {
            int quarterSize = parentTile.HalfSize / 2;
            
            //clockwise order starting from top-left=0 ending to bottom-left=3
            switch (childrenNr)
            {
                case 0:
                    return new QuadTile(parentTile.CenterX - quarterSize, parentTile.CenterY + quarterSize, quarterSize);
                case 1:
                    return new QuadTile(parentTile.CenterX + quarterSize, parentTile.CenterY + quarterSize, quarterSize);
                case 2:
                    return new QuadTile(parentTile.CenterX + quarterSize, parentTile.CenterY - quarterSize, quarterSize);
                case 3:
                    return new QuadTile(parentTile.CenterX - quarterSize, parentTile.CenterY - quarterSize, quarterSize);
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }
    }


    public readonly struct QuadTile 
    {
        //we want to use integers for fast division by 2
        private readonly int _centerX;
        private readonly int _centerY;
        private readonly int _halfSize;

        public int CenterX => _centerX;
        public int CenterY => _centerY;
        public int HalfSize => _halfSize;

        public int MinX => (_centerX - _halfSize);
        public int MaxX => (_centerX + _halfSize);
        public int MinY => (_centerY - _halfSize);
        public int MaxY => (_centerY + _halfSize);

        public QuadTile(int centerX, int centerY, int halfSize)
        {
            _centerX = centerX;
            _centerY = centerY;
            _halfSize = halfSize;
        }
    }
}
