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

        Debug.Log($"Placed \"{buildObjectSO.name}\" for {buildObjectSO.price}");
    }

    public void Sell() {
        OnSell();

        Debug.Log($"Sold \"{buildObjectSO.name}\" for {buildObjectSO.price}");
    }
}

