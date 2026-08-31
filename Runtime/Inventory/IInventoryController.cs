using System.Threading;

namespace Bakery
{
    public interface IInventoryController
    {
        void RegisteredHand(InventoryHand hand);
        void UnregisterHand(InventoryHand hand);

        void RegisterTrash(InventoryTrashUI trash);
        void UnregisterTrash(InventoryTrashUI trash);

    }
}
