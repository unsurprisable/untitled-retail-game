using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableItem : InteractableItem
{
    
    public virtual void OnPickup(PlayerController player) {}
    public virtual void OnDrop() {}

    public override void OnInteract(PlayerController player)
    {
        PickupItem(player);
    }
    

    private void PickupItem(PlayerController player)
    {
        if (player.TryPickupItem(this)) {
            this.OnPickup(player);
        }
    }

    
}
