using UnityEngine;

[CreateAssetMenu()]
public class StoreItemSO : ScriptableObject
{

    [Space]

    [Tooltip("The item's display name.")]
    public string itemName;
    [Tooltip("The item's icon (make sure it's 128x128).")]
    public Sprite icon; // 128x128 for consistent sizing!!!

    [Space]
    
    [Tooltip("The type of storage this item requires.")]
    public ItemStorageSpace.StorageType storageType;

    [Space]

    [Tooltip("Amount of this item that a container can hold.")]
    public int containerSize;
    [Tooltip("Amount of this item that a shelf space can hold.")]
    public int shelfSize;
    [Tooltip("Price per unit (not per stack).")]
    public float unitPrice;
}
