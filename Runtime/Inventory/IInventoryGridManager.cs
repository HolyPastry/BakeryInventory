using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bakery
{
    public interface IInventoryGridManager
    {

        CustomYieldInstruction WaitUntilReady { get; }

        bool Place(GridInfo inventory, RotatableGrid item);
        void Place(GridInfo inventory, List<RotatableGrid> inventoryItems);


        bool Create(GridInfo inventoryInfo,
                    GridInfo inventoryItems,
                    int amount,
                    bool stackable);
        bool Remove(RotatableGrid item, GridInfo inventory);
        bool Remove(RotatableGrid item);
        bool Remove(GridInfo inventory, GridInfo item, int amount);
        bool IsItemIn(GridInfo inventory, RotatableGrid item);
        IEnumerable<RotatableGrid> GetAllItems(GridInfo inventory);
        IEnumerable<RotatableGrid> GetItems(GridInfo inventory, Predicate<RotatableGrid> predicate);
        bool TryGetObjectAt(GridInfo gridInfo, Vector2Int position, out RotatableGrid gridObject);
        void PickUp(RotatableGrid hoveredObject, int numToGrab, out RotatableGrid numGrabbed);
        bool TryPlaceAt(RotatableGrid grabbedObject, GridInfo gridInfo, Vector2Int gridCoordinates, int numToRelease, out int numReleased);
    }
}
