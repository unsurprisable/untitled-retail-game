using System;
using System.Collections.Generic;
using UnityEngine;

public class ProductDisplayObject : BuildObject
{
    [Header("Product Display Object")]
    [SerializeField] private StorageVolume[] storageVolumes;
    private Dictionary<StoreItemSO, int> itemAmounts;
    public Transform viewingArea;


    protected override void OnAwake()
    {
        foreach (StorageVolume storageVolume in storageVolumes) {
            Register(storageVolume);
        }
        FetchItemAmounts();
    }

    protected override void OnOnNetworkSpawn()
    {
        StoreManager.Instance.Register(this);
    }

    private void FetchItemAmounts() {
        Dictionary<StoreItemSO, int> itemAmounts = new Dictionary<StoreItemSO, int>();
        foreach (StorageVolume storageVolume in storageVolumes) {
            StoreItemSO storeItemSO = storageVolume.GetStoreItemSO();
            if (storeItemSO == null) continue;

            itemAmounts[storeItemSO] = itemAmounts.GetValueOrDefault(storeItemSO, 0) + storageVolume.GetItemAmount();
        }
        this.itemAmounts = itemAmounts;
    }

    public void Register(StorageVolume storageVolume) {
        storageVolume.OnStoreItemChanged += StorageVolume_OnStoreItemChanged;
        storageVolume.OnItemAmountChanged += StorageVolume_OnItemAmountChanged;
    }

    private void StorageVolume_OnStoreItemChanged(object sender, EventArgs e)
    {
        FetchItemAmounts();
    }

    private void StorageVolume_OnItemAmountChanged(object sender, StorageVolume.OnItemAmountChangedEventArgs e)
    {
        StorageVolume storageVolume = sender as StorageVolume;
        StoreItemSO storeItemSO = storageVolume.GetStoreItemSO();

        itemAmounts[storeItemSO] = itemAmounts.GetValueOrDefault(storeItemSO, 0) + (e.newAmount - e.previousAmount);
    }



    public int GetStoreItemAmount(StoreItemSO storeItemSO) {
        if (!itemAmounts.ContainsKey(storeItemSO)) {
            return 0;
        }
        return itemAmounts[storeItemSO];
    }

    public Dictionary<StoreItemSO, int> GetStoreItemDictionary() {
        return new Dictionary<StoreItemSO, int>(itemAmounts);
    }
}
