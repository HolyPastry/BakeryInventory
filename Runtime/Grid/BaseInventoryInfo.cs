using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bakery
{
    public abstract class BaseInventoryInfo : ScriptableObject
    {
        public List<InventoryFilter> Filters;
        public List<Vector2Int> Coordinates = new();
        public Vector2Int MaxSize;

        internal bool Compatible(GridInfo gridInfo)
        {
            if (gridInfo == null) return false;
            if (Filters.Count == 0 || gridInfo.Filters.Count == 0)
                return true;
            return Filters.Any(f => gridInfo.Filters.Contains(f));
        }
    }

}