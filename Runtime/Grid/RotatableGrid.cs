using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bakery
{
    [Serializable]
    public class RotatableGrid : GridBase
    {
        public int Rotation; // number of 90Degree Rotations ClockWise (0, 1, 2, 3)


        public RotatableGrid(GridInfo gridInfo)
        {
            GridInfo = gridInfo;
        }

        public RotatableGrid(RotatableGrid grabbedObject)
        {
            GridInfo = grabbedObject.GridInfo;
            RootPosition = grabbedObject.RootPosition;
            Rotation = grabbedObject.Rotation;
        }

        public override IEnumerable<Vector2Int> LocalPositions
        {
            get
            {
                List<Vector2Int> rotatedPositions = new();
                foreach (var localPos in GridInfo.Coordinates)
                    rotatedPositions.Add(RotatePosition(localPos, Rotation));
                return rotatedPositions;
            }
        }


        public override IEnumerable<Vector2Int> WorldPositions
        {
            get
            {
                _worldPositionsCache.Clear();
                foreach (var localPos in GridInfo.Coordinates)
                {
                    Vector2Int rotatedPos = RotatePosition(localPos, Rotation);
                    Vector2Int worldPos = RootPosition + rotatedPos;
                    _worldPositionsCache.Add(worldPos);
                }
                return _worldPositionsCache;
            }
        }

        public bool Grabbed
        {
            get => _grabbed;
            set
            {
                _grabbed = value;
                RootPosition = _grabbed ? Vector2Int.zero : RootPosition;
            }
        }



        private bool _grabbed;

        private Vector2Int RotatePosition(Vector2Int localPos, int rotation)
        {

            return rotation switch
            {
                // 0 degrees
                0 => localPos,
                // 90 degrees clockwise
                1 => new Vector2Int(localPos.y, -localPos.x),
                // 180 degrees
                2 => new Vector2Int(-localPos.x, -localPos.y),
                // 270 degrees clockwise (or 90 degrees counter-clockwise)
                3 => new Vector2Int(-localPos.y, localPos.x),
                _ => throw new ArgumentException("Rotation must be between 0 and 3"),
            };
        }

        // private Vector2Int GetPivot()
        // {
        //     // Assuming the pivot is the center of the grid
        //     int minX = GridInfo.Coordinates.Min(pos => pos.x);
        //     int maxX = GridInfo.Coordinates.Max(pos => pos.x);
        //     int minY = GridInfo.Coordinates.Min(pos => pos.y);
        //     int maxY = GridInfo.Coordinates.Max(pos => pos.y);

        //     return new Vector2Int(maxX, maxY > 1 ? maxY : 1);
        // }

        public bool Overlaps(RotatableGrid other)
        {
            foreach (var pos in WorldPositions)
                if (other.WorldPositions.Any(p => p == pos))
                    return true;
            return false;
        }


        public void RotateClockwise()
        {
            Rotation = (Rotation + 1) % 4;
        }
        public void RotateCounterClockwise()
        {
            Rotation = (Rotation + 3) % 4; // Adding 3 is equivalent to subtracting 1 in modulo 4
        }

        internal void Rotate()
        {
            RotateClockwise();
        }
    }

}