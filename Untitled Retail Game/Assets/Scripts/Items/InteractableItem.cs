using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [SerializeField] private GameObject selectedVisual;


    public virtual void OnInteract(PlayerController player) {}

    public void OnHovered()
    {
        selectedVisual.SetActive(true);
    }

    public void OnUnhovered()
    {
        selectedVisual.SetActive(false);
    }
}
