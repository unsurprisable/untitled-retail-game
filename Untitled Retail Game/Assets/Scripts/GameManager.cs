using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnBalanceChanged;

    [SerializeField] private StoreItemListSO storeItemList;
    private Dictionary<StoreItemSO, float> itemPriceDict;

    [SerializeField] private float storeBalance;


    private void Awake()
    {
        Instance = this;

        itemPriceDict = new Dictionary<StoreItemSO, float>();
        foreach (StoreItemSO storeItemSO in storeItemList.list) {
            itemPriceDict[storeItemSO] = storeItemSO.unitPrice;
        }
    }

    private void Start()
    {
        OnBalanceChanged?.Invoke(this, EventArgs.Empty);
    }

    public float GetBalance() {
        return storeBalance;
    }

    public void SetBalance(float newBalance) {
        storeBalance = newBalance;
        OnBalanceChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveFromBalance(float cost) {
        storeBalance -= cost;
        OnBalanceChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool CanAfford(float price) {
        return storeBalance - price >= 0f;
    }

    public StoreItemSO[] GetStoreItemSOList() {
        return storeItemList.list;
    }

    public float GetStoreItemPrice(StoreItemSO storeItemSO) {
        return itemPriceDict[storeItemSO];
    }

    public void SetStoreItemPrice(StoreItemSO storeItemSO, float newPrice) {
        itemPriceDict[storeItemSO] = newPrice;
    }
}
