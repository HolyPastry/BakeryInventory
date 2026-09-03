using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bakery
{
    public class GridContainerUI : MonoBehaviour
    {
        [SerializeField] private GridInfo _gridInfo;
        [SerializeField] private GridCellUI _cellPrefab;
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
                cell.GridContainerUI = this;
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

            // Inventory.Events.Grids.OnItemPlaced += OnItemPlaced;

            Inventory.Events.Grids.OnItemStackModified += OnItemStackModified;

            Inventory.Events.Controller.OnHighlight += OnHighlight;
            Inventory.Events.Controller.OnCleanHighlight += OnCleanHighlight;
            Inventory.Events.Controller.OnItemRotated += OnItemRotated;
        }
        void OnDisable()
        {

            //s Inventory.Events.Grids.OnItemPlaced -= OnItemPlaced;

            Inventory.Events.Grids.OnItemStackModified -= OnItemStackModified;

            Inventory.Events.Controller.OnHighlight -= OnHighlight;
            Inventory.Events.Controller.OnCleanHighlight -= OnCleanHighlight;
            Inventory.Events.Controller.OnItemRotated -= OnItemRotated;
        }


        private void OnItemStackModified(RotatableGrid hoveredObject, int amount)
        {
            var gridObjectUI = _gridObjects.Find(obj => obj.Grid == hoveredObject);
            if (gridObjectUI == null)
                return;
            gridObjectUI.UpdateStack();
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


        // private void OnItemPlaced(GridContainer container, RotatableGrid grid)
        // {
        //     if (container.GridInfo != _gridInfo) return;

        //     var gridObjectUI = _gridObjects.Find(obj => obj.Grid == grid);
        //     if (gridObjectUI == null)
        //         gridObjectUI = AddItemUI(grid);

        //     gridObjectUI.transform.SetParent(_gridObjectUIContainer, false);
        //     gridObjectUI.transform.SetAsLastSibling();
        //     gridObjectUI.Place(grid);
        // }

        public void RemoveItem(RotatableGrid grid)
        {
            var gridObjectUI = _gridObjects.Find(obj => obj.Grid == grid);
            if (gridObjectUI != null)
            {
                _gridObjects.Remove(gridObjectUI);
                InventorySpawner.Destroy(gridObjectUI);
            }
        }


        public GridObjectUI AddItem(RotatableGrid grid, InventorySpawner spawner)
        {
            var gridObjectUI =
                spawner.Spawn(_gridObjectUIContainer, grid, _cellPrefab.Size);
            if (gridObjectUI == null)
            {
                Debug.LogWarning($"GridObjectUI not found for item {grid.GridInfo.name} in GridUIBuilder {this.name}", this);
                return null;
            }
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

        internal GridObjectUI GetGridObjectUI(RotatableGrid hoveredObject)
        {
            var gridObjectUI = _gridObjects.Find(obj => obj.Grid == hoveredObject);
            if (gridObjectUI == null)
                Debug.LogWarning($"GridObjectUI not found for item {hoveredObject.GridInfo.name} in GridUIBuilder {this.name}", this);
            return gridObjectUI;
        }
    }
}