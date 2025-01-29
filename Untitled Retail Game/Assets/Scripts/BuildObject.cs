using UnityEngine;

public class BuildObject : MonoBehaviour
{
    public BuildObjectSO buildObjectSO;

    protected virtual void OnSell() {}
    protected virtual void OnPlace() {}

    public void Sell() {
        GameManager.Instance.AddToBalance(buildObjectSO.price); // 100% sellback rate
        OnSell();
        Destroy(gameObject);
        Debug.Log($"Sold \"{buildObjectSO.name}\" for {buildObjectSO.price}");
    }
}

