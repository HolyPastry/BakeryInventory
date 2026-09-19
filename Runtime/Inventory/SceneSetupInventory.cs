using System;
using System.Collections;
using System.Collections.Generic;
using Bakery.Flow;
using UnityEngine;


namespace Bakery
{

    public class SceneSetupInventory : SceneSetupScript
    {
        [Serializable]
        public struct GridAmount
        {
            public GridInfo grid;
            public int amount;
            public bool stackable;
        }
        [SerializeField] private ContainerInfo _inventoryInfo;
        [SerializeField] private List<GridAmount> _inventoryItems;
        public override IEnumerator Routine()
        {
            yield return FlowServices.WaitUntilReady();
            yield return Inventory.Grids().WaitUntilReady;
            if(_inventoryItems == null)
            {
                Debug.LogWarning("Inventory items list is null.");
                yield break;
            }
            foreach (var gridAmount in _inventoryItems)
            {
                if (gridAmount.grid == null)
                {
                    Debug.LogWarning("Grid is null for one of the inventory items.");
                    continue;
                }
                Inventory.Grids().Create(_inventoryInfo,
                                gridAmount.grid,
                                gridAmount.amount,
                                gridAmount.stackable);
            }

        }
    }
}