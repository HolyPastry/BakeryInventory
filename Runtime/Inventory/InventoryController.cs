using System;
using System.Collections.Generic;
using System.Linq;
using Bakery.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bakery
{
    public class InventoryController : MonoBehaviour
    {
        [Header("Local References")]
        [SerializeReference] private InventoryHand _hand;
        [SerializeReference] private List<InventoryTrashUI> _trashes = new();
        [SerializeReference] private InventorySpawner _spawner;


        [Header("Input Actions")]
        [SerializeField] private InputActionReference _grabOne;
        [SerializeField] private InputActionReference _releaseOne;
        [SerializeField] private InputActionReference _grabAll;
        [SerializeField] private InputActionReference _releaseAll;
        [SerializeField] private InputActionReference _rotate;


        [Header("Cursor")]
        [SerializeField] private CursorType _interactiveCursorType;

        private RotatableGrid _hoveredGrid;
        private RotatableGrid GrabbedObject
        {
            get
            {
                if (_hand.IsEmpty)
                    return null;
                if (_hand.GrabbedObject.Grid == null)
                    Debug.LogWarning($"Grid should not be null inside the GrabbedObject in the Hand: {_hand.GrabbedObject}");

                return _hand.GrabbedObject.Grid;
            }
        }


        private GridCellUI _cellUI;

        //We use This flag to prevent multiple inputs from
        // being processed in the same frame
        private bool _inputProcessed;

        private List<GridContainerUI> _containers = new();

        void Awake()
        {
            _hoveredGrid = null;
            GetComponentsInChildren(true, _containers);
        }

        void OnEnable()
        {

            if (_releaseOne != null)
                _releaseOne.action.canceled += OnReleaseOne;

            if (_grabOne != null)
                _grabOne.action.canceled += OnGrabOne;

            if (_grabAll != null)
                _grabAll.action.canceled += OnGrabAll;

            if (_releaseAll != null)
                _releaseAll.action.canceled += OnReleaseAll;

            if (_rotate != null)
                _rotate.action.performed += OnRotate;

            Inventory.Events.Grids.OnItemAdded += OnItemAdded;
            Inventory.Events.Grids.OnItemRemoved += OnItemRemoved;

            UpdateGrids();
        }

        private void UpdateGrids()
        {
            foreach (var container in _containers)
            {
                var items = Inventory.Grids().GetAllItems(container.GridInfo);
                foreach (var item in items)
                {
                    container.AddItem(item, _spawner);
                }
            }
        }

        void OnDisable()
        {

            if (_grabOne != null)
                _grabOne.action.canceled -= OnGrabOne;

            if (_releaseOne != null)
                _releaseOne.action.canceled -= OnReleaseOne;

            if (_grabAll != null)
                _grabAll.action.canceled -= OnGrabAll;

            if (_releaseAll != null)
                _releaseAll.action.canceled -= OnReleaseAll;

            if (_rotate != null)
                _rotate.action.performed -= OnRotate;

            Inventory.Events.Grids.OnItemAdded -= OnItemAdded;
            Inventory.Events.Grids.OnItemRemoved -= OnItemRemoved;

            CleanGrids();
        }

        private void CleanGrids()
        {
            foreach (var container in _containers)
            {
                container.Clear();
            }
        }

        void Update()
        {
            //Input events are called before the update loop
            _inputProcessed = false;

            if (_hoveredGrid != null && !_hoveredGrid.Locked)
            {
                User.Cursor().Override(_interactiveCursorType);
            }

            var hoveredObject = User.Raycast().HoveredObject;
            if (hoveredObject == null ||
                !hoveredObject.TryGetComponent<GridCellUI>(out _cellUI))
            {
                _cellUI = null;
                _hoveredGrid = null;
                UpdateHighlight();
                return;
            }

            Inventory.Grids().TryGetObjectAt(_cellUI.GridInfo,
                                                _cellUI.GridCoordinates,
                                                out _hoveredGrid);
            UpdateHighlight();
        }

        private void OnReleaseAll(InputAction.CallbackContext context)
        {
            if (_inputProcessed || GrabbedObject == null) return;

            if (_trashes != null && _trashes.Any(t => t.IsHovering))
            {
                Trash(GrabbedObject);
            }
            Release(GrabbedObject);
            _inputProcessed = true;
        }

        private void Trash(RotatableGrid grabbedObject, int numToTrash = -1)
        {
            if (grabbedObject == null) return;
            var trash = _trashes.FirstOrDefault(t => t.IsHovering);
            if (trash == null) return;
            if (numToTrash == -1 || numToTrash >= _hand.AmountHeld)
            {
                var releasedObject = _hand.Release();
                trash.Trash();
                InventorySpawner.Destroy(releasedObject);
                return;
            }

            _hand.ModifyStack(-numToTrash);
            trash.Trash();
        }

        private void OnRotate(InputAction.CallbackContext context)
        {
            if (GrabbedObject == null) return;
            GrabbedObject.Rotate();
            Inventory.Events.Controller.OnItemRotated?.Invoke(GrabbedObject);

        }

        private void UpdateHighlight()
        {
            if (_cellUI == null ||
                    (GrabbedObject != null &&
                    !_cellUI.GridInfo.Compatible(GrabbedObject.GridInfo)))
            {
                Inventory.Events.Controller.OnCleanHighlight?.Invoke();
                return;
            }

            Inventory.Events.Controller.OnHighlight?.Invoke(GrabbedObject,
                                    _cellUI.GridInfo,
                                    _cellUI.GridCoordinates);
        }

        private void OnGrabOne(InputAction.CallbackContext context)
        {
            if (_inputProcessed || _hoveredGrid == null) return;

            if (!_hand.CanGrab(_hoveredGrid)) return;

            if (Grab(_hoveredGrid, 1))
                _inputProcessed = true;
        }

        private void OnGrabAll(InputAction.CallbackContext context)
        {
            if (_inputProcessed || _hoveredGrid == null) return;
            if (!_hand.CanGrab(_hoveredGrid)) return;

            if (Grab(_hoveredGrid))
                _inputProcessed = true;
        }
        private void OnReleaseOne(InputAction.CallbackContext context)
        {
            if (_inputProcessed || GrabbedObject == null) return;
            if (_trashes != null && _trashes.Any(t => t.IsHovering))
                Trash(GrabbedObject, 1);

            Release(new(GrabbedObject), 1);
            _inputProcessed = true;
        }

        private void Release(RotatableGrid grabbedObject, int numToRelease = -1)
        {

            if (_cellUI == null || grabbedObject == null) return;

            var stackBeforeRelease = grabbedObject.Stack;
            if (numToRelease == -1)
                numToRelease = grabbedObject.Stack;

            if (!Inventory.Grids().TryPlaceAt(grabbedObject,
                                        _cellUI.GridInfo,
                                        _cellUI.GridCoordinates,
                                        numToRelease,
                                        out int numReleased))
                return;

            if (numReleased == 0)
                return;

            if (numReleased == stackBeforeRelease)
            {
                RemoveFromHand();
            }
            else
            {
                //just to update the number in the UI. 
                // the grabbed object's stack has been reduced but the hand still holds it
                _hand.ModifyStack(0);
            }
        }

        public void RemoveFromHand()
        {
            var gridObjectUI = _hand.Release();
            InventorySpawner.Destroy(gridObjectUI);
            Inventory.Events.Controller.OnReleased?.Invoke(gridObjectUI, _hand, _cellUI);
        }

        private bool Grab(RotatableGrid hoveredObject, int numToGrab = -1)
        {
            if (numToGrab == -1)
                numToGrab = hoveredObject.Stack;

            //If the hand is already holding an object, 
            // we can only grab so much
            numToGrab = _hand.NumCanGrab(hoveredObject, numToGrab);
            if (numToGrab <= 0)
                return false;


            Inventory.Grids().PickUp(hoveredObject, numToGrab, out RotatableGrid pickedUpObject);

            if (_hand.IsEmpty)
            {
                CreateInHand(pickedUpObject);
            }
            else
            {
                _hand.ModifyStack(pickedUpObject.Stack);
            }
            return true;
        }

        public void CreateInHand(GridInfo info, int quantity)
        {
            RotatableGrid grid = new(info)
            {
                Stack = quantity
            };
            CreateInHand(grid);
        }

        private void CreateInHand(RotatableGrid pickedUpObject)
        {
            GridObjectUI gridObject = _spawner.Spawn(_hand.transform as RectTransform,
                                                    pickedUpObject);

            _hand.Grab(gridObject);
        }

        private void OnItemRemoved(GridContainer inventory, RotatableGrid grid)
        {

            if (!_containers.Exists(ui => ui.GridInfo == inventory.GridInfo))
                return;
            var container = _containers.First(ui => ui.GridInfo == inventory.GridInfo);
            container.RemoveItem(grid);
        }

        private void OnItemAdded(GridContainer inventory, RotatableGrid grid)
        {
            if (!_containers.Exists(ui => ui.GridInfo == inventory.GridInfo))
                return;
            var container = _containers.First(ui => ui.GridInfo == inventory.GridInfo);

            container.AddItem(grid, _spawner);
        }
    }
}
