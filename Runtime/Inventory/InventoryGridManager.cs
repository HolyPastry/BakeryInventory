using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bakery.Core;
using Bakery.Saves;
using UnityEngine;


namespace Bakery
{

    public class InventoryGridManager : MonoBehaviour, IInventoryGridManager
    {
        [SerializeField] private string _gridInfoFolderName = "InventoryGrids";
        private readonly List<GridContainer> _containers = new();

        public CustomYieldInstruction WaitUntilReady => new WaitUntil(() => _isReady);

        private bool _isReady = false;
        private DataCollection<GridInfo> _gridCollection;

        void Awake()
        {
            _gridCollection = new(_gridInfoFolderName);
        }

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

        public bool IsEmpty(ContainerInfo inventory)
        {
            return GetInventory(inventory).Count > 0;
        }

        public bool Place(ContainerInfo inventory, RotatableGrid item)
        {
            if (GetInventory(inventory).Add(item))
            {
                return true;
            }
            return false;
        }

        public void Place(ContainerInfo inventoryInfo, List<RotatableGrid> inventoryItems)
        {
            var inventory = GetInventory(inventoryInfo);
            foreach (var item in inventoryItems)
                inventory.Add(item);
        }

        public void Place(RotatableGrid grabbedObject, ContainerInfo gridInfo, Vector2Int gridCoordinates)
        {
            GetInventory(gridInfo).Place(grabbedObject, gridCoordinates);
        }

        private GridContainer GetInventory(ContainerInfo inventory)
        {
            var serialInventory = _containers.Find(i => i.ContainerInfo == inventory);
            if (serialInventory == null)
            {
                if (inventory.IsPersistent)
                {
                    serialInventory = SaveServices.Load<GridContainer>(inventory.name);
                    serialInventory ??= new();

                    serialInventory.ContainerInfo = inventory;
                    serialInventory.Deserialize(_gridCollection);
                }
                else
                    serialInventory = new()
                    {
                        ContainerInfo = inventory
                    };


                _containers.AddUnique(serialInventory);
            }
            return serialInventory;
        }

        public IEnumerable<RotatableGrid> GetAllItems(ContainerInfo inventory)
        {
            return GetInventory(inventory).Grids;
        }

        public IEnumerable<RotatableGrid> GetItems(ContainerInfo inventory, Predicate<RotatableGrid> predicate)
        {
            return GetInventory(inventory).Grids.FindAll(predicate);
        }

        public bool IsItemIn(ContainerInfo inventory, RotatableGrid item)
        {
            return GetInventory(inventory).IsItemIn(item);
        }

        public bool Remove(RotatableGrid item, ContainerInfo inventory)
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
                if (Remove(item, container.ContainerInfo))
                    return true;
            return false;
        }

        public bool Remove(ContainerInfo inventory, GridInfo item, int amount)
        {
            var container = _containers.FirstOrDefault(c => c.ContainerInfo = inventory);
            if (container == null)
            {
                Debug.LogWarning($"Container is not found: {inventory}");
                return false;
            }
            return container.Remove(item, amount);



        }

        public bool Create(ContainerInfo inventoryInfo,
                    GridInfo inventoryItem,
                    bool stackable,
                    int id = -1)
        {
            if (!inventoryInfo.Compatible(inventoryItem))
            {
                Debug.LogWarning($"Trying to add an incompatible item {inventoryItem} to inventory  {inventoryInfo}.");
                return false;
            }
            if (stackable && TryStacking(inventoryInfo, inventoryItem))
                return true;

            var gridObject = new RotatableGrid(inventoryItem)
            {
                Stackable = stackable,
                Id = id,
            };
            if (!Place(inventoryInfo, gridObject))
                return false;
            Inventory.Events.Grids.OnItemCreated(gridObject);
            return true;
        }

        public bool Create(ContainerInfo inventoryInfo,
                            GridInfo inventoryItems,
                            int amount,
                            bool stackable,
                            int id = -1)
        {
            bool success = true;
            for (int i = 0; i < amount; i++)
            {
                success &= Create(inventoryInfo, inventoryItems, stackable, id);
            }
            return success;
        }


        private bool TryStacking(ContainerInfo inventoryInfo, GridInfo inventoryItem)
        {
            var serialInventory = GetInventory(inventoryInfo);
            foreach (var item in serialInventory.Grids)
            {
                if (item.GridInfo == inventoryItem &&
                    item.Amount < item.GridInfo.StackCapacity)
                {
                    item.Amount++;
                    Inventory.Events.Grids.OnItemStackModified?.Invoke(item, 1);
                    return true;
                }
            }
            return false;
        }

        public bool TryGetObjectAt(ContainerInfo inventoryInfo, Vector2Int position, out RotatableGrid gridObject)
        {
            return GetInventory(inventoryInfo).TryGetObjectAt(position, out gridObject);
        }

        public bool CanPlace(RotatableGrid grabbedObject, out ContainerInfo inventory)
        {
            var objectCopy = new RotatableGrid(grabbedObject);
            foreach (var container in _containers)
            {
                if (container.FitIn(objectCopy, grabbedObject.RootPosition))
                {
                    inventory = container.ContainerInfo;
                    return true;
                }
            }
            inventory = null;
            return false;
        }

        public bool CanPlace(RotatableGrid grabbedObject, ContainerInfo inventoryId, Vector2Int gridCoordinates)
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

        public bool TryPlaceAt(RotatableGrid grabbedObject, ContainerInfo inventoryInfo, Vector2Int gridCoordinates, int numToRelease, out int numReleased)
        {
            var inventory = GetInventory(inventoryInfo);
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