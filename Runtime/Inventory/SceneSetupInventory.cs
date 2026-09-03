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
        }
        [SerializeField] private GridInfo _inventoryInfo;
        [SerializeField] private List<GridAmount> _inventoryItems;
        public override IEnumerator Routine()
        {
            yield return FlowServices.WaitUntilReady();
            yield return Inventory.Grids().WaitUntilReady;
            foreach (var gridAmount in _inventoryItems)
            {
                Inventory.Grids().Create(_inventoryInfo, gridAmount.grid, gridAmount.amount);
            }

        }
    }
}