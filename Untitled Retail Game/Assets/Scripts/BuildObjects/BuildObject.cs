using Unity.Netcode;
using UnityEngine;

public class BuildObject : NetworkBehaviour
{
    [Header("Build Object")]
    public BuildObjectSO buildObjectSO;

    protected virtual void OnSell() {}
    protected virtual void OnPlace() {}


    public void Place() {
        OnPlace();
    }

    public void Sell() {
        GameManager.Instance.AddToBalance(buildObjectSO.price); // 100% sellback rate

        OnSell();

        Destroy(gameObject);
        Debug.Log($"Sold \"{buildObjectSO.name}\" for {buildObjectSO.price}");
    }
}

