using System.Collections.Generic;
using UnityEngine;

public class ItemStorageSpace : InteractableObject
{
    public enum StorageType { SHELF, CLOSED_FRIDGE, OPEN_FRIDGE, FREEZER, PRODUCE_BIN, WARMER }

    [Space]
    [Header("Item Storage Space")]
    [SerializeField] private StorageType storageType;
    [SerializeField] private Transform displayOrigin;

    [Space]

    [SerializeField] private StoreItemSO storeItem;
    [SerializeField] private int itemAmount;

    private Stack<Transform> itemDisplayStack = new Stack<Transform>();

    // FOR TESTING ONLY:
    // auto-fills the drawer if it is pre-set with a StoreItemSO
    private void Start()
    {
        if (storeItem != null) {
            Debug.Log("a testing drawer was activated and filled.");
            for (int i = 0; i < storeItem.storageAmount; i++) {
                AddItem();
            }
        }
    }

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
            if (itemAmount == storeItem.storageAmount) {
                Debug.Log("you silly goose, this storage is full!");
                return;
            }

            // place the item in storage
            AddItem(container);
        }
    }

    public override void OnInteractAlternate(PlayerController player)
    {
        if (itemAmount > 0) {
            RemoveItem();
        }
    }

    private void AddItemToDisplay()
    {
        Transform itemDisplay = Instantiate(storeItem.prefab);
        itemDisplayStack.Push(itemDisplay);

        // move them away from the corner
        Vector3 addedPosition = new Vector3(storeItem.modelDimensions.x/2, 0f, -storeItem.modelDimensions.z/2);


        // shift each object based on existing ones... how fun :)
        addedPosition.x += storeItem.modelDimensions.x * itemAmount;

        int horizontalWraps = Mathf.FloorToInt(itemAmount / storeItem.storageCapacity.x);
        addedPosition.x -= horizontalWraps * storeItem.modelDimensions.x * storeItem.storageCapacity.x;
        addedPosition.z -= horizontalWraps * storeItem.modelDimensions.z;

        int verticalWraps = Mathf.FloorToInt(itemAmount / (storeItem.storageCapacity.x * storeItem.storageCapacity.z));
        addedPosition.z += verticalWraps * storeItem.modelDimensions.z * storeItem.storageCapacity.z;
        addedPosition.y += verticalWraps * storeItem.modelDimensions.y;


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

    private void AddItem(Container container = null) 
    {
        if (container != null) container.TakeItem();
        AddItemToDisplay();
        itemAmount++;
    }

    private void  RemoveItem() 
    {
        RemoveItemFromDisplay();
        itemAmount--;
    }
}
