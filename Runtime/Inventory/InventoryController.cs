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

        [Header("Cursor")]
        [SerializeField] private CursorType _interactiveCursorType;

        private RotatableGrid _hoveredObject;
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
            _hoveredObject = null;
        }

        void OnEnable()
        {
            Inventory.Controller = () => this;
            User.Events.Cursor.OnEnter += OnCursorEnter;
            User.Events.Cursor.OnExit += OnCursorExit;

            if (_releaseOne != null)
                _releaseOne.action.performed += OnReleaseOne;

            if (_grabOne != null)
                _grabOne.action.performed += OnGrabOne;

            if (_grabAll != null)
                _grabAll.action.performed += OnGrabAll;

            if (_releaseAll != null)
                _releaseAll.action.performed += OnReleaseAll;

            if (_rotate != null)
                _rotate.action.performed += OnRotate;
        }

        void OnDisable()
        {
            Inventory.Controller = Inventory.UnregisterController;
            User.Events.Cursor.OnEnter -= OnCursorEnter;
            User.Events.Cursor.OnExit -= OnCursorExit;

            if (_grabOne != null)
                _grabOne.action.performed -= OnGrabOne;

            if (_releaseOne != null)
                _releaseOne.action.performed -= OnReleaseOne;

            if (_grabAll != null)
                _grabAll.action.performed -= OnGrabAll;

            if (_releaseAll != null)
                _releaseAll.action.performed -= OnReleaseAll;

            if (_rotate != null)
                _rotate.action.performed -= OnRotate;
        }

        void Update()
        {
            //Input events are called before the update loop
            _inputProcessed = false;

            if (_hoveredObject != null)
            {
                User.Cursor().Override(_interactiveCursorType);
            }

            if (GrabbedObject != null)
            {
                var hoveredObject = User.Raycast().HoveredObject;

                if (hoveredObject == null ||
                    !hoveredObject.TryGetComponent<GridCellUI>(out var cellUI))
                    return;
                _cellUI = cellUI;
                HighlightCells(_cellUI);
            }
        }

        private void OnGrabAll(InputAction.CallbackContext context)
        {
            if (_inputProcessed || _hoveredObject == null) return;

            Grab(_hoveredObject);
            _inputProcessed = true;
        }



        private void OnReleaseAll(InputAction.CallbackContext context)
        {
            if (_inputProcessed || GrabbedObject == null) return;
            Release(GrabbedObject);
            _inputProcessed = true;
        }



        private void OnRotate(InputAction.CallbackContext context)
        {
            if (GrabbedObject == null) return;
            GrabbedObject.Rotate();
            Inventory.Events.Controller.OnItemRotated?.Invoke(GrabbedObject);

        }

        private void OnCursorEnter(GameObject @object)
        {
            if (@object == null ||
                @object.TryGetComponent(out _cellUI) == false)
                return;

            if (GrabbedObject != null)
                return;

            if (!Inventory.Grids().TryGetObjectAt(_cellUI.GridInfo, _cellUI.GridCoordinates, out var gridObject))
            {
                HighlightCells(_cellUI);
                return;
            }

            _hoveredObject = gridObject;

        }


        private void HighlightCells(GridCellUI cellUI)
        {
            Inventory.Events.Controller.OnHighlight?.Invoke(GrabbedObject,
                                        cellUI.GridInfo,
                                        cellUI.GridCoordinates);
        }

        private void HighlightCells(RotatableGrid grabbedObject, GridInfo gridInfo)
        {
            Inventory.Events.Controller.OnHighlight?.Invoke(GrabbedObject,
                                        gridInfo,
                                        _cellUI.GridCoordinates);
        }

        private void CleanHighlight()
        {
            Inventory.Events.Controller.OnCleanHighlight?.Invoke();
        }

        private void OnCursorExit(GameObject @object)
        {
            if (@object == null ||
                @object.TryGetComponent(out GridCellUI cellUI) == false)
                return;
            CleanHighlight();
            _hoveredObject = null;
            User.Cursor().RemoveOverride();
        }

        private void OnGrabOne(InputAction.CallbackContext context)
        {
            if (_inputProcessed || _hoveredObject == null) return;
            if (!_hand.CanGrab(_hoveredObject)) return;

            Grab(_hoveredObject, 1);
            _inputProcessed = true;

        }
        private void OnReleaseOne(InputAction.CallbackContext context)
        {
            if (_inputProcessed || GrabbedObject == null) return;

            Release(GrabbedObject, 1);
            _inputProcessed = true;
        }

        private void Release(RotatableGrid grabbedObject, int numToRelease = -1)
        {
            if (_cellUI == null) return;

            if (numToRelease == -1)
                numToRelease = grabbedObject.Stack;

            if (Inventory.Grids().TryPlaceAt(grabbedObject,
                                        _cellUI.GridInfo,
                                        _cellUI.GridCoordinates,
                                        numToRelease,
                                        out int numReleased))
            {
                if (numReleased == numToRelease)
                {
                    var gridObjectUI = _hand.Release();
                    Inventory.Spawner().Destroy(gridObjectUI);
                    Inventory.Events.Controller.OnReleased?.Invoke(gridObjectUI, _hand, _cellUI);
                }
                else
                {
                    _hand.ModifyStack(-numReleased);
                }
            }
        }

        private void Grab(RotatableGrid hoveredObject, int numToGrab = -1)
        {
            if (numToGrab == -1)
                numToGrab = hoveredObject.Stack;

            //If the hand is already holding an object, 
            // we can only grab so much
            numToGrab = _hand.NumCanGrab(hoveredObject, numToGrab);

            Inventory.Grids().PickUp(hoveredObject, numToGrab, out int numGrabbed);

            if (_hand.IsEmpty)
            {
                RotatableGrid copy = new(hoveredObject)
                {
                    Stack = numGrabbed,
                };

                GridObjectUI gridObject = Inventory.Spawner().Spawn(_hand.transform as RectTransform,
                                        copy,
                                        _cellUI.Size);
                _hand.Grab(gridObject);
            }
            else
            {
                _hand.ModifyStack(numGrabbed);
            }

        }
    }
}
