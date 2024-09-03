using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroomItem : HoldableItem
{
    public override void OnPickup(PlayerController player)
    {
        Debug.Log("I, a broom, was picked up!");
    }
}
