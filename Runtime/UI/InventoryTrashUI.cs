using UnityEngine;
using UnityEngine.Events;

namespace Bakery
{
    [RequireComponent(typeof(RectTransform))]
    public class InventoryTrashUI : MonoBehaviour
    {
        [SerializeReference] private InventoryHand _hand;

        public bool IsHovering => _isHovering;

        public UnityEvent OnGridObjectEnter;
        public UnityEvent OnGridObjectExit;
        public UnityEvent OnTrash;
        private RectTransform _rectTransform => (RectTransform)transform;

        private bool _isHovering;

        public void Trash()
        {
            OnTrash?.Invoke();
        }

        void Start()
        {
            Inventory.Controller().RegisterTrash(this);
        }

        void Update()
        {
            bool isHovering = _hand.Hovering(_rectTransform);
            if (_hand.IsEmpty || !isHovering)
            {
                if (_isHovering)
                {
                    _isHovering = false;
                    OnGridObjectExit?.Invoke();
                }
                return;
            }


            if (isHovering && !_isHovering)
            {
                _isHovering = true;
                OnGridObjectEnter?.Invoke();
            }
        }
    }
}
