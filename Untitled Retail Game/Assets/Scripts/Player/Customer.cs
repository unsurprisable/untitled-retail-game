using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Customer : NetworkBehaviour
{
    [SerializeField] private string[] possibleNames;
    private Rigidbody rb;
    private float rotateAmount;
    [SerializeField] private float wanderSpeed;


    [SerializeField] private Stack<StoreItemSO> shoppingList;
    [SerializeField] private Stack<StoreItemSO> cart;
    
    private bool isFrozen = false;

    void Awake() {
        string randName = possibleNames[Random.Range(0, possibleNames.Length)];
        gameObject.name = $"Customer [{randName}]";
        GetComponent<PlayerNametagDisplay>().SetNametag(randName);
        rb = GetComponent<Rigidbody>();

        // brain

        // temp until i figure out how this system will actually work
        // (just wants 1 of each item)
        shoppingList = new Stack<StoreItemSO>(SerializeManager.Instance.GetStoreItemListSO().list);
        cart = new Stack<StoreItemSO>();
    }

    void FixedUpdate()
    {
        GetComponent<PlayerNametagDisplay>().enabled = true;

        if (!isFrozen) {
            if (Random.Range(0, 90) == 0) rotateAmount = Random.Range(-1f, 1f) * Mathf.PI/2;
            rb.angularVelocity = Vector3.up * rotateAmount;
            rb.linearVelocity = transform.forward * wanderSpeed * Time.fixedDeltaTime;
        }
    }

}
