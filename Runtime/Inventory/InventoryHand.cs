using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Bakery
{
    public class InventoryHand : MonoBehaviour, ICursorAttachable
    {
        private GridObjectUI _handedGridObject;

        public void UpdatePosition(Vector2 position)
        {
            transform.position = position;
        }

        internal void Grab(GridObjectUI gridObjectUI)
        {
            gridObjectUI.Grab();
            _handedGridObject = gridObjectUI;
            User.Cursor().Attach(this);
            StartCoroutine(DelayedAttach(gridObjectUI));
        }

        private IEnumerator DelayedAttach(GridObjectUI gridObjectUI)
        {
            yield return new WaitForEndOfFrame();
            gridObjectUI.transform.SetParent(this.transform, true);
            gridObjectUI.transform.localPosition =
                new Vector2(-gridObjectUI.Size.x / 2, gridObjectUI.Size.y);
        }

        public GridObjectUI Release()
        {
            User.Cursor().Detach(this);
            if (_handedGridObject != null)
                _handedGridObject.Release();
            var handedGridObject = _handedGridObject;
            _handedGridObject = null;
            return handedGridObject;
        }
    }
}
