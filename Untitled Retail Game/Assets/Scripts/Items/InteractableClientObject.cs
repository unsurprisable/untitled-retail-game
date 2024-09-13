using UnityEngine;

public class InteractableClientObject : MonoBehaviour, IInteractableObject
{
    [Header("Interactable Object")]
    [Tooltip("Leave empty if you don't want it to have an outline.")]
    [SerializeField] private GameObject selectedVisual;
    protected bool isHovered;
    
    public virtual void OnHovered(){}
    public virtual void OnInteract(PlayerController player){}
    public virtual void OnInteractSecondary(PlayerController player){}
    public virtual void OnUnhovered(){}

    public void Hover()
    {
        OnHovered();
        if (selectedVisual != null)
            selectedVisual.SetActive(true);
        isHovered = true;
    }

    public void Unhover()
    {
        OnUnhovered();
        if (selectedVisual != null)
            selectedVisual.SetActive(false);
        isHovered = false;
    }
}
