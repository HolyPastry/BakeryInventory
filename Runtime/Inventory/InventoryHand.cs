using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Bakery
{
    public class InventoryHand : MonoBehaviour, ICursorAttachable
    {
        public GridObjectUI GrabbedObject { get; private set; } = null;

        public bool IsEmpty => GrabbedObject == null;



        public void UpdatePosition(Vector2 position)
        {
            transform.position = position;
        }

        internal void Grab(GridObjectUI gridObjectUI)
        {
            if (GrabbedObject != null &&
                GrabbedObject != gridObjectUI)
                throw new InvalidOperationException("Hand is already holding an object.");


            gridObjectUI.Grab();
            GrabbedObject = gridObjectUI;
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
            if (GrabbedObject != null)
                GrabbedObject.Release();
            var handedGridObject = GrabbedObject;
            GrabbedObject = null;
            return handedGridObject;
        }

        internal bool CanGrab(RotatableGrid hoveredObject)
        {
            if (hoveredObject == null) return false;
            if (GrabbedObject == null) return true;
            if (GrabbedObject.Grid != hoveredObject) return false;

            if (GrabbedObject.FullStack) return false;

            return true;
        }

        internal void ModifyStack(int amount)
        {
            if (GrabbedObject == null) return;
            GrabbedObject.Grid.Stack += amount;
            GrabbedObject.UpdateStack();
            if (GrabbedObject.Grid.Stack <= 0)
                Release();
        }

        internal int NumCanGrab(RotatableGrid hoveredObject, int numToGrab)
        {
            if (hoveredObject == null) return 0;
            if (GrabbedObject == null) return numToGrab;
            if (GrabbedObject.Grid != hoveredObject) return 0;

            return Math.Min(numToGrab, GrabbedObject.MaxStack - GrabbedObject.Stack);
        }
    }
}
