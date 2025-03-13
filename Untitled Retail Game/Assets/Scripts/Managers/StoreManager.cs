using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    // POSSIBLE WARNING:
    // this whole class is just managed per-client since it works with a lot of unserializable types.
    // i don't know the full extent of how this class is going to be used in the future, but for right now this seems fine.
    // (the server will be the one handling customer behavior so clients should only use this class to semi-reliably track the products that the store sells)

    public static StoreManager Instance { get; private set; }

    public enum StorageType { ITEM_RACK, CLOSED_FRIDGE, OPEN_FRIDGE, FREEZER, PRODUCE_BIN, WARMER }

    private Dictionary<StoreItemSO, int> storeItemData;

    // registered stations
    private List<StorageVolume> registeredStorageVolumes;
    // private List<...> registeredCheckouts;
    // private List<...> registeredInventoryStorages;

    public void Awake() {
        Instance = this;

        registeredStorageVolumes = new List<StorageVolume>();
    }

    // StorageVolumes SHOULD be calling this when they're spawned on the network
    public void Register(StorageVolume storageVolume) {
        registeredStorageVolumes.Add(storageVolume);
    }

    private void RetrieveStoreItemData() {
        Dictionary<StoreItemSO, int> storeItemData = new Dictionary<StoreItemSO, int>();        

        foreach (StorageVolume volume in registeredStorageVolumes) {
            if (volume.GetStoreItemSO() == null) continue;
            storeItemData[volume.GetStoreItemSO()] = storeItemData.GetValueOrDefault(volume.GetStoreItemSO(), 0) + volume.GetItemAmount();
        }

        this.storeItemData = storeItemData;
    }

    private string FormatItemDataDictionary(Dictionary<StoreItemSO, int> dictionary) {
        StringBuilder formattedDict = new StringBuilder("{");
        foreach (StoreItemSO itemSO in dictionary.Keys) {
            if (formattedDict.Length > 1) formattedDict.Append(", ");
            formattedDict.Append($"{itemSO.name}: {dictionary[itemSO]}");
        }
        formattedDict.Append("}");
        return formattedDict.ToString();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) {
            RetrieveStoreItemData();
            Debug.Log(FormatItemDataDictionary(storeItemData));
        }
    }

}
