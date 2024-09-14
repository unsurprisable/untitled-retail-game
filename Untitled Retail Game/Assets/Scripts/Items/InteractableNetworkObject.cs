using Unity.Netcode;
using UnityEngine;

public class InteractableNetworkObject : NetworkBehaviour, IInteractableObject
{
    [Header("Interactable Object")]
    [Tooltip("Leave empty if you don't want it to have an outline.")]
    [SerializeField] private GameObject selectedVisual;
    protected bool isHoveredOnClient;

    public virtual void OnHovered(){}
    public virtual void OnInteract(PlayerController player){}
    public virtual void OnInteractSecondary(PlayerController player){}
    public virtual void OnUnhovered(){}

    public void Hover()
    {
        if (isHoveredOnClient) return;

        OnHovered();
        if (selectedVisual != null)
            selectedVisual.SetActive(true);
        isHoveredOnClient = true;
    }

    public void Unhover()
    {
        if (!isHoveredOnClient) return;

        OnUnhovered();
        if (selectedVisual != null)
            selectedVisual.SetActive(false);
        isHoveredOnClient = false;
    }
}
