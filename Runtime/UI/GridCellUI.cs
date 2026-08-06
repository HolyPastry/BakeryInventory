using System;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.UI;

namespace Bakery
{
    public class GridCellUI : MonoBehaviour
    {
        [SerializeField, Self] private Image image;
        [SerializeField] private Color _highlightColor;
        [SerializeField] private Color _defaultColor;

        public Vector2Int Size
        {
            get
            {
                var rect = image.rectTransform.rect;
                return new Vector2Int(Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height));
            }
        }

        public Vector2Int Position
        {
            get => Vector2Int.RoundToInt((transform as RectTransform).anchoredPosition);
            set => (transform as RectTransform).anchoredPosition = value;
        }
        public Vector2Int GridCoordinates;

        public GridInfo GridInfo;

        public GridUIBuilder GridUIBuilder { get; internal set; }

        internal void CleanHighlight()
            => image.color = _defaultColor;

        internal void Highlight()
            => image.color = _highlightColor;

    }
}