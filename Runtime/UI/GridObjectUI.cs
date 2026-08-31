using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bakery
{


    public class GridObjectUI : MonoBehaviour, ICursorAttachable
    {
        [SerializeField] private GameObject _hiddable;
        [SerializeField] private GridInfo _gridInfo;
        [SerializeField] private Transform _background;
        [SerializeField] private Image _itemSprite;
        [SerializeReference] private GameObject _stackBg;
        [SerializeReference] private TextMeshProUGUI _stackCountText;
        // [SerializeReference] private GameObject CornerTopLeft;
        // [SerializeReference] private GameObject CornerTopRight;
        // [SerializeReference] private GameObject CornerBottomLeft;
        // [SerializeReference] private GameObject CornerBottomRight;
        [SerializeReference] private Image CellBGPrefab;
        private RotatableGrid _grid;
        private Vector2Int _size;
        private InstancePool<Image> _cellBgs;

        public RotatableGrid Grid => _grid;

        private RectTransform rectTransform => transform as RectTransform;

        public Vector2 Size
        {
            get; private set;
        }
        public bool Grabbed => _grid != null && _grid.Grabbed;
        public bool FullStack => _gridInfo != null && _grid.Stack >= _gridInfo.StackCapacity;

        public int MaxStack => _gridInfo != null ? _gridInfo.StackCapacity : 1;
        public int Stack => _grid != null ? _grid.Stack : 0;

        void Awake()
        {
            _cellBgs = new(CellBGPrefab, _background);
        }

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

            _grid.Grabbed = true;
            _hiddable.SetActive(false);
            //UpdateGrid(Grid);
            // _hiddable.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(LateUpdateGridRoutine(Grid));
        }

        public void Release()
        {
            _grid.Grabbed = false;
        }

        private IEnumerator LateUpdateGridRoutine(RotatableGrid grid)
        {
            yield return null;
            UpdateGrid(grid);
            _hiddable.SetActive(true);
        }

        private void OnItemRotated(RotatableGrid grid)
        {
            if (grid != _grid) return;
            UpdateGrid(grid);
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

        public bool Overlaps(RectTransform rectTransform)
        {
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(this.transform, _background);
            return bounds.Intersects(RectTransformUtility.CalculateRelativeRectTransformBounds(this.transform, rectTransform));
        }

        private void UpdateGrid(RotatableGrid grid)
        {

            _grid = grid;
            _cellBgs.Clear();
            rectTransform.anchoredPosition = new Vector2(grid.RootPosition.x * _size.x,
                                                -grid.RootPosition.y * _size.y);

            _gridInfo = grid.GridInfo;
            SetupStack(grid);
            foreach (var pos in _grid.LocalPositions)
            {
                var cellBg = _cellBgs.Add();
                cellBg.rectTransform.sizeDelta = new Vector2(_size.x, _size.y);
                cellBg.rectTransform.anchoredPosition =
                    new Vector2(pos.x * _size.x, -pos.y * _size.y);

            }
            Size = _gridInfo.Size * _size;

            _itemSprite.sprite = _gridInfo.Sprite;
            var cellBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(transform, _background);
            switch (grid.Rotation)
            {
                case 0:
                    _itemSprite.rectTransform.localRotation = Quaternion.Euler(Vector3.zero);
                    _itemSprite.rectTransform.sizeDelta = new Vector2(cellBounds.size.x, cellBounds.size.y);
                    break;
                case 1:
                    _itemSprite.rectTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
                    _itemSprite.rectTransform.sizeDelta = new Vector2(cellBounds.size.y, cellBounds.size.x);
                    break;
                case 2:
                    _itemSprite.rectTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, 180));
                    _itemSprite.rectTransform.sizeDelta = new Vector2(cellBounds.size.x, cellBounds.size.y);
                    break;
                case 3:
                    _itemSprite.rectTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, 270));
                    _itemSprite.rectTransform.sizeDelta = new Vector2(cellBounds.size.y, cellBounds.size.x);
                    break;
            }



            _itemSprite.rectTransform.anchoredPosition = new Vector2(cellBounds.center.x, cellBounds.center.y);

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
