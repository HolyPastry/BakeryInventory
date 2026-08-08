using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Bakery
{

    public class InventoryGridManager : MonoBehaviour, IInventoryGridManager
    {
        private readonly List<GridContainer> _containers = new();

        public CustomYieldInstruction WaitUntilReady => new WaitUntil(() => _isReady);

        private bool _isReady = false;

        void OnEnable()
        {
            Inventory.Grids = () => this;
        }

        void OnDisable()
        {
            Inventory.Grids = Inventory.UnregisterManager;
        }

        IEnumerator Start()
        {
            yield return FlowServices.WaitUntilReady();
            _isReady = true;
        }

        public bool Place(GridInfo inventory, RotatableGrid item)
        {
            if (GetInventory(inventory).Add(item))
            {
                Inventory.Events.Grids.OnItemAdded?.Invoke(GetInventory(inventory), item);
                return true;
            }
            return false;
        }

        public void Place(GridInfo inventoryInfo, List<RotatableGrid> inventoryItems)
        {
            var serialInventory = GetInventory(inventoryInfo);
            foreach (var item in inventoryItems)
                if (serialInventory.Add(item))
                    Inventory.Events.Grids.OnItemAdded?.Invoke(serialInventory, item);
        }

        public void Place(RotatableGrid grabbedObject, GridInfo gridInfo, Vector2Int gridCoordinates)
        {
            GetInventory(gridInfo).Place(grabbedObject, gridCoordinates);
            Inventory.Events.Grids.OnItemPlaced?.Invoke(GetInventory(gridInfo), grabbedObject);
        }

        private GridContainer GetInventory(GridInfo inventory)
        {
            var serialInventory = _containers.Find(i => i.GridInfo == inventory);
            if (serialInventory == null)
            {
                serialInventory = new GridContainer { GridInfo = inventory };
                _containers.Add(serialInventory);
            }
            return serialInventory;
        }

        public IEnumerable<RotatableGrid> GetAllItems(GridInfo inventory)
        {
            return GetInventory(inventory).Grids;
        }

        public IEnumerable<RotatableGrid> GetItems(GridInfo inventory, Predicate<RotatableGrid> predicate)
        {
            return GetInventory(inventory).Grids.FindAll(predicate);
        }

        public bool IsItemIn(GridInfo inventory, RotatableGrid item)
        {
            return GetInventory(inventory).IsItemIn(item);
        }

        public bool Remove(RotatableGrid item, GridInfo inventory)
        {
            if (GetInventory(inventory).Remove(item))
            {
                Inventory.Events.Grids.OnItemRemoved?.Invoke(GetInventory(inventory), item);
                return true;
            }
            return false;
        }
        public bool Remove(RotatableGrid item)
        {
            foreach (var container in _containers)
                if (Remove(item, container.GridInfo))
                    return true;
            return false;
        }

        public bool Create(GridInfo inventoryInfo, List<GridInfo> inventoryItems)
        {
            foreach (var item in inventoryItems)
            {
                if (!Create(inventoryInfo, item))
                    return false;
            }
            return true;
        }

        public bool Create(GridInfo inventoryInfo, GridInfo inventoryItem)
        {
            if (TryStacking(inventoryInfo, inventoryItem))
                return true;

            var gridObject = new RotatableGrid(inventoryItem);
            if (!Place(inventoryInfo, gridObject))
                return false;
            Inventory.Events.Grids.OnItemSpawned?.Invoke(GetInventory(inventoryInfo), gridObject);
            return true;
        }

        private bool TryStacking(GridInfo inventoryInfo, GridInfo inventoryItem)
        {
            var serialInventory = GetInventory(inventoryInfo);
            foreach (var item in serialInventory.Grids)
            {
                if (item.GridInfo == inventoryItem &&
                    item.Stack < item.GridInfo.StackCapacity)
                {
                    item.Stack++;
                    Inventory.Events.Grids.OnItemStacked?.Invoke(serialInventory, item);
                    return true;
                }
            }
            return false;
        }

        public bool TryGetObjectAt(GridInfo gridInfo, Vector2Int position, out RotatableGrid gridObject)
        {
            return GetInventory(gridInfo).TryGetObjectAt(position, out gridObject);
        }

        public bool CanPlace(RotatableGrid grabbedObject, out GridInfo inventory)
        {
            var objectCopy = new RotatableGrid(grabbedObject);
            foreach (var container in _containers)
            {
                if (container.FitIn(objectCopy, grabbedObject.RootPosition))
                {
                    inventory = container.GridInfo;
                    return true;
                }
            }
            inventory = null;
            return false;
        }

        public bool CanPlace(RotatableGrid grabbedObject, GridInfo inventoryId, Vector2Int gridCoordinates)
        {
            var objectCopy = new RotatableGrid(grabbedObject);
            var inventory = GetInventory(inventoryId);
            if (inventory.CanStack(objectCopy, gridCoordinates))
                return true;

            return inventory.FitIn(objectCopy, gridCoordinates, objectCopy.Rotation);
        }

        public void PickUp(RotatableGrid hoveredObject, int numToGrab, out int numGrabbed)
        {
            var inventory = _containers.Find(i => i.Grids.Contains(hoveredObject));
            if (inventory == null)
            {
                numGrabbed = 0;
                Debug.LogWarning($"Could not find inventory holding {hoveredObject.GridInfo.name}");
                return;
            }
            inventory.PickUp(hoveredObject, numToGrab, out numGrabbed);
        }

        public bool TryPlaceAt(RotatableGrid grabbedObject, GridInfo gridInfo, Vector2Int gridCoordinates, int numToRelease, out int numReleased)
        {
            var inventory = GetInventory(gridInfo);

            if (inventory.TryPlaceAt(grabbedObject, gridCoordinates, numToRelease, out numReleased))
            {
                Inventory.Events.Grids.OnItemPlaced?.Invoke(inventory, grabbedObject);
                return true;
            }
            return false;
        }
    }

}