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

        bool Create(GridInfo inventoryInfo, List<GridInfo> inventoryItems);
        bool Create(GridInfo inventoryInfo, GridInfo inventoryItems);
        bool Remove(RotatableGrid item, GridInfo inventory);
        bool Remove(RotatableGrid item);
        bool IsItemIn(GridInfo inventory, RotatableGrid item);
        IEnumerable<RotatableGrid> GetAllItems(GridInfo inventory);
        IEnumerable<RotatableGrid> GetItems(GridInfo inventory, Predicate<RotatableGrid> predicate);
        bool TryGetObjectAt(GridInfo gridInfo, Vector2Int position, out RotatableGrid gridObject);

        // bool CanPlace(RotatableGrid grabbedObject, GridInfo inventory, Vector2Int gridCoordinates);
        //void Place(RotatableGrid grabbedObject, GridInfo gridInfo, Vector2Int gridCoordinates);
        //void ModifyStack(RotatableGrid hoveredObject, int amount);
        //bool CanStack(RotatableGrid grabbedObject, GridInfo gridInfo, Vector2Int gridCoordinates);
        // int Stack(RotatableGrid grabbedObject, GridInfo gridInfo, Vector2Int gridCoordinates);
        void PickUp(RotatableGrid hoveredObject, int numToGrab, out RotatableGrid numGrabbed);
        bool TryPlaceAt(RotatableGrid grabbedObject, GridInfo gridInfo, Vector2Int gridCoordinates, int numToRelease, out int numReleased);
    }
}
