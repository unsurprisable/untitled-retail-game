using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : HoldableItem
{
    [Space]
    [Header("Container")]
    [SerializeField] private StoreItemSO storeItem;
    [SerializeField] private SpriteRenderer[] iconObjects;
    [SerializeField] private int itemAmount;
    
    // FOR TESTING!!!!
    private void Awake() {
        if (storeItem != null) {
            SetStoreItemSO(storeItem);
        }
    }

    // make sure to set this instantly whenever instantiating a new container
    public void SetStoreItemSO(StoreItemSO storeItemSO)
    {
        storeItem = storeItemSO;
        foreach(SpriteRenderer r in iconObjects)
        {
            r.sprite = storeItem.icon;
        }
        transform.name = "Container (" + storeItem.itemName + ")";
    }

    public bool IsEmpty() {
        return itemAmount <= 0;
    }

    public StoreItemSO GetStoreItemSO() {
        return storeItem;
    }

    public void TakeItem() {
        itemAmount--;
        if (itemAmount < 0) {
            Debug.LogError(storeItem.itemName + " container is storing a negative amount of items..?");
        }
    }
}
