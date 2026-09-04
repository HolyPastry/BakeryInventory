using UnityEngine;

namespace Bakery
{
    public class InventorySpawner : MonoBehaviour
    {
        [SerializeField] private GridObjectUI _gridObjectUIPrefab;
        [SerializeField] private GridObjectUI _lockedGridUIPrefab;

        public GridObjectUI Spawn(RectTransform parent,
                    RotatableGrid grid)
        {
            var prefab = grid.Locked ? _lockedGridUIPrefab : _gridObjectUIPrefab;
            var gridObjectUI = Instantiate(prefab, parent);
            gridObjectUI.Initialize(grid);
            return gridObjectUI;
        }

        public static void Destroy(GridObjectUI gridObjectUI)
        {
            if (gridObjectUI == null) return;
            Destroy(gridObjectUI.gameObject);
        }
    }
}