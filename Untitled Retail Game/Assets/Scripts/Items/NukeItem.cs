
using UnityEngine;

public class NukeItem : HoldableItem
{
    [Header("Nuke")]
    [SerializeField] private float interactDistance;
    [SerializeField] private LayerMask interactLayerMask;

    public override void OnUse(PlayerController player)
    {
        if (Physics.Raycast(player.cameraAnchor.position, player.orientation.forward, out RaycastHit hit, interactDistance, interactLayerMask)) {
            Debug.Log(hit.transform.parent.name);
            Destroy(hit.transform.parent.gameObject);
        }
    }

}
