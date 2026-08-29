using System;
using System.Collections.Generic;
using System.Linq;
using Bakery.Core;
using UnityEngine;

namespace Bakery
{

    [Serializable]
    public class GridContainer : GridBase
    {
        public readonly List<RotatableGrid> Grids = new();

        public bool Remove(RotatableGrid grid)
        {
            bool removed = Grids.Remove(grid);
            if (removed)
                Inventory.Events.Grids.OnItemRemoved?.Invoke(this, grid);

            return removed;
        }

        public bool Contains(RotatableGrid grid)
                => Grids.Contains(grid);

        public bool Add(RotatableGrid grid)
        {
            if (!Compatible(grid))
                return false;
            if (FitIn(grid))
            {
                Grids.AddUnique(grid);
                Inventory.Events.Grids.OnItemAdded?.Invoke(this, grid);
                return true;
            }
            return false;
        }

        internal bool Place(RotatableGrid grabbedObject,
                        Vector2Int gridCoordinates)
            => Place(grabbedObject, gridCoordinates, -1, out _);

        internal bool Place(RotatableGrid grabbedObject,
                        Vector2Int gridCoordinates,
                        int numToRelease,
                        out int numReleased)
        {
            numReleased = 0;
            if (!FitIn(grabbedObject, gridCoordinates, grabbedObject.Rotation))
                return false;
            if (numToRelease != -1 && numToRelease < grabbedObject.Stack)
            {
                RotatableGrid copy = new(grabbedObject)
                {
                    Stack = numToRelease
                };
                grabbedObject.Stack -= numToRelease;
                numReleased = numToRelease;
                Grids.AddUnique(copy);
                Inventory.Events.Grids.OnItemAdded?.Invoke(this, copy);
                return true;
            }
            numReleased = grabbedObject.Stack;
            Grids.AddUnique(grabbedObject);
            Inventory.Events.Grids.OnItemAdded?.Invoke(this, grabbedObject);
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
            var fitIn = !IsOutsideGrid(grid) && OverlapsExisting(grid);
            return fitIn;
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
            return gridObject != null && !gridObject.Locked;
        }

        internal int StackItem(RotatableGrid objectToStack, Vector2Int gridCoordinates, int numToStack = -1)
        {
            if (numToStack == -1)
                numToStack = objectToStack.Stack;
            var otherItem = Grids.Find(item => item.WorldPositions.Any(p => p == gridCoordinates));
            if (otherItem == null || !otherItem.CanStackWith(objectToStack))
            {
                Debug.LogWarning("No stackable item found at the specified coordinates.");
                return objectToStack.Stack; // Return the original amount since no stacking occurred
            }

            int availableSpace = otherItem.GridInfo.StackCapacity - otherItem.Stack;
            int stackAmount = Math.Min(availableSpace, numToStack);

            otherItem.Stack += stackAmount;
            objectToStack.Stack -= stackAmount;

            Inventory.Events.Grids.OnItemStackModified?.Invoke(otherItem, stackAmount);

            return objectToStack.Stack;
        }

        internal bool TryPlaceAt(RotatableGrid grabbedObject, Vector2Int gridCoordinates, int numToRelease, out int numReleased)
        {
            if (!Compatible(grabbedObject))
            {
                numReleased = 0;
                return false;
            }
            if (CanStack(grabbedObject, gridCoordinates))
            {
                var stackBeforeStacking = grabbedObject.Stack;
                var remainingStack = StackItem(grabbedObject, gridCoordinates, numToRelease);
                if (remainingStack <= 0)
                {
                    numReleased = numToRelease;
                    return true;
                }
                numReleased = stackBeforeStacking - remainingStack;
                return true;
            }

            if (!Place(grabbedObject, gridCoordinates, numToRelease, out numReleased))
            {
                numReleased = 0;
                return false;
            }
            return true;

        }

        private bool Compatible(RotatableGrid grabbedObject)
        {
            return GridInfo.Compatible(grabbedObject.GridInfo);
        }

        internal void PickUp(RotatableGrid hoveredObject,
                                int numToGrab,
                                out RotatableGrid pickedUpGrid)
        {
            pickedUpGrid = null;
            if (numToGrab <= 0)
                return;

            if (hoveredObject.Stack > numToGrab)
            {
                hoveredObject.Stack -= numToGrab;
                pickedUpGrid = new RotatableGrid(hoveredObject) { Stack = numToGrab };
                Inventory.Events.Grids.OnItemStackModified(hoveredObject, hoveredObject.Stack);
                return;
            }

            if (hoveredObject.Stack <= numToGrab)
            {
                Remove(hoveredObject);
                pickedUpGrid = hoveredObject;
                return;
            }
        }
    }
}