using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bakery
{
    public class GridUIBuilder : MonoBehaviour
    {
        [SerializeField] private GridInfo _gridInfo;
        [SerializeField] private GridCellUI _cellPrefab;
        [SerializeField] private GridObjectUI _gridObjectUIPrefab;
        [SerializeField] private RectTransform _gridObjectUIContainer;

        public GridInfo GridInfo => _gridInfo;


        private Vector2Int CellSize => _cellPrefab.Size;
        private List<GridCellUI> _cells = new();
        private readonly List<GridObjectUI> _gridObjects = new();


        void Awake()
        {
            transform.GetComponentsInChildren(true, _cells);
            foreach (var cell in _cells)
            {
                cell.GridUIBuilder = this;
            }
        }

        void OnValidate()
        {
            if (_gridInfo == null)
            {
                Debug.LogWarning($"GridInfo reference is missing in GridUIBuilder {this.name}", this);
                return;
            }
            if (_cellPrefab == null)
            {
                Debug.LogWarning($"CellPrefab reference is missing in GridUIBuilder {this.name}", this);
                return;
            }
        }
        void OnEnable()
        {
            Inventory.Events.Grids.OnItemAdded += OnItemAdded;
            Inventory.Events.Grids.OnItemRemoved += OnItemRemoved;
            Inventory.Events.Grids.OnItemPlaced += OnItemPlaced;
            Inventory.Events.Grids.OnItemStacked += OnItemStacked;
            Inventory.Events.Controller.OnReleased += OnItemReleased;
            Inventory.Events.Controller.OnGrabbed += OnItemGrabbed;
            Inventory.Events.Controller.OnHighlight += OnHighlight;
            Inventory.Events.Controller.OnCleanHighlight += OnCleanHighlight;
            Inventory.Events.Controller.OnItemRotated += OnItemRotated;
        }
        void OnDisable()
        {
            Inventory.Events.Grids.OnItemAdded -= OnItemAdded;
            Inventory.Events.Grids.OnItemRemoved -= OnItemRemoved;
            Inventory.Events.Grids.OnItemPlaced -= OnItemPlaced;
            Inventory.Events.Grids.OnItemStacked -= OnItemStacked;
            Inventory.Events.Controller.OnGrabbed -= OnItemGrabbed;
            Inventory.Events.Controller.OnHighlight -= OnHighlight;
            Inventory.Events.Controller.OnCleanHighlight -= OnCleanHighlight;
            Inventory.Events.Controller.OnItemRotated -= OnItemRotated;
        }

        private void OnItemStacked(GridContainer container, RotatableGrid grid)
        {
            if (container.GridInfo != _gridInfo) return;
            var gridObjectUI = _gridObjects.Find(obj => obj.Grid == grid);
            if (gridObjectUI == null)
                Debug.LogWarning($"GridObjectUI not found for stacked item {grid.GridInfo.name} in GridUIBuilder {this.name}", this);
            else
                gridObjectUI.UpdateStack();
        }

        private void OnItemReleased(GridObjectUI gridObjectUI, InventoryHand hand, GridCellUI cellUI)
        {
            if (cellUI.GridInfo != _gridInfo) return;
            _gridObjects.Add(gridObjectUI);
            gridObjectUI.transform.SetParent(_gridObjectUIContainer, false);
            gridObjectUI.transform.SetAsLastSibling();
            Inventory.Grids().Place(gridObjectUI.Grid, cellUI.GridInfo, cellUI.GridCoordinates);
        }

        private void OnItemRotated(RotatableGrid grid)
        {
            // OnCleanHighlight();
        }

        private void OnHighlight(RotatableGrid grabbedObject,
                            GridInfo gridInfo, Vector2Int
                            hoveredCoordinates)
        {
            if (gridInfo != _gridInfo) return;

            if (grabbedObject == null)
            {
                foreach (var cell in _cells)
                {
                    if (cell.GridCoordinates == hoveredCoordinates)
                        cell.Highlight();
                    else
                        cell.CleanHighlight();
                }
                return;
            }

            var rotatableGrid = new RotatableGrid(grabbedObject)
            {
                RootPosition = hoveredCoordinates
            };
            Highlight(rotatableGrid);
        }

        public void Highlight(RotatableGrid rotatableGrid)
        {
            foreach (var cell in _cells)
            {
                if (rotatableGrid.WorldPositions.Any(pos => pos == cell.GridCoordinates))
                    cell.Highlight();
                else
                    cell.CleanHighlight();
            }
        }

        private void OnCleanHighlight()
        {
            foreach (var cell in _cells)
                cell.CleanHighlight();
        }

        private void OnItemGrabbed(RotatableGrid grid, InventoryHand hand)
        {
            if (grid == null) return;
            var gridObjectUI = _gridObjects.Find(obj => obj.Grid == grid);
            if (gridObjectUI == null) return;

            hand.Grab(gridObjectUI);
        }

        private void OnItemPlaced(GridContainer container, RotatableGrid grid)
        {
            if (container.GridInfo != _gridInfo) return;
            var gridObjectUI = _gridObjects.Find(obj => obj.Grid == grid);
            if (gridObjectUI == null)
                gridObjectUI = AddItemUI(container, grid);

            gridObjectUI.Place(grid);

        }

        private void OnItemRemoved(GridContainer inventory, RotatableGrid grid)
        {
            if (inventory.GridInfo != _gridInfo) return;
            _gridObjects.RemoveAll(obj => obj.Grid == grid);
        }

        private void OnItemAdded(GridContainer inventory, RotatableGrid grid)
        {
            if (inventory.GridInfo != _gridInfo) return;
            AddItemUI(inventory, grid);
        }

        private GridObjectUI AddItemUI(GridContainer inventory, RotatableGrid grid)
        {
            var gridObjectUI = Instantiate(_gridObjectUIPrefab, _gridObjectUIContainer);
            gridObjectUI.Initialize(grid, _cellPrefab.Size);
            _gridObjects.Add(gridObjectUI);
            return gridObjectUI;
        }

        public void UpdateGrid()
        {
            int i = 0;
            transform.GetComponentsInChildren(true, _cells);
            while (i < _cells.Count)
            {
                var cell = _cells[i];
                _cells.RemoveAt(i);
                if (cell != null)
                    DestroyImmediate(cell.gameObject);
            }

            foreach (var position in _gridInfo.Coordinates)
            {
                if (_cells.Exists(cell => cell.GridCoordinates == position))
                    continue;
                var cell = Instantiate(_cellPrefab, transform);
                cell.name = $"Cell {position.x},{position.y}";
                cell.Position = new Vector2Int(position.x * CellSize.x, -position.y * CellSize.y);
                cell.GridCoordinates = position;
                cell.GridInfo = _gridInfo;
                _cells.Add(cell);
            }
        }
    }
}