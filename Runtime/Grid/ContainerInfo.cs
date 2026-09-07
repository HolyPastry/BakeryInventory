using UnityEngine;

namespace Bakery
{
    [CreateAssetMenu(fileName = "New Container Info", menuName = "Bakery/Inventory/Container Info")]
    public class ContainerInfo : BaseInventoryInfo
    {
        public bool IsPersistent;
    }

}