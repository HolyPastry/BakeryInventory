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
        [SerializeField] private InputActionReference _rotate;

        [Header("Inventory Hand")]
        [SerializeField] private InventoryHand _hand;

        [Header("Cursor")]
        [SerializeField] private CursorType _interactiveCursorType;

        private RotatableGrid _hoveredObject;
        private RotatableGrid _grabbedObject;
        private GridCellUI _cellUI;


        //We use This flag to prevent multiple inputs from
        // being processed in the same frame
        private bool _inputProcessed;

        void Awake()
        {
            _grabbedObject = null;
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

            if (_grabbedObject != null)
            {
                var hoveredObject = User.Raycast().HoveredObject;

                if (hoveredObject == null ||
                    !hoveredObject.TryGetComponent<GridCellUI>(out var cellUI))
                    return;
                _cellUI = cellUI;
                HighlightCells(_cellUI);
            }
        }

        private void OnRotate(InputAction.CallbackContext context)
        {
            if (_grabbedObject == null) return;
            _grabbedObject.Rotate();
            Inventory.Events.Controller.OnItemRotated?.Invoke(_grabbedObject);

        }

        private void OnCursorEnter(GameObject @object)
        {
            if (@object == null ||
                @object.TryGetComponent(out _cellUI) == false)
                return;

            if (_grabbedObject != null)
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
            Inventory.Events.Controller.OnHighlight?.Invoke(_grabbedObject,
                                        cellUI.GridInfo,
                                        cellUI.GridCoordinates);
        }

        private void HighlightCells(RotatableGrid grabbedObject, GridInfo gridInfo)
        {
            Inventory.Events.Controller.OnHighlight?.Invoke(_grabbedObject,
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

            GrabOne(_hoveredObject);
            _inputProcessed = true;

        }
        private void OnReleaseOne(InputAction.CallbackContext context)
        {
            if (_inputProcessed || _grabbedObject == null) return;

            ReleaseOne(_grabbedObject);
            _inputProcessed = true;
        }

        private void GrabOne(RotatableGrid hoveredObject)
        {
            hoveredObject.Grabbed = true;
            _hoveredObject = null;
            _grabbedObject = hoveredObject;

            Inventory.Events.Controller.OnGrabbed?.Invoke(hoveredObject, _hand);
            Inventory.Grids().Remove(_grabbedObject);
        }



        private void ReleaseOne(RotatableGrid grabbedObject)
        {
            if (_cellUI == null) return;
            if (!Inventory.Grids().CanPlace(grabbedObject,
                                    _cellUI.GridInfo,
                                    _cellUI.GridCoordinates)) return;
            var gridObjectUI = _hand.Release();

            _hoveredObject = _grabbedObject;
            _grabbedObject = null;
            Inventory.Events.Controller.OnReleased?.Invoke(gridObjectUI, _hand, _cellUI);
        }
    }
}
