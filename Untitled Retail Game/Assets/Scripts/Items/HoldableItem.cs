using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableItem : InteractableObject
{
    public Vector3 heldPositionOffset;
    public Vector3 heldRotationValues;
    public bool hasUse;

    public virtual void OnUse(PlayerController player) {}
    public virtual void OnPickup(PlayerController player) {}
    public virtual void OnDrop() {}
}
