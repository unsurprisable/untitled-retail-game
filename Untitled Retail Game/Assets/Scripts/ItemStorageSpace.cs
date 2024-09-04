using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStorageSpace : InteractableObject
{
    public enum StorageType { SHELF, CLOSED_FRIDGE, OPEN_FRIDGE, FREEZER, PRODUCE_BIN, WARMER }

    [Space]
    [Header("Item Storage Space")]
    [SerializeField] private StorageType storageType;

    [Space]

    [SerializeField] private StoreItemSO storeItem;
    [SerializeField] private int itemAmount;

    public override void OnHovered()
    {
        // eventually, display a UI element with info about this storage space
        string itemName = storeItem != null ? storeItem.itemName : "null";
        Debug.Log("Storage Space of: " + itemName + ", amount: " + itemAmount);
    }
    public override void OnUnhovered()
    {
        // hide that UI element
    }

    public override void OnInteract(PlayerController player)
    {
        if (player.GetHeldItem() != null && player.GetHeldItem() is Container container) {
            if (container.IsEmpty()) {
                Debug.Log("that container is empty, silly!");
                return;
            }
            if (storeItem == null || itemAmount == 0) {
                // empty storage space; override it
                storeItem = container.GetStoreItemSO();
            }
            if (container.GetStoreItemSO() != storeItem) {
                Debug.Log("wrong storage space, silly! those are different items!");
                return;
            } 

            // place the item in storage
            container.TakeItem();
            itemAmount++;
            Debug.Log("added 1x " + container.GetStoreItemSO().itemName + "! new amount: " + itemAmount);
        }
    }
}
