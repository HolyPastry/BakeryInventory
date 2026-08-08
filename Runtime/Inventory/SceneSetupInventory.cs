using System.Collections;
using System.Collections.Generic;
using Bakery.Flow;
using UnityEngine;


namespace Bakery
{
    public class SceneSetupInventory : SceneSetupScript
    {
        [SerializeField] private GridInfo _inventoryInfo;
        [SerializeField] private List<GridInfo> _inventoryItems;
        public override IEnumerator Routine()
        {
            yield return FlowServices.WaitUntilReady();
            yield return Inventory.Grids().WaitUntilReady;
            Inventory.Grids().Create(_inventoryInfo, _inventoryItems);
        }
    }
}