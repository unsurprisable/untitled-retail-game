using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerNametagDisplay : NetworkBehaviour
{

    [SerializeField] TextMeshPro nametag;

    public void SetNametag(string displayName)
    {
        nametag.text = displayName;
    }

    private void LateUpdate()
    {
        // everyone BUT the owner should be running this script
        if (IsOwner) return;

        nametag.transform.forward = Camera.main.transform.forward;
    }

}
