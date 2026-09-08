using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bakery
{
    public interface IInventoryGridManager
    {

        CustomYieldInstruction WaitUntilReady { get; }

        bool Place(ContainerInfo inventory, RotatableGrid item);
        void Place(ContainerInfo inventory, List<RotatableGrid> inventoryItems);


        bool Create(ContainerInfo inventoryInfo,
                    GridInfo inventoryItems,
                    int amount,
                    bool stackable);
        bool Remove(RotatableGrid item, ContainerInfo inventory);
        bool Remove(RotatableGrid item);
        bool Remove(ContainerInfo inventory, GridInfo item, int amount);
        bool IsItemIn(ContainerInfo inventory, RotatableGrid item);
        IEnumerable<RotatableGrid> GetAllItems(ContainerInfo inventory);
        IEnumerable<RotatableGrid> GetItems(ContainerInfo inventory, Predicate<RotatableGrid> predicate);
        bool TryGetObjectAt(ContainerInfo gridInfo, Vector2Int position, out RotatableGrid gridObject);
        void PickUp(RotatableGrid hoveredObject, int numToGrab, out RotatableGrid numGrabbed);
        bool TryPlaceAt(RotatableGrid grabbedObject, ContainerInfo gridInfo, Vector2Int gridCoordinates, int numToRelease, out int numReleased);

        bool IsEmpty(ContainerInfo containerInfo);
    }
}
