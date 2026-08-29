using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bakery
{
    public class InventoryController : MonoBehaviour, IInventoryController
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference _grabOne;
        [SerializeField] private InputActionReference _releaseOne;
        [SerializeField] private InputActionReference _grabAll;
        [SerializeField] private InputActionReference _releaseAll;
        [SerializeField] private InputActionReference _rotate;

        [Header("Inventory Hand")]
        [SerializeField] private InventoryHand _hand;
        [SerializeField] private InventoryTrashUI _trash;

        [Header("Cursor")]
        [SerializeField] private CursorType _interactiveCursorType;

        private RotatableGrid _hoveredGrid;
        private RotatableGrid GrabbedObject
        {
            get
            {
                if (_hand.IsEmpty)
                    return null;
                return _hand.GrabbedObject.Grid;
            }
        }


        private GridCellUI _cellUI;


        //We use This flag to prevent multiple inputs from
        // being processed in the same frame
        private bool _inputProcessed;

        void Awake()
        {
            _hoveredGrid = null;
        }

        void OnEnable()
        {
            Inventory.Controller = () => this;

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
        }

        void OnDisable()
        {
            Inventory.Controller = Inventory.UnregisterController;

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
        }

        void Update()
        {
            //Input events are called before the update loop
            _inputProcessed = false;

            if (_hoveredGrid != null)
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

            if (_trash != null && _trash.IsHovering)
            {
                Trash(GrabbedObject);
            }
            Release(GrabbedObject);
            _inputProcessed = true;
        }

        private void Trash(RotatableGrid grabbedObject, int numToTrash = -1)
        {
            if (grabbedObject == null) return;
            if (numToTrash == -1 || numToTrash >= _hand.AmountHeld)
            {
                var releasedObject = _hand.Release();
                _trash.Trash();
                Inventory.Spawner().Destroy(releasedObject);
                return;
            }

            _hand.ModifyStack(-numToTrash);
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
                    _cellUI.GridInfo.Filter != GrabbedObject.GridInfo.Filter))
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

            if (_trash != null && _trash.IsHovering)
            {
                Trash(GrabbedObject, 1);
            }

            Release(GrabbedObject, 1);
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
                var gridObjectUI = _hand.Release();
                Inventory.Spawner().Destroy(gridObjectUI);
                Inventory.Events.Controller.OnReleased?.Invoke(gridObjectUI, _hand, _cellUI);
            }
            else
            {
                //just to update the number in the UI. 
                // the grabbed object's stack has been reduced but the hand still holds it
                _hand.ModifyStack(0);
            }
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

                GridObjectUI gridObject = Inventory.Spawner().Spawn(_hand.transform as RectTransform,
                                        pickedUpObject,
                                        _cellUI.Size);
                _hand.Grab(gridObject);

            }
            else
            {
                _hand.ModifyStack(pickedUpObject.Stack);
            }
            return true;
        }
    }
}
