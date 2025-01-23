
public static class ItemInteractions
{
    public static bool HeldItemCanInteractWithOther(HoldableItem heldItem, IInteractableObject other) {
        if (

            // compatibility matrix
            (heldItem is Container 
                && other is StorageVolume) ||
            (heldItem is ItemScannerItem
                && other is StorageVolume)

        ) return true;
        return false;
    }
}
