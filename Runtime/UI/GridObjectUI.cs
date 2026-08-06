using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bakery
{
    public class GridObjectUI : MonoBehaviour, ICursorAttachable
    {
        [SerializeField] private GridInfo _gridInfo;
        [SerializeField] private Transform _background;
        [SerializeReference] private GameObject _stackBg;
        [SerializeReference] private TextMeshProUGUI _stackCountText;
        [SerializeReference] private GameObject CornerTopLeft;
        [SerializeReference] private GameObject CornerTopRight;
        [SerializeReference] private GameObject CornerBottomLeft;
        [SerializeReference] private GameObject CornerBottomRight;
        [SerializeReference] private Image CellBGPrefab;
        private RotatableGrid _grid;
        private Vector2Int _size;
        private readonly List<Image> _cellBgs = new();

        public RotatableGrid Grid => _grid;

        private RectTransform rectTransform => transform as RectTransform;

        public Vector2 Size
        {
            get; private set;
        }
        public bool Grabbed { get; private set; }



        void OnEnable()
        {
            Inventory.Events.Controller.OnItemRotated += OnItemRotated;
        }

        void OnDisable()
        {
            Inventory.Events.Controller.OnItemRotated -= OnItemRotated;

        }

        public void Grab()
        {
            Grabbed = true;
            StopAllCoroutines();
            StartCoroutine(LateUpdateGridRoutine(Grid));
        }

        public void Release()
        {
            Grabbed = false;
        }

        private IEnumerator LateUpdateGridRoutine(RotatableGrid grid)
        {
            yield return null;
            UpdateGrid(grid);
        }

        private void OnItemRotated(RotatableGrid grid)
        {
            if (grid != _grid) return;
            UpdateGrid(grid);
        }

        public void SetGridInfo(GridInfo gridInfo)
        {
            CleanBackgroundTiles();
            _gridInfo = gridInfo;
            SetupStack(null);
            foreach (var pos in _gridInfo.Coordinates)
            {
                var cellBg = Instantiate(CellBGPrefab, _background);
                cellBg.rectTransform.sizeDelta = new Vector2(_size.x, _size.y);
                cellBg.rectTransform.anchoredPosition =
                    new Vector2(pos.x * _size.x, -pos.y * _size.y);

                _cellBgs.Add(cellBg);
            }
            Size = _gridInfo.MaxSize * _size;
        }

        private void CleanBackgroundTiles()
        {
            _cellBgs.Clear();
            foreach (Transform child in _background)
            {
                Destroy(child.gameObject);
            }
        }

        private void SetupStack(RotatableGrid grid)
        {
            if (grid != null &&
                _gridInfo.StackCapacity > 1 &&
                grid.Stack > 1)
            {
                _stackBg.SetActive(true);

                _stackCountText.text = $"{grid.Stack}/{_gridInfo.StackCapacity}";
            }
            else
            {
                _stackBg.SetActive(false);
            }
        }

        internal void Initialize(RotatableGrid grid, Vector2Int size)
        {
            _size = size;
            UpdateGrid(grid);
        }

        private void UpdateGrid(RotatableGrid grid)
        {

            _grid = grid;
            _cellBgs.Clear();
            rectTransform.anchoredPosition = new Vector2(grid.RootPosition.x * _size.x,
                                                -grid.RootPosition.y * _size.y);

            foreach (Transform child in _background)
            {
                Destroy(child.gameObject);
            }
            _gridInfo = grid.GridInfo;
            SetupStack(grid);
            foreach (var pos in _grid.LocalPositions)
            {
                var cellBg = Instantiate(CellBGPrefab, _background);
                cellBg.rectTransform.sizeDelta = new Vector2(_size.x, _size.y);
                cellBg.rectTransform.anchoredPosition =
                    new Vector2(pos.x * _size.x, -pos.y * _size.y);
                _cellBgs.Add(cellBg);
            }
            Size = _gridInfo.Size * _size;
        }

        internal void Place(RotatableGrid grid)
        {
            UpdateGrid(grid);
        }

        public void UpdatePosition(Vector2 position)
        {
            transform.position = position;
        }

        internal void UpdateStack()
        {
            SetupStack(_grid);
        }
    }
}
