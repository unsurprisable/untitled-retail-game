
using UnityEngine;

public class NukeItem : HoldableItem
{
    [Header("Nuke")]
    [SerializeField] private float interactDistance;
    [SerializeField] private LayerMask interactLayerMask;

    public override void OnUse(PlayerController player)
    {
        if (Physics.Raycast(player.cameraAnchor.position, player.orientation.forward, out RaycastHit hit, interactDistance, interactLayerMask)) {
            if (hit.transform.parent.TryGetComponent(out BuildObject buildObject)) {
                buildObject.Sell();
            } else {
                Debug.LogWarning($"Object \"{hit.transform.name}\" is on BuildBounds layer without a BuildObject parent! oh no D:");
            }
        }
    }

}
