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
            {
                if (serialInventory.Add(item))
                    Inventory.Events.Grids.OnItemAdded?.Invoke(serialInventory, item);
            }
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

        public bool Spawn(GridInfo inventoryInfo, List<GridInfo> inventoryItems)
        {
            foreach (var item in inventoryItems)
            {
                if (!Spawn(inventoryInfo, item))
                    return false;
            }
            return true;
        }

        public bool Spawn(GridInfo inventoryInfo, GridInfo inventoryItems)
        {
            var gridObject = new RotatableGrid(inventoryItems);
            if (!Place(inventoryInfo, gridObject))
                return false;
            Inventory.Events.Grids.OnItemSpawned?.Invoke(GetInventory(inventoryInfo), gridObject);
            return true;
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

        public bool CanPlace(RotatableGrid grabbedObject, GridInfo inventory, Vector2Int gridCoordinates)
        {
            var objectCopy = new RotatableGrid(grabbedObject);
            return GetInventory(inventory).FitIn(objectCopy, gridCoordinates, objectCopy.Rotation);
        }


    }

}