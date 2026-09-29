using System;
using UnityEngine;

namespace Bakery
{
    public class InventoryUISpawner : MonoBehaviour
    {
        [SerializeField]
        private GridObjectUI _gridObjectUIPrefab;

        [SerializeField]
        private GridObjectUI _lockedGridUIPrefab;

        public static Func<RectTransform, RotatableGrid, GridObjectUI> Spawn = (parent, grid) =>
            null;

        void Awake()
        {
            Spawn = SpawnHandler;
        }

        void OnDestroy()
        {
            Spawn = (parent, grid) => null;
        }

        protected virtual GridObjectUI SpawnHandler(RectTransform parent, RotatableGrid grid)
        {
            var prefab = grid.Locked ? _lockedGridUIPrefab : _gridObjectUIPrefab;
            var gridObjectUI = Instantiate(prefab, parent);
            gridObjectUI.Initialize(grid);
            return gridObjectUI;
        }

        public static void Destroy(GridObjectUI gridObjectUI)
        {
            if (gridObjectUI == null)
                return;
            gridObjectUI.transform.SetParent(null);
            Destroy(gridObjectUI.gameObject);
        }
    }
}
