using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bakery
{
    [Serializable]
    public class GridBase
    {
        [NonSerialized]
        public GridInfo GridInfo;

        [HideInInspector]
        public string GrindInfoName;
        public Vector2Int RootPosition;

        public virtual IEnumerable<Vector2Int> LocalPositions => GridInfo.Coordinates;

        public virtual IEnumerable<Vector2Int> WorldPositions
        {
            get
            {
                List<Vector2Int> worldPositions = new();
                foreach (var localPos in GridInfo.Coordinates)
                {
                    Vector2Int worldPos = RootPosition + localPos;
                    worldPositions.Add(worldPos);
                }
                return worldPositions;
            }
        }

        public bool Overlaps(RotatableGrid other)
        {
            foreach (var pos in WorldPositions)
                if (other.WorldPositions.Any(p => p == pos))
                    return true;
            return false;
        }

    }
}