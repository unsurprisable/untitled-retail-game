using System.Collections.Generic;
using UnityEngine;

public class StorageVolume : InteractableObject
{
    public enum StorageType { ITEM_RACK, CLOSED_FRIDGE, OPEN_FRIDGE, FREEZER, PRODUCE_BIN, WARMER }

    [Space]
    [Header("Item Storage Space")]
    [SerializeField] private StorageType storageType;
    [SerializeField] private Transform displayOrigin;

    [Space]

    [SerializeField] private StoreItemSO storeItemSO;
    [SerializeField] private int itemAmount;

    private Stack<Transform> itemDisplayStack = new Stack<Transform>();


    // FOR TESTING ONLY:
    // auto-fills the drawer if it is pre-set with a StoreItemSO
    private void Awake()
    {
        if (storeItemSO != null) {
            Debug.Log("a testing drawer was activated and filled.");
            for (int i = 0; i < storeItemSO.storageAmount; i++) {
                AddItem();
            }
        }
    }

    public override void OnHovered()
    {
        if (storeItemSO == null) return;

        StorageVolumeUI.Instance.UpdateInfo(storeItemSO, itemAmount);
        StorageVolumeUI.Instance.Show();

        if (PlayerController.Instance.GetHeldItem() is ItemScannerItem scanner) {
            scanner.SetStoreItemSO(storeItemSO);
        }
    }
    public override void OnUnhovered()
    {
        StorageVolumeUI.Instance.Hide();

        if (PlayerController.Instance.GetHeldItem() is ItemScannerItem scanner) {
            scanner.ResetStoreItemSO();
        }
    }

    public override void OnInteract(PlayerController player)
    {
        if (player.GetHeldItem() != null && player.GetHeldItem() is Container container) {
            if (container.IsEmpty()) {
                Debug.Log("that container is empty, silly!");
                return;
            }
            if (container.GetStoreItemSO().storageType != storageType) {
                Debug.Log("oh you silly billy! this isn't a " + container.GetStoreItemSO().storageType.ToString() + "!");
                return;
            }
            if (storeItemSO == null || itemAmount == 0) {
                // empty storage space; override it
                storeItemSO = container.GetStoreItemSO();
            }
            if (container.GetStoreItemSO() != storeItemSO) {
                Debug.Log("wrong storage space, silly! those are different items!");
                return;
            }
            if (itemAmount == storeItemSO.storageAmount) {
                Debug.Log("you silly goose, this storage is full!");
                return;
            }

            // place the item in storage
            AddItem();
            container.RemoveItem();
            StorageVolumeUI.Instance.UpdateInfo(storeItemSO, itemAmount);
            StorageVolumeUI.Instance.Show();
        }
    }

    public override void OnInteractSecondary(PlayerController player)
    {
        if (itemAmount > 0 && player.GetHeldItem() is Container container) {
            if (container.IsFull() || container.GetStoreItemSO() != storeItemSO) return;
            RemoveItem();
            container.AddItem();
            StorageVolumeUI.Instance.UpdateInfo(storeItemSO, itemAmount);
            StorageVolumeUI.Instance.Show();
        }
    }

    private void AddItemToDisplay()
    {
        Transform itemDisplay = Instantiate(storeItemSO.prefab);
        itemDisplayStack.Push(itemDisplay);

        // move them away from the corner
        Vector3 addedPosition = new Vector3(storeItemSO.modelDimensions.x/2, 0f, -storeItemSO.modelDimensions.z/2);


        // shift each object based on existing ones... how fun :)
        addedPosition.x += storeItemSO.modelDimensions.x * itemAmount;

        int horizontalWraps = Mathf.FloorToInt(itemAmount / storeItemSO.storageCapacity.x);
        addedPosition.x -= horizontalWraps * storeItemSO.modelDimensions.x * storeItemSO.storageCapacity.x;
        addedPosition.z -= horizontalWraps * storeItemSO.modelDimensions.z;

        int verticalWraps = Mathf.FloorToInt(itemAmount / (storeItemSO.storageCapacity.x * storeItemSO.storageCapacity.z));
        addedPosition.z += verticalWraps * storeItemSO.modelDimensions.z * storeItemSO.storageCapacity.z;
        addedPosition.y += verticalWraps * storeItemSO.modelDimensions.y;


        // rotate the addedPosition relative to the Y axis rotation of the storage object
        // (so that these translations will always be relative to the object, regardless of its rotation)
        float theta = -transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
        addedPosition = new Vector3(
            addedPosition.x * Mathf.Cos(theta) - addedPosition.z * Mathf.Sin(theta), addedPosition.y,
            addedPosition.x * Mathf.Sin(theta) + addedPosition.z * Mathf.Cos(theta)
        );

        Vector3 itemPosition = displayOrigin.position + addedPosition;

        itemDisplay.SetPositionAndRotation(itemPosition + itemDisplay.transform.position, Quaternion.Euler(0f, transform.rotation.eulerAngles.y + 180f, 0f));
    }

    private void RemoveItemFromDisplay()
    {
        Destroy(itemDisplayStack.Pop().gameObject);
    }

    private void AddItem() 
    {
        AddItemToDisplay();
        itemAmount++;
    }

    private void  RemoveItem() 
    {
        RemoveItemFromDisplay();
        itemAmount--;
        if (itemAmount < 0) {
            Debug.LogError("Storage volume is storing a negative amount of items..?");
        }
    }
}
