using UnityEngine;

namespace Bakery
{
    public interface IInventorySpawner
    {
        void Destroy(GridObjectUI gridObjectUI);
        GridObjectUI Spawn(RectTransform parent,
                            RotatableGrid grid,
                            Vector2Int cellSize);
    }
}