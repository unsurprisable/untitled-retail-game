using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnBalanceChanged;

    public event EventHandler<OnItemPriceChangedEventArgs> OnItemPriceChanged;
    public class OnItemPriceChangedEventArgs : EventArgs {
        public StoreItemSO storeitemSO;
        public float newItemPrice;
    }

    [SerializeField] private StoreItemListSO storeItemList;

    private Dictionary<StoreItemSO, int> itemToIdDict;
    private Dictionary<int, float> itemPriceDict;

    public float initialStoreBalance;
    private NetworkVariable<float> storeBalance = new NetworkVariable<float>();



    private void Awake()
    {
        Instance = this;

        itemToIdDict = new Dictionary<StoreItemSO, int>();
        for (int id = 0; id < storeItemList.list.Length; id++) {
            itemToIdDict[storeItemList.list[id]] = id;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) {
            NetworkManager.Singleton.SceneManager.OnSynchronize += NetworkManager_OnSynchronize;

            itemPriceDict = new Dictionary<int, float>();
            for (int id = 0; id < storeItemList.list.Length; id++) {
                itemPriceDict[id] = storeItemList.list[id].unitPrice;
            }

            storeBalance.Value = initialStoreBalance;
        }

        storeBalance.OnValueChanged += (preValue, newValue) => {
            OnBalanceChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnSynchronize(ulong clientId)
    {
        SynchronizeItemPricesRpc(itemPriceDict.Values.ToArray(), RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SynchronizeItemPricesRpc(float[] values, RpcParams rpcParams)
    {
        Debug.Log("synching dictionary");
        itemPriceDict = new Dictionary<int, float>();
        for (int id = 0; id < values.Length; id++) {
            // don't need to pass in keys because the Server itemPriceDict values are created in the same way these are; the order will be the same either way
            itemPriceDict[id] = values[id];
            Debug.Log("ID " + id + ": " + values[id]);
        }
    }


    public float GetBalance() {
        return storeBalance.Value;
    }

    public void SetBalance(float newBalance) {
        storeBalance.Value = newBalance;
    }

    public void RemoveFromBalance(float cost) {
        storeBalance.Value -= cost;
    }

    public bool CanAfford(float price) {
        return storeBalance.Value - price >= 0f;
    }

    public StoreItemSO[] GetStoreItemSOList() {
        return storeItemList.list;
    }

    public float GetStoreItemPrice(StoreItemSO storeItemSO) {
        int id = itemToIdDict[storeItemSO];
        Debug.Log("ID: " + id);
        Debug.Log(itemToIdDict != null);
        return itemPriceDict[itemToIdDict[storeItemSO]];
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetStoreItemPriceRpc(int storeItemId, float newItemPrice) {
        itemPriceDict[storeItemId] = newItemPrice;
        OnItemPriceChanged?.Invoke(this, new OnItemPriceChangedEventArgs {
            storeitemSO = GetStoreItemFromId(storeItemId),
            newItemPrice = newItemPrice
        });
    }

    public int GetStoreItemId(StoreItemSO storeItemSO)
    {
        return itemToIdDict[storeItemSO];
    }

    public StoreItemSO GetStoreItemFromId(int id)
    {
        return storeItemList.list[id];
    }

    public int[] ConvertStoreItemArrayToId(StoreItemSO[] storeItemSOArray)
    {
        int[] idArray = new int[storeItemSOArray.Length];
        for (int i = 0; i < idArray.Length; i++)
        {
            idArray[i] = itemToIdDict[storeItemSOArray[i]];
        }
        return idArray;
    }

    public StoreItemSO[] ConvertIdArrayToStoreItem(int[] idArray)
    {
        StoreItemSO[] storeItemSOArray = new StoreItemSO[idArray.Length];
        for (int i = 0; i < idArray.Length; i++)
        {
            storeItemSOArray[i] = storeItemList.list[idArray[i]];
        }
        return storeItemSOArray;
    }
}
