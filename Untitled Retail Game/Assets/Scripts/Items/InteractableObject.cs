using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Interactable Object")]
    [Tooltip("Leave empty if you don't want it to have an outline.")]
    [SerializeField] private GameObject selectedVisual;

    public virtual void OnInteract(PlayerController player) {}
    public virtual void OnHovered() {}
    public virtual void OnUnhovered() {}

    public void Hover()
    {
        OnHovered();
        if (selectedVisual != null)
            selectedVisual.SetActive(true);
        
    }

    public void Unhover()
    {
        OnUnhovered();
        if (selectedVisual != null)
            selectedVisual.SetActive(false);
    }
}
