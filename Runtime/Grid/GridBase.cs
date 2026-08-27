using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bakery
{
    [Serializable]
    public class GridBase
    {
        public GridInfo GridInfo;
        public Vector2Int RootPosition;



        protected List<Vector2Int> _worldPositionsCache = new();

        public virtual IEnumerable<Vector2Int> LocalPositions => GridInfo.Coordinates;

        public virtual IEnumerable<Vector2Int> WorldPositions
        {
            get
            {
                _worldPositionsCache.Clear();
                foreach (var localPos in GridInfo.Coordinates)
                {
                    Vector2Int worldPos = RootPosition + localPos;
                    _worldPositionsCache.Add(worldPos);
                }
                return _worldPositionsCache;
            }
        }
        public bool OverlapsWith(GridBase other)
        {
            foreach (var pos in WorldPositions)
            {
                if (other.WorldPositions.Contains(pos))
                    return true;
            }
            return false;
        }

    }
}