using System;
using System.Collections.Generic;
using UnityEngine;


namespace Bakery
{
    public static class Inventory
    {
        public static class Events
        {
            public static class Grids
            {
                public static Action<GridContainer, RotatableGrid> OnItemAdded = delegate { };

                public static Action<GridContainer, RotatableGrid> OnItemRemoved = delegate { };
                // public static Action<GridContainer, RotatableGrid> OnItemPlaced = delegate { };

                // public static Action<GridContainer, RotatableGrid> OnItemStacked = delegate { };

                public static Action<RotatableGrid, int> OnItemStackModified = delegate { };

                public static Action<RotatableGrid> OnItemCreated = delegate { };
            }
            public static class Controller
            {
                public static Action<RotatableGrid, InventoryHand> OnGrabbed = delegate { };
                public static Action<GridObjectUI, InventoryHand, GridCellUI> OnReleased = delegate { };
                public static Action<RotatableGrid> OnItemRotated = delegate { };
                public static Action<RotatableGrid, GridInfo, Vector2Int> OnHighlight = delegate { };
                public static Action OnCleanHighlight = delegate { };
            }
        }
        public static Func<IInventoryGridManager> Grids = UnregisterManager;

        private static IInventoryGridManager _dummyManager;

        public static IInventoryGridManager UnregisterManager()
        {
            Debug.Log("No Inventory Grid Manager registered, returning dummy manager");
            if (_dummyManager == null)
            {
                _dummyManager = new InventoryGridDummyManager();
            }
            return _dummyManager;
        }

        internal class InventoryGridDummyManager : IInventoryGridManager
        {
            public CustomYieldInstruction WaitUntilReady => null;

            public bool Place(GridInfo inventory, RotatableGrid item)
            => false;

            public void Place(GridInfo inventory, List<RotatableGrid> inventoryItems)
            { }


            public IEnumerable<RotatableGrid> GetAllItems(GridInfo inventory)
                => new List<RotatableGrid>();

            public IEnumerable<RotatableGrid> GetItems(GridInfo inventory, Predicate<RotatableGrid> predicate)
                => new List<RotatableGrid>();

            public bool IsItemIn(GridInfo inventory, RotatableGrid item)
                => false;
            public bool Create(GridInfo inventoryInfo, List<GridInfo> inventoryItems)
                => false;
            public bool Create(GridInfo inventoryInfo, GridInfo inventoryItems)
                => false;

            public bool TryGetObjectAt(GridInfo gridInfo,
                                    Vector2Int position, out RotatableGrid gridObject)
            {
                gridObject = null;
                return false;
            }

            public bool Remove(RotatableGrid item, GridInfo inventory)
             => false;

            public bool Remove(RotatableGrid item)
            => false;

            public void PickUp(RotatableGrid hoveredObject,
                                int numToGrab, out RotatableGrid pickedUpGrid)
            {
                pickedUpGrid = null;
                //noop

            }

            public bool TryPlaceAt(RotatableGrid grabbedObject,
                                    GridInfo gridInfo,
                                    Vector2Int gridCoordinates,
                                    int numToPlace,
                                    out int numPlaced)
            {
                numPlaced = 0;
                //noop
                return false;
            }

            public bool Create(GridInfo inventoryInfo, GridInfo inventoryItems, int amount)
            {
                return false;
            }

            public bool Remove(GridInfo inventory, GridInfo item, int amount)
            {
                return false;
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Events.Controller.OnCleanHighlight = delegate { };
            Events.Controller.OnHighlight = delegate { };
            Events.Controller.OnGrabbed = delegate { };
            Events.Controller.OnItemRotated = delegate { };
            Events.Controller.OnReleased = delegate { };

            Events.Grids.OnItemAdded = delegate { };
            // Events.Grids.OnItemPlaced = delegate { };
            Events.Grids.OnItemRemoved = delegate { };

            Grids = UnregisterManager;

            Debug.Log("[Inventory] Static fields reset (domain reload skipped)");
        }
    }
}