using Unity.Netcode;
using UnityEngine;

public class ProductDisplayObject : BuildObject
{
    [Header("Product Display Object")]
    [SerializeField] Transform[] storageVolumes;

    override protected void OnPlace()
    {
        foreach (Transform storageVolume in storageVolumes) {
            storageVolume.GetComponent<NetworkObject>().Spawn();
        }
    }
}
