using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bakery
{
    public class GridContainerUI : MonoBehaviour
    {
        [SerializeField] private ContainerInfo _containerInfo;
        [SerializeField] private GridCellUI _cellPrefab;
        [SerializeField] private RectTransform _gridObjectUIContainer;
        [SerializeField] private RectTransform _cellsContainer;

        public ContainerInfo ContainerInfo => _containerInfo;


        private Vector2Int CellSize => _cellPrefab.Size;
        private List<GridCellUI> _cells = new();
        private readonly List<GridObjectUI> _gridObjects = new();


        void Awake()
        {
            transform.GetComponentsInChildren(true, _cells);
            foreach (var cell in _cells)
            {
                cell.GridContainerUI = this;
                cell.ContainerInfo = _containerInfo;
            }
        }

        void OnValidate()
        {
            if (_containerInfo == null)
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
            Inventory.Events.Grids.OnItemStackModified += OnItemStackModified;

            Inventory.Events.Controller.OnHighlight += OnHighlight;
            Inventory.Events.Controller.OnCleanHighlight += OnCleanHighlight;
            Inventory.Events.Controller.OnItemRotated += OnItemRotated;
        }
        void OnDisable()
        {

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

        private void OnItemRotated(RotatableGrid grid)
        {
            // OnCleanHighlight();
        }

        private void OnHighlight(RotatableGrid grabbedObject,
                            ContainerInfo containerInfo, Vector2Int
                            hoveredCoordinates)
        {
            if (containerInfo != _containerInfo) return;

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

        public void RemoveItem(RotatableGrid grid)
        {
            var gridObjectUI = _gridObjects.Find(obj => obj.Grid == grid);
            if (gridObjectUI != null)
            {
                gridObjectUI.Visibility = false;
                _gridObjects.Remove(gridObjectUI);
                InventorySpawner.Destroy(gridObjectUI);
            }
        }


        public GridObjectUI AddItem(RotatableGrid grid, InventorySpawner spawner)
        {
            var gridObjectUI =
                spawner.Spawn(_gridObjectUIContainer, grid);
            if (gridObjectUI == null)
            {
                Debug.LogWarning($"GridObjectUI not found for item {grid.GridInfo.name} in GridUIBuilder {this.name}", this);
                return null;
            }
            _gridObjects.Add(gridObjectUI);
            gridObjectUI.Visibility = true;
            return gridObjectUI;
        }

        public void UpdateGrid()
        {
            int i = 0;
            _cellsContainer.GetComponentsInChildren(true, _cells);
            while (i < _cells.Count)
            {
                var cell = _cells[i];
                _cells.RemoveAt(i);
                if (cell != null)
                    DestroyImmediate(cell.gameObject);
            }

            foreach (var position in _containerInfo.Coordinates)
            {
                if (_cells.Exists(cell => cell.GridCoordinates == position))
                    continue;
                var cell = Instantiate(_cellPrefab, _cellsContainer);
                cell.name = $"Cell {position.x},{position.y}";
                cell.Position = new Vector2Int(position.x * CellSize.x, -position.y * CellSize.y);
                cell.GridCoordinates = position;
                cell.ContainerInfo = _containerInfo;
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

        internal void Clear()
        {

            while (_gridObjects.Count > 0)
            {
                var grid = _gridObjects[0];
                _gridObjects.Remove(grid);
                InventorySpawner.Destroy(grid);
            }
        }

        internal void Initialize(InventorySpawner spawner)
        {
            var allItems = Inventory.Grids().GetAllItems(_containerInfo);
            foreach (var item in allItems)
                AddItem(item, spawner);
        }
    }
}