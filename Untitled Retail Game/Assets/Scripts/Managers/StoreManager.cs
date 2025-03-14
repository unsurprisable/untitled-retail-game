using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

public class StoreManager : NetworkBehaviour
{
    // POSSIBLE WARNING:
    // this whole class is just managed per-client since it works with a lot of unserializable types.
    // i don't know the full extent of how this class is going to be used in the future, but for right now this seems fine.
    // (the server will be the one handling customer behavior so clients should only use this class to semi-reliably track the products that the store sells)

    public static StoreManager Instance { get; private set; }

    public enum StorageType { ITEM_RACK, CLOSED_FRIDGE, OPEN_FRIDGE, FREEZER, PRODUCE_BIN, WARMER }

    public event EventHandler OnStoreOpenChanged;

    // [Header("Logistics")]
    private Dictionary<StoreItemSO, int> storeItemData;
    private List<StorageVolume> registeredStorageVolumes;
    // private List<...> registeredCheckouts;
    // private List<...> registeredInventoryStorages;

    [Header("Store Properties")]
    [SerializeField] private bool isOpen;

    [Space]
    [Header("Customers")]
    [SerializeField] private Transform storeSpawnPoint;
    [SerializeField] private Transform customerPrefab;
    [SerializeField] private float customerCooldown;
    [SerializeField] private float customerCooldownLeft;
    private HashSet<StoreItemSO> availableProducts;


    public void Awake() {
        Instance = this;

        registeredStorageVolumes = new List<StorageVolume>();
        
        availableProducts = new HashSet<StoreItemSO>();
    }

    void Start()
    {
        StartCoroutine(InitializeRegisteredData());

        // temp until i figure out how this is actually gonna be determined
        // (for now customers will just look for every item that exists in the game automatically)
        foreach (StoreItemSO storeItemSO in SerializeManager.Instance.GetStoreItemListSO().list) {
            availableProducts.Add(storeItemSO);
        }
    }

    // StorageVolumes SHOULD be calling this when they're spawned on the network
    public void Register(StorageVolume storageVolume) {
        registeredStorageVolumes.Add(storageVolume);
        // if (storageVolume.GetStoreItemSO() != null) {
        //     availableProducts.Add(storageVolume.GetStoreItemSO());
        // }
        Debug.Log("Registered StorageVolume: " + storageVolume.name); 
    }

    // runs one frame AFTER start to make sure every scene object has registered itself by the time this runs
    private IEnumerator InitializeRegisteredData() {
        yield return null; // wait for one frame

        RetrieveStoreItemData();
        Debug.Log(FormatItemDataDictionary(storeItemData));
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

    public void OpenStore() {
        if (isOpen) return;
        isOpen = false;
        OnStoreOpenChanged?.Invoke(this, EventArgs.Empty);
    }
    public void CloseStore() {
        if (!isOpen) return;
        isOpen = false;
        OnStoreOpenChanged?.Invoke(this, EventArgs.Empty);
    }



    void Update() {
        customerCooldownLeft -= Time.deltaTime;
        if (customerCooldownLeft <= 0) {
            NetworkManager.SpawnManager.InstantiateAndSpawn(customerPrefab.GetComponent<NetworkObject>(), destroyWithScene: true, position: storeSpawnPoint.position);
            customerCooldownLeft = customerCooldown;
        }
    }

}
