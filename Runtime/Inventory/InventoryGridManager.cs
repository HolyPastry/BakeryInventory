using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                return true;
            }
            return false;
        }

        public void Place(GridInfo inventoryInfo, List<RotatableGrid> inventoryItems)
        {
            var inventory = GetInventory(inventoryInfo);
            foreach (var item in inventoryItems)
                inventory.Add(item);
        }

        public void Place(RotatableGrid grabbedObject, GridInfo gridInfo, Vector2Int gridCoordinates)
        {
            GetInventory(gridInfo).Place(grabbedObject, gridCoordinates);
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

        public bool Remove(GridInfo inventory, GridInfo item, int amount)
        {
            var container = _containers.FirstOrDefault(c => c.GridInfo = inventory);
            if (container == null)
            {
                Debug.LogWarning($"Container is not found: {inventory}");
                return false;
            }
            return container.Remove(item, amount);



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
            if (!inventoryInfo.Compatible(inventoryItem))
            {
                Debug.LogWarning($"Trying to add an incompatible item {inventoryItem} to inventory  {inventoryInfo}.");
                return false;
            }
            if (TryStacking(inventoryInfo, inventoryItem))
                return true;

            var gridObject = new RotatableGrid(inventoryItem);
            if (!Place(inventoryInfo, gridObject))
                return false;
            return true;
        }

        public bool Create(GridInfo inventoryInfo, GridInfo inventoryItems, int amount)
        {
            bool success = true;
            for (int i = 0; i < amount; i++)
            {
                success &= Create(inventoryInfo, inventoryItems);
            }
            return success;
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
                    Inventory.Events.Grids.OnItemStackModified?.Invoke(item, 1);
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

        public void PickUp(RotatableGrid hoveredObject,
                            int numToGrab,
                            out RotatableGrid pickedUpGrid)
        {
            var inventory = _containers.Find(i => i.Grids.Contains(hoveredObject));
            if (inventory == null)
            {
                pickedUpGrid = null;
                Debug.LogWarning($"Could not find inventory holding {hoveredObject.GridInfo.name}");
                return;
            }
            inventory.PickUp(hoveredObject, numToGrab, out pickedUpGrid);
        }

        public bool TryPlaceAt(RotatableGrid grabbedObject, GridInfo gridInfo, Vector2Int gridCoordinates, int numToRelease, out int numReleased)
        {
            var inventory = GetInventory(gridInfo);
            if (inventory == null)
            {
                numReleased = 0;
                return false;
            }

            return inventory.TryPlaceAt(grabbedObject,
                                    gridCoordinates,
                                    numToRelease,
                                    out numReleased);
        }


    }

}