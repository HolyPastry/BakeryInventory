using UnityEngine;

namespace Bakery
{
    public class InventorySpawner : MonoBehaviour, IInventorySpawner
    {
        [SerializeField] private GridObjectUI _gridObjectUIPrefab;

        void OnEnable()
        {
            Inventory.Spawner = () => this;
        }

        void OnDisable()
        {
            Inventory.Spawner = Inventory.UnregisterSpawner;
        }

        public GridObjectUI Spawn(RectTransform parent,
                    RotatableGrid grid,
                    Vector2Int cellSize)
        {
            var gridObjectUI = Instantiate(_gridObjectUIPrefab, parent);
            gridObjectUI.Initialize(grid, cellSize);
            return gridObjectUI;
        }

        public void Destroy(GridObjectUI gridObjectUI)
        {
            if (gridObjectUI == null) return;
            Destroy(gridObjectUI.gameObject);
        }
    }
}