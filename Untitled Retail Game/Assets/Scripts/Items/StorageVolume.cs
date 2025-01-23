using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StorageVolume : InteractableNetworkObject
{
    public enum StorageType { ITEM_RACK, CLOSED_FRIDGE, OPEN_FRIDGE, FREEZER, PRODUCE_BIN, WARMER }

    [Space]
    [Header("Item Storage Space")]
    [SerializeField] private StorageType storageType;
    [SerializeField] private Transform displayOrigin;

    [Space]

    [SerializeField] private StoreItemSO storeItemSO;
    private NetworkVariable<int> itemAmount = new NetworkVariable<int>();

    private Stack<Transform> itemDisplayStack = new Stack<Transform>();

    [Space]
    [Header("Interaction")]
    [SerializeField] private AnimationCurve interactHeldCooldownCurve; // should probably make this static somewhere to save memory (rn every single volume has one of these in memory)
    private float interactHeldCooldownLeft;




    public override void OnNetworkSpawn()
    {
        if (IsServer) {
            NetworkManager.Singleton.SceneManager.OnSynchronize += NetworkManager_OnSynchronize;
        }

        itemAmount.OnValueChanged += UpdateVisualForItemAmountChange;

        if (IsServer && storeItemSO != null) {
            itemAmount.Value = storeItemSO.storageAmount;
        }
    }

    private void NetworkManager_OnSynchronize(ulong clientId)
    {
        if (storeItemSO == null) return;
        SynchronizeItemDataRpc(storeItemSO.Id, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SynchronizeItemDataRpc(int storeItemId, RpcParams rpcParams)
    {
        storeItemSO = StoreItemSO.FromId(storeItemId);

        UpdateVisualForItemAmountChange(0, itemAmount.Value);
    }

    private void UpdateVisualForItemAmountChange(int preValue, int newValue)
    {
            if (preValue == newValue) return;
            if (preValue < newValue) {
                for (int i = preValue; i < newValue; i++) {
                    AddItemToDisplay(i);
                }
            }
            if (preValue > newValue) {
                for (int i = preValue; i > newValue; i--) {
                    RemoveItemFromDisplay();
                }
            }
            UpdateStorageVolumeUI();
    }

    public override void OnHovered()
    {
        if (storeItemSO == null) return;

        StorageVolumeUI.Instance.UpdateInfo(storeItemSO, itemAmount.Value);
        StorageVolumeUI.Instance.Show();

        if (PlayerController.LocalInstance.GetHeldItem() is ItemScannerItem scanner) {
            scanner.SetStoreItemSO(storeItemSO);
        }
    }
    public override void OnUnhovered()
    {
        StorageVolumeUI.Instance.Hide();

        if (PlayerController.LocalInstance.GetHeldItem() is ItemScannerItem scanner) {
            scanner.ResetStoreItemSO();
        }
    }

    // double repetition here, but imo it's more efficient than making a general "interact OR alternate interact" event
    public override void OnInteract(PlayerController player)
    {
        // reset the cooldown so the actual functionality in OnInteractHeld triggers instantly
        interactHeldCooldownLeft = 0f;
    }

    public override void OnInteractSecondary(PlayerController player)
    {
        interactHeldCooldownLeft = 0f;
    }

    public override void OnInteractHeld(PlayerController player, float time)
    {
        interactHeldCooldownLeft -= Time.deltaTime;

        if (interactHeldCooldownLeft <= 0) {
            AddItemServerRpc(player);
            interactHeldCooldownLeft = interactHeldCooldownCurve.Evaluate(time);
        }
    }

    public override void OnAlternateHeld(PlayerController player, float time)
    {
        interactHeldCooldownLeft -= Time.deltaTime;

        if (interactHeldCooldownLeft <= 0) {
            RemoveItemServerRpc(player);
            interactHeldCooldownLeft = interactHeldCooldownCurve.Evaluate(time);
        }
    }

    [Rpc(SendTo.Server)]
    private void AddItemServerRpc(NetworkBehaviourReference senderObject)
    {
        senderObject.TryGet(out PlayerController sender);
        if (sender.GetHeldItem() != null && sender.GetHeldItem() is Container container)
        { 
            if (container.IsEmpty() ||
                container.GetStoreItemSO().storageType != storageType ||
                (storeItemSO != null && container.GetStoreItemSO() != storeItemSO) ||
                (storeItemSO != null && itemAmount.Value == storeItemSO.storageAmount)
            ) return;

            AddItemClientRpc(container);
            itemAmount.Value++;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AddItemClientRpc(NetworkBehaviourReference containerObject)
    {
        containerObject.TryGet(out Container container);
        if (storeItemSO == null) storeItemSO = container.GetStoreItemSO();
        container.RemoveItem();
    }

    [Rpc(SendTo.Server)]
    private void RemoveItemServerRpc(NetworkBehaviourReference senderObject)
    {
        senderObject.TryGet(out PlayerController sender);
        if (sender.GetHeldItem() != null && sender.GetHeldItem() is Container container && itemAmount.Value > 0 && !container.IsFull() && container.GetStoreItemSO() == storeItemSO) {
            
            RemoveItemClientRpc(container);
            itemAmount.Value--;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RemoveItemClientRpc(NetworkBehaviourReference containerObject)
    {
        containerObject.TryGet(out Container container);
        container.AddItem();
    }

    private void AddItemToDisplay(int itemAmount)
    {
        Transform itemDisplay = Instantiate(storeItemSO.prefab, transform.parent);
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

        float slantTheta = transform.rotation.eulerAngles.x * Mathf.Deg2Rad;
        float verticalSlant = addedPosition.z * Mathf.Tan(slantTheta);
        addedPosition.y -= verticalSlant;

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

    private void UpdateStorageVolumeUI()
    {
        if (isHovered) {
            StorageVolumeUI.Instance.UpdateInfo(storeItemSO, itemAmount.Value);
            StorageVolumeUI.Instance.Show();
        }
    }

    private void RemoveItemFromDisplay()
    {
        Destroy(itemDisplayStack.Pop().gameObject);
    }
}
