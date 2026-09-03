using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bakery
{
    [CreateAssetMenu(fileName = "New Grid Info", menuName = "Bakery/Inventory/Grid Info")]
    public class GridInfo : ScriptableObject
    {
        public Sprite Sprite;
        public List<InventoryFilter> Filters;
        public List<Vector2Int> Coordinates = new();
        public Vector2Int MaxSize;
        public int StackCapacity;
        public bool Lock;

        public Vector2Int Size
        {
            get
            {
                if (Coordinates.Count == 0)
                    return Vector2Int.zero;

                var minX = Coordinates.Min(c => c.x);
                var maxX = Coordinates.Max(c => c.x);
                var minY = Coordinates.Min(c => c.y);
                var maxY = Coordinates.Max(c => c.y);

                return new Vector2Int(maxX - minX + 1, maxY - minY + 1);
            }
        }



        internal bool Compatible(GridInfo gridInfo)
        {
            if (Filters.Count == 0 || gridInfo.Filters.Count == 0)
                return true;
            return Filters.Any(f => gridInfo.Filters.Contains(f));
        }
    }



}