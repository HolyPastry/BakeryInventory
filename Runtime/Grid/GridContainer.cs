using System;
using System.Collections.Generic;
using System.Linq;
using Bakery.Core;
using Bakery.Saves;
using UnityEngine;

namespace Bakery
{

    [Serializable]
    public class GridContainer : SerialData
    {

        public ContainerInfo ContainerInfo { get; set; }
        public int Count => Grids.Count;

        public List<RotatableGrid> Grids = new();

        private void Load()
        {


        }

        public bool Remove(RotatableGrid grid)
        {
            bool removed = Grids.Remove(grid);
            if (removed)
            {
                Inventory.Events.Grids.OnItemRemoved?.Invoke(this, grid);
                Save();
            }

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
                Save();
                return true;
            }
            return false;
        }

        public void Deserialize(DataCollection<GridInfo> collection)
        {
            foreach (var grid in Grids)
            {
                grid.GridInfo = collection.GetFromName(grid.GrindInfoName);
                if (grid.GridInfo == null)
                    Debug.LogWarning($"Saved Grid couldn't be loaded:{grid.GrindInfoName}");
            }
        }
        public override void Serialize()
        {
            foreach (var grid in Grids)
            {
                grid.GrindInfoName = grid.GridInfo.name;
            }
        }

        public void Save()
        {
            if (!ContainerInfo.IsPersistent) return;
            SaveServices.Save(ContainerInfo.name, this);
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
            if (numToRelease != -1 && numToRelease < grabbedObject.Amount)
            {
                RotatableGrid copy = new(grabbedObject)
                {
                    Amount = numToRelease
                };
                grabbedObject.Amount -= numToRelease;
                numReleased = numToRelease;
                Grids.AddUnique(copy);
                Inventory.Events.Grids.OnItemAdded?.Invoke(this, copy);
                Save();
                return true;
            }
            numReleased = grabbedObject.Amount;
            Grids.AddUnique(grabbedObject);
            Inventory.Events.Grids.OnItemAdded?.Invoke(this, grabbedObject);
            Save();
            return true;
        }

        public bool FitIn(RotatableGrid grid)
        {
            foreach (var coordinate in ContainerInfo.Coordinates)
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
                if (!ContainerInfo.Coordinates.Exists(p => p == pos))
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
                numToStack = objectToStack.Amount;
            var otherItem = Grids.Find(item => item.WorldPositions.Any(p => p == gridCoordinates));
            if (otherItem == null || !otherItem.CanStackWith(objectToStack))
            {
                Debug.LogWarning("No stackable item found at the specified coordinates.");
                return objectToStack.Amount; // Return the original amount since no stacking occurred
            }

            int availableSpace = otherItem.GridInfo.StackCapacity - otherItem.Amount;
            int stackAmount = Math.Min(availableSpace, numToStack);

            otherItem.Amount += stackAmount;
            objectToStack.Amount -= stackAmount;

            Inventory.Events.Grids.OnItemStackModified?.Invoke(otherItem, stackAmount);
            Save();
            return objectToStack.Amount;
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
                var stackBeforeStacking = grabbedObject.Amount;
                var remainingStack = StackItem(grabbedObject, gridCoordinates, numToRelease);
                if (remainingStack <= 0)
                {
                    numReleased = numToRelease;
                    return true;
                }
                numReleased = stackBeforeStacking - remainingStack;
                Save();
                return true;
            }

            if (!Place(grabbedObject, gridCoordinates, numToRelease, out numReleased))
            {
                numReleased = 0;
                return false;
            }
            Save();
            return true;

        }

        private bool Compatible(RotatableGrid grabbedObject)
        {
            return ContainerInfo.Compatible(grabbedObject.GridInfo);
        }

        internal void PickUp(RotatableGrid hoveredObject,
                                int numToGrab,
                                out RotatableGrid pickedUpGrid)
        {
            pickedUpGrid = null;
            if (numToGrab <= 0)
                return;

            if (hoveredObject.Amount > numToGrab)
            {
                hoveredObject.Amount -= numToGrab;
                pickedUpGrid = new RotatableGrid(hoveredObject)
                { Amount = numToGrab };
                Inventory.Events.Grids.OnItemStackModified(hoveredObject, hoveredObject.Amount);
                Save();
                return;
            }

            if (hoveredObject.Amount <= numToGrab)
            {
                Remove(hoveredObject);
                pickedUpGrid = hoveredObject;

                return;
            }
        }

        internal bool Remove(GridInfo item, int amount)
        {
            var matchingGrids = Grids.FindAll(g => g.GridInfo == item);
            if (matchingGrids.Count == 0) return false;
            foreach (var grid in matchingGrids)
            {
                if (grid.Amount > amount)
                {
                    grid.Amount -= amount;
                    Inventory.Events.Grids.OnItemStackModified?.Invoke(grid, amount);
                    return true;
                }
                if (grid.Amount == amount)
                {
                    Remove(grid);
                    return true;
                }
                if (grid.Amount < amount)
                {
                    amount -= grid.Amount;
                    Remove(grid);
                }
            }
            return amount == 0;
        }
    }
}