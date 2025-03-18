using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Customer : NetworkBehaviour
{
    public enum MoveState { FROZEN, SEARCHING, LOOTING, IN_LINE, CHECKOUT }
    public enum ShoppingExperience { AMAZING, GOOD, BAD, HORRIBLE }

    [SerializeField] private string[] possibleNames;
    private string customerName;

    private Rigidbody rb;
    private float rotateAmount;
    [SerializeField] private float wanderSpeed;

    [SerializeField] private Stack<StoreItemSO> shoppingList;
    [SerializeField] private Stack<StoreItemSO> cart;
    private ProductDisplayObject targetDisplayObject;

    private Vector3 startSearchPosition;
    private float timeSpentSearching;

    [SerializeField] private float itemPickupTime;
    private float itemPickupTimeLeft;
    
    [SerializeField] private MoveState moveState;
    [SerializeField] private ShoppingExperience experience;

    public override void OnNetworkSpawn() {
        customerName = possibleNames[Random.Range(0, possibleNames.Length)];
        gameObject.name = $"Customer [{customerName}]";
        GetComponent<PlayerNametagDisplay>().SetNametag(customerName);
        rb = GetComponent<Rigidbody>();

        // brain

        // temp until i figure out how this system will actually work
        // (just wants 1 of each item)
        shoppingList = new Stack<StoreItemSO>(SerializeManager.Instance.GetStoreItemListSO().list);
        cart = new Stack<StoreItemSO>();
        
        LookForNextItem();
    }


    private void FixedUpdate()
    {
        GetComponent<PlayerNametagDisplay>().enabled = true;

        switch (moveState) {
            case MoveState.FROZEN:
                break;

            case MoveState.SEARCHING:
                // if (Random.Range(0, 90) == 0) rotateAmount = Random.Range(-1f, 1f) * Mathf.PI/2;
                // rb.angularVelocity = Vector3.up * rotateAmount;
                // rb.linearVelocity = transform.forward * wanderSpeed * Time.fixedDeltaTime;

                if (targetDisplayObject == null) {
                    Debug.LogWarning($"{customerName}: i don't know :( where :( im going -- (targetDisplayObject was null when starting search)");
                    Freeze();
                    return;
                }

                Vector3 targetPos = targetDisplayObject.viewingArea.position;
                float moveTime = 3f;

                rb.Move(Vector3.Lerp(startSearchPosition, targetPos, timeSpentSearching / moveTime), transform.rotation);

                timeSpentSearching += Time.fixedDeltaTime;

                if (timeSpentSearching >= moveTime) {
                    StartLooting();
                }
                break;

            case MoveState.LOOTING:
                itemPickupTimeLeft -= Time.fixedDeltaTime;
                if (itemPickupTimeLeft <= 0) {
                    TakeItem();
                }
                break;

            case MoveState.IN_LINE:
                break;

            case MoveState.CHECKOUT:
                break;
        }
    }

    private void Freeze() {
        moveState = MoveState.FROZEN;
        rb.isKinematic = true;
        rb.detectCollisions = false;
    }
    private void Unfreeze() {
        moveState = MoveState.SEARCHING;
        rb.isKinematic = true;
        rb.detectCollisions = false;
    }

    private void TakeItem() {
        /* 
        if (display object doesn't contain the item anymore) {
            // look for another display object with the item
            LookForNextItem();
        }
        */
        cart.Push(shoppingList.Pop());
        LookForNextItem();
    }

    private void LookForNextItem() {
        if (shoppingList.Count == 0) {
            if (cart.Count == 0) {
                experience = ShoppingExperience.HORRIBLE;
            }
            LookForCheckout();
            return;
        }
        // TODO
        // StorageVolume nextVolume = StoreManager.Instance.SearchForItemInVolumes(shoppingList.Peek());
        // targetDisplayObject = nextVolume.GetProductDisplayObject();

        targetDisplayObject = StoreManager.Instance.SearchForItemInStore(shoppingList.Peek());
        if (targetDisplayObject == null) {
            Debug.Log($"{customerName}: um excuse me, i can't find the {shoppingList.Peek().name}");
            shoppingList.Pop();
            LookForNextItem();
            return;
        }
        
        startSearchPosition = transform.position;
        moveState = MoveState.SEARCHING;
    }

    private void StartLooting() {
        itemPickupTimeLeft = itemPickupTime;

        moveState = MoveState.LOOTING;
    }

    private void LookForCheckout() {
        Debug.Log($"{customerName}: i'm looking for a checkout now :) :)");
    }

    private void LeaveStore() {
        string message = "i have no opinion on this shopping experience";
        switch (experience) {
            case ShoppingExperience.AMAZING:
                break;
            case ShoppingExperience.GOOD:
                break;
            case ShoppingExperience.BAD:
                break;
            case ShoppingExperience.HORRIBLE:
                message = "i'm never coming back here again. and im filing a lawsuit. and im wishing you a very bad christmas. and im blowing up your house tomorrow.";
                break;
        }
        Debug.Log($"{customerName}: {message}");
    }
}
