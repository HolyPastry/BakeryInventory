using System;
using System.Collections.Generic;
using System.Linq;
using Bakery.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bakery
{

    [Serializable]
    public class GridContainer : GridBase
    {
        public readonly List<RotatableGrid> Grids = new();

        public bool Remove(RotatableGrid grid)
                => Grids.Remove(grid);

        public bool Contains(RotatableGrid grid)
                => Grids.Contains(grid);

        public bool Add(RotatableGrid grid)
        {
            if (FitIn(grid))
            {
                Grids.AddUnique(grid);
                return true;
            }
            return false;
        }
        internal bool Place(RotatableGrid grabbedObject, Vector2Int gridCoordinates)
        {
            if (!FitIn(grabbedObject, gridCoordinates, grabbedObject.Rotation))
                return false;
            Grids.AddUnique(grabbedObject);
            return true;
        }

        public bool FitIn(RotatableGrid grid)
        {
            foreach (var coordinate in GridInfo.Coordinates)
                if (FitIn(grid, coordinate))
                    return true;
            return false;
        }

        public bool FitIn(RotatableGrid grid, Vector2Int coordinate, int rotation = 0)
        {

            grid.RootPosition = coordinate;
            grid.Rotation = rotation;

            return !IsOutsideGrid(grid) && OverlapsExisting(grid);
        }

        public bool CanStack(RotatableGrid grid,
                        Vector2Int coordinate)
        {
            foreach (var otherItem in Grids)
            {
                if (otherItem.WorldPositions.Any(p => p == coordinate) &&
                     otherItem.CanStackWith(grid))
                {
                    return true;
                }
            }
            return false;
        }

        public bool FitIn(RotatableGrid grid, Vector2Int coordinate)
        {
            grid.RootPosition = coordinate;

            for (int rotation = 0; rotation < 4; rotation++)
            {
                if (FitIn(grid, rotation))
                    return true;
            }
            return false;
        }

        public bool FitIn(RotatableGrid grid, int rotation)
        {
            grid.Rotation = rotation;

            return !IsOutsideGrid(grid) && OverlapsExisting(grid);
        }

        public bool FitIn(RotatableGrid grid, int rotation, Vector2Int coordinate)
        {
            grid.Rotation = rotation;
            grid.RootPosition = coordinate;
            var placed = !IsOutsideGrid(grid) && OverlapsExisting(grid);
            if (placed)
                Inventory.Events.Grids.OnItemPlaced?.Invoke(this, grid);
            return placed;
        }

        private bool OverlapsExisting(RotatableGrid grid)
        {
            var overlap = false;
            foreach (var otherItem in Grids)
            {
                if (otherItem.Overlaps(grid))
                {
                    overlap = true;
                    break;
                }
            }
            return !overlap;
        }

        private bool IsOutsideGrid(RotatableGrid grid)
        {
            foreach (var pos in grid.WorldPositions)
            {
                if (!GridInfo.Coordinates.Exists(p => p == pos))
                    return true;
            }
            return false;
        }

        internal bool IsItemIn(RotatableGrid grid)
        {
            return Grids.Contains(grid);
        }

        internal bool TryGetObjectAt(Vector2Int position, out RotatableGrid gridObject)
        {
            gridObject = Grids.Find(grid => grid.WorldPositions.Any(p => p == position));
            return gridObject != null;
        }

        internal int StackItem(RotatableGrid grabbedObject, Vector2Int gridCoordinates)
        {
            var otherItem = Grids.Find(item => item.WorldPositions.Any(p => p == gridCoordinates));
            if (otherItem == null || !otherItem.CanStackWith(grabbedObject))
            {
                Debug.LogWarning("No stackable item found at the specified coordinates.");
                return grabbedObject.Stack; // Return the original amount since no stacking occurred
            }

            int availableSpace = otherItem.GridInfo.StackCapacity - otherItem.Stack;
            int stackAmount = Math.Min(availableSpace, grabbedObject.Stack);

            otherItem.Stack += stackAmount;
            grabbedObject.Stack -= stackAmount;

            Inventory.Events.Grids.OnItemStacked?.Invoke(this, otherItem);

            return grabbedObject.Stack;
        }

        internal bool TryPlaceAt(RotatableGrid grabbedObject, Vector2Int gridCoordinates, int numToRelease, out int numReleased)
        {
            //TODO:: Logic to place at specific coordinates, considering stacking and available space
            numReleased = 0;
            return false;
        }

        internal void PickUp(RotatableGrid hoveredObject, int numToGrab, out int numGrabbed)
        {
            numGrabbed = 0;
            //TODO:: Logic to pick up from multiple stacks of the same item if needed
        }
    }
}