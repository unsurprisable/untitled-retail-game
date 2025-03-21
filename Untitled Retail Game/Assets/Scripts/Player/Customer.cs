using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Customer : NetworkBehaviour
{
    public enum MoveState { FROZEN, SEARCHING, LOOTING, IN_LINE, CHECKOUT }
    public enum ShoppingExperience { AMAZING, GOOD, NEUTRAL, BAD, HORRIBLE }

    private string[] possibleNames;
    private string customerName;

    private Rigidbody rb;
    private float rotateAmount;
    [SerializeField] private float wanderSpeed;

    [SerializeField] private Stack<StoreItemSO> shoppingList;
    [SerializeField] private Stack<StoreItemSO> cart;
    private ProductDisplayObject targetDisplayObject;
    private Transform targetSearchPosition;
    private int shoppingListLength;

    private Vector3 startSearchPosition;
    private float timeSpentSearching;

    [SerializeField] private float itemPickupTime;
    private float itemPickupTimeLeft;
    
    [SerializeField] private MoveState moveState;
    [SerializeField] private ShoppingExperience experience;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.detectCollisions = false;

        possibleNames = new string[]{
            "Bob", "Rob", "Tob", "Dob", "Sob", "Gob", "Nob", "Lob", "Mob", "Job",
            "Cob", "Hob", "Pob", "Wob", "Zob", "Yob", "Vob", "Qob", "Xob", "Keb",
            "Brob", "Snob", "Klob", "Blob", "Flob", "Glob", "Plob", "Slob", "Scrob",
            "Drob", "Zlob", "Grubob", "Trob", "Glorbob", "Zoblob", "Knobob", "Vrob",
            "Fribob", "Blorob", "Crubob", "Shlob", "Mlob", "Qlob", "Drobob", "Bzob",
            "Frob", "Clob", "Gribob", "Trubob", "Slorb", "Vlob", "Snorb", "Blubob",
            "Jrob", "Trobb", "Zorb", "Clobob", "Splob", "Wrob", "Glubob", "Drobobob",
            "Skob", "Grob", "Krob", "Jlob", "Blorb", "Srob", "Qrob", "Krobob", "Trlob",
            "Mrob", "Vrobob", "Snobob", "Grobb", "Plorb", "Wlob", "Dlob", "Nrob", "Hrob",
            "Splorb", "Clobobob", "Florb", "Zrob", "Brorb", "Crorb", "Frobob"
        };
    }

    public override void OnNetworkSpawn() {

        if (!IsServer) {
            this.enabled = false;
        }

        customerName = possibleNames[Random.Range(0, possibleNames.Length)];
        gameObject.name = $"Customer [{customerName}]";
        GetComponent<PlayerNametagDisplay>().SetNametag(customerName);

        // brain

        // temp until i figure out how this system will actually work
        // (just wants 1 of each item)
        shoppingList = new Stack<StoreItemSO>(SerializeManager.Instance.GetStoreItemListSO().list);
        cart = new Stack<StoreItemSO>();
        shoppingListLength = shoppingList.Count;
        experience = ShoppingExperience.NEUTRAL;
        
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

                if (targetSearchPosition == null) {
                    Debug.LogWarning($"{customerName}: i don't know :( where :( im going -- (targetSearchPosition was null when starting search)");
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
        transform.position = transform.position + Vector3.up * 5f;
    }
    private void Unfreeze() {
        moveState = MoveState.SEARCHING;
        rb.isKinematic = false;
        transform.position = transform.position + Vector3.down * 5f;
    }

    private void TakeItem() {
        StoreItemSO storeItemSO = shoppingList.Peek();
        if (targetDisplayObject.GetStoreItemAmount(storeItemSO) <= 0) {
            Debug.Log($"{customerName}: someone took my {storeItemSO.name} D:");
            LookForNextItem(); // look for the item again (didn't .Pop() yet)
            return;
        }

        targetDisplayObject.CustomerTakeItem(storeItemSO);
        Debug.Log($"{customerName}: I got a {storeItemSO.name} :D");
        cart.Push(shoppingList.Pop());
        LookForNextItem();
    }

    private void LookForNextItem() {
        if (shoppingList.Count == 0) {
            if (cart.Count == 0) {
                experience = ShoppingExperience.HORRIBLE;
                LeaveStore();
                return;
            } else if (cart.Count == shoppingListLength) {
                Debug.Log($"{customerName}: Setting AMAZING experience. Cart count: {cart.Count}, Shopping list length: {shoppingListLength}");
                experience = ShoppingExperience.AMAZING;
            }
            LookForCheckout();
            return;
        }

        targetDisplayObject = StoreManager.Instance.SearchForItemInStore(shoppingList.Peek());
        if (targetDisplayObject == null) {
            Debug.Log($"{customerName}: um excuse me, i can't find the {shoppingList.Peek().name}");
            shoppingList.Pop();
            LookForNextItem();
            return;
        }
        
        targetSearchPosition = targetDisplayObject.viewingArea.transform;
        timeSpentSearching = 0f;
        startSearchPosition = transform.position;
        moveState = MoveState.SEARCHING;
    }

    private void StartLooting() {
        itemPickupTimeLeft = itemPickupTime;
        moveState = MoveState.LOOTING;
    }

    private void LookForCheckout() {
        Debug.Log($"{customerName}: i'm looking for a checkout now :) :)");
        // no checkouts rn
        LeaveStore();
    }

    private void LeaveStore() {
        string message = "i have no opinion on this shopping experience";
        switch (experience) {
            case ShoppingExperience.AMAZING:
                message = "wow that was so fun and exciting! :D i found all of my items";
                break;
            case ShoppingExperience.GOOD:
                break;
            case ShoppingExperience.NEUTRAL:
                message = "not the best shopping experience but not the worst";
                break;
            case ShoppingExperience.BAD:
                break;
            case ShoppingExperience.HORRIBLE:
                message = "i'm never coming back here again. and im wishing you a very bad christmas. and im blowing up your house tomorrow.";
                break;
        }
        Debug.Log($"{customerName}: {message}");
        // Freeze();
        Destroy(gameObject);
    }
}
