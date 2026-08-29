using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bakery
{
    public class TestLockGrid : MonoBehaviour
    {
        [Serializable]
        public struct LockInfo
        {
            public Vector2Int Position;
            public GridInfo GridInfo;
        }

        [SerializeField] private List<LockInfo> _locks = new();
        [SerializeField] private GridInfo _inventory;



        [ContextMenu("Add Lock")]
        void AddLock()
        {
            foreach (var lockInfo in _locks)
            {
                Inventory.Grids().TryPlaceAt(new RotatableGrid(lockInfo.GridInfo),
                                    _inventory,
                                    lockInfo.Position, 1, out _);
            }

        }
    }
}
