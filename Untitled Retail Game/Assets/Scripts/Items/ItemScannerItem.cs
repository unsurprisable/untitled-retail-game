using System;
using UnityEngine;

public class ItemScannerItem : HoldableItem
{
    [Header("Item Scanner")]
    [SerializeField] private TMPro.TextMeshPro itemName;
    [SerializeField] private TMPro.TextMeshPro stockPrice;
    [SerializeField] private TMPro.TextMeshPro sellPrice;

    private StoreItemSO storeItemSO;
    private float newItemPrice;


    public override void OnUseSecondary(PlayerController player)
    {
        if (storeItemSO == null) return;

        newItemPrice = storeItemSO.unitPrice;
        SetNewItemPrice();
    }

    public override void OnDrop()
    {
        ResetStoreItemSO();
    }

    public void SetStoreItemSO(StoreItemSO storeItemSO)
    {
        this.storeItemSO = storeItemSO;
        itemName.text = storeItemSO.name;
        stockPrice.text = "$" + storeItemSO.unitPrice.ToString("0.00");
        sellPrice.text = "$" + GameManager.Instance.GetStoreItemPrice(storeItemSO).ToString("0.00");
        newItemPrice = GameManager.Instance.GetStoreItemPrice(storeItemSO);

        GameInput.Instance.OnScroll += HandleScrollWheel;
    }

    public void ResetStoreItemSO()
    {
        this.storeItemSO = null;
        itemName.text = "";
        stockPrice.text = "$0.00";
        sellPrice.text = "$0.00";
        newItemPrice = 0;
        
        GameInput.Instance.OnScroll -= HandleScrollWheel;
    }

    private void SetNewItemPrice()
    {
        GameManager.Instance.SetStoreItemPrice(storeItemSO, newItemPrice);
        sellPrice.text = "$" + newItemPrice.ToString("0.00");
    }

    private void HandleScrollWheel(object sender, EventArgs e)
    {
        if (storeItemSO == null) {
            Debug.LogWarning("A scanner was listening to scroll inputs but didn't have a StoreItemSO!");
            return;
        }

        float priceDelta = GameInput.Instance.GetIsSprinting() ? 0.1f : 0.01f;
        
        int scrollDirection = GameInput.Instance.GetScrollDirection();
        if (newItemPrice == 0f && scrollDirection == -1) return;

        newItemPrice += priceDelta * scrollDirection;
        if (newItemPrice < 0f) newItemPrice = 0f;
        SetNewItemPrice();
    }
}
