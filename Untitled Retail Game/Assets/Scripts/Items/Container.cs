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
    public void SetStoreItemSO(StoreItemSO storeItemSO, bool startEmpty = false)
    {
        storeItem = storeItemSO;

        transform.name = "Container (" + storeItem.name + ")";
        foreach(SpriteRenderer r in iconObjects) {
            r.sprite = storeItem.icon;
        }

        itemAmount = storeItem.containerAmount;
    }

    public bool IsEmpty() {
        return itemAmount == 0;
    }
    public bool IsFull() {
        return itemAmount == storeItem.containerAmount;
    }

    public StoreItemSO GetStoreItemSO() {
        return storeItem;
    }

    public void RemoveItem() {
        itemAmount--;
        if (itemAmount < 0) {
            Debug.LogError(storeItem.name + " container is storing a negative amount of items..?");
        }
    }
    public void AddItem() {
        itemAmount++;
    }
}
