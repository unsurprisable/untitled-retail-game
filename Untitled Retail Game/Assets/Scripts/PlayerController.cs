using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    public static PlayerController LocalInstance { get; private set; }

    [SerializeField] private Rigidbody rb;

    [Header("Movement")]
    public Transform orientation;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float drag;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpInputBuffer;
    private float jumpInputBufferLeft;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float collisionWidth;
    [SerializeField] private float maxGroundDistance;
    [SerializeField] private Vector3 collisionReduction; // no idea what this is (i forgor)

    [Header("Interaction")]
    public Transform cameraAnchor;
    [SerializeField] private LayerMask itemLayerMask;
    [SerializeField] private Transform itemAnchor;
    [SerializeField] private Transform playerModelTransform;
    [SerializeField] private float playerRotateSpeed;
    [SerializeField] private float interactionDistance;
    [SerializeField] private IInteractableObject hoveredItem;
    [SerializeField] private float throwForce;
    private HoldableItem heldItem;

    [Header("Interaction Held")]
    private bool interactHeld;
    [SerializeField] private IInteractableObject interactHeldItem; // the item that was interacted with at the start of the hold
    [SerializeField] private float interactHeldTime;
    private bool InteractHeldIsHovering => interactHeldItem != null && interactHeldItem.IsHovered();
    private bool InteractHeldItemIsHoldable => interactHeldItem != null && interactHeldItem is HoldableItem;

    [Header("Interaction Outlines")]
    public Outline.Mode outlineMode;
    public Color outlineColor;
    public float outlineWidth;

    private bool controlsDisabled;


    // TODO:
    // Interaction Held detection:
    // X only starts if an object is interacted with (so clicking and holding in the air won't do anything)
    // X should remember what object the hold is associated with (should also have an event for when you look away from it)
    // X needs to keep track of the time it's been held down (and reset when looking away or when controls are disabled in a menu)
    // X needs to call OnInteractHeld() on that item every frame it's being hovered/held
    // X needs to turn off and reset time+item when LMB isn't held anymore
    // X OnInteractHeldLookAway() event triggers when you look away from ANY item, not just the held one!


    public override void OnNetworkSpawn()
    {
        if (IsServer) {
            NetworkManager.Singleton.SceneManager.OnSynchronize += NetworkManager_OnSynchronize;
        }

        if (IsOwner) {
            LocalInstance = this;
            playerModelTransform.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            playerModelTransform.GetChild(0).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        } else {
            enabled = false;
        }
    }

    private void NetworkManager_OnSynchronize(ulong clientId)
    {
        if (heldItem != null) {
            PickupItemClientRpc(heldItem, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }
    }

    private void Start()
    {
        rb.isKinematic = false;

        FirstPersonCamera.LocalInstance.Enable(true);
        SettingsMenuUI.Instance.OnPlayerSpawned();

        #region Events

        GameInput.Instance.OnJump += (sender, args) => {
            // jump is actually detected in HandleMovement(); this indirectly causes the player to jump by resetting the buffer
            jumpInputBufferLeft = jumpInputBuffer;
        };

        // Main Action (LMB)
        // used for: Using held items, Interacting with objects, Picking up objects, Beginning interact held functionality
        GameInput.Instance.MainAction += (sender, args) => {
            if (controlsDisabled) return;
            if (hoveredItem == null) return;

            if (heldItem != null && heldItem.hasUse) {
                heldItem.OnUse(this);
                // interact held logic
                interactHeld = true;
                // interactHeldIsHovering = true;
                interactHeldItem = heldItem;

            } else if (heldItem == null && hoveredItem is HoldableItem item) {
                PickupItem(item);
            
            } else {
                hoveredItem.OnInteract(this);
                // interact held logic
                interactHeld = true;
                // interactHeldIsHovering = true;
                interactHeldItem = hoveredItem;
            }
        };

        // Secondary Action (RMB)
        // used for: Using secondary ability of held items, Interacting with second ability of objects
        GameInput.Instance.SecondaryAction += (sender, args) => {
            if (controlsDisabled) return;
            if (hoveredItem == null) return; 

            if (heldItem != null && heldItem.hasUse) {
                heldItem.OnUseSecondary(this);

            } else {
                hoveredItem.OnInteractSecondary(this);
            }
        };

        // Releasing Main Action (LMB)
        // used for: Detecting when interact held is cancelled
        GameInput.Instance.MainActionReleased += (sender, args) => {
            interactHeld = false;
            // interactHeldIsHovering = false;
            // interactHeldTime = 0f;
            interactHeldItem = null;
        };

        GameInput.Instance.OnDrop += (sender, args) => {
            if (controlsDisabled) return;
            if (heldItem == null) return;
            
            DropHeldItem();
        };

        #endregion
    }

    private void Update()
    {
        if (controlsDisabled) return;

        HandleItemHovering();
        HandleInteractHeld();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void LateUpdate()
    {
        HandlePlayerVisual();
    }

    #region Update Functions

    private void HandlePlayerVisual()
    {
        playerModelTransform.transform.rotation = Quaternion.Slerp(
            playerModelTransform.transform.rotation, 
            Quaternion.Euler(Vector3.up * orientation.rotation.eulerAngles.y),
            Time.deltaTime * playerRotateSpeed);

        if (heldItem != null) {
            // TODO: add heldItem.heldPositionValues to item position (but it needs to be adjusted for the angle using trig)
            heldItem.transform.SetPositionAndRotation(itemAnchor.position + Vector3.up * heldItem.heldPositionOffset.y, Quaternion.Euler(playerModelTransform.rotation.eulerAngles + heldItem.heldRotationValues));
        }
    }

    private void HandleItemHovering()
    {
        if (Physics.Raycast(cameraAnchor.position, orientation.forward, out RaycastHit hit, interactionDistance, itemLayerMask)) {
            if (hit.transform.TryGetComponent(out IInteractableObject item))
            {
                // player is looking at an item
                if (hoveredItem != item) {
                    hoveredItem?.Unhover();
                    item.Hover();
                    hoveredItem = item;
                }

                // if (interactHeld) {
                //     interactHeldIsHovering = true;
                // }
            } else {
                Debug.LogError("Item is missing an 'IInteractableItem' component! (make sure the only collider is on the parent object with the component)");
            }
        }
        else
        {
            // player is not looking at an item
            if (hoveredItem != null) {

                // as far as i know, interactHeld should only be true if interactHeldItem is !null... if this errors, that's probably why
                if (interactHeld && interactHeldItem == hoveredItem) {
                    // interactHeldTime = 0f;
                    interactHeldItem.OnInteractHeldLookAway(this);
                    // interactHeldIsHovering = false;
                }

                hoveredItem.Unhover();
                hoveredItem = null;
            }
        }
    }

    private void HandleMovement()
    {
        // horizontal drag
        Vector3 dragVelocity = -rb.velocity;
        dragVelocity.y = 0;
        rb.AddForce(drag * dragVelocity);

        if (controlsDisabled) return;
        
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        // movement relative to camera direction
        float theta = -orientation.localRotation.eulerAngles.y * Mathf.Deg2Rad;
        Vector2 relMoveDir = new Vector2(
            inputVector.x * Mathf.Cos(theta) - inputVector.y * Mathf.Sin(theta),
            inputVector.x * Mathf.Sin(theta) + inputVector.y * Mathf.Cos(theta)
        );

        float moveSpeed = GameInput.Instance.GetIsSprinting() ? runSpeed : walkSpeed;

        Vector3 moveDir = new Vector3(relMoveDir.x, 0f, relMoveDir.y) * moveSpeed;

        rb.AddForce(moveDir);
        
        // jump detection
        jumpInputBufferLeft -= Time.fixedDeltaTime;
        if (jumpInputBufferLeft > 0 && IsGrounded()) {
            rb.velocity -= Vector3.up * rb.velocity.y; // cancel current velocity
            rb.AddForce(Vector3.up * jumpForce);
            jumpInputBufferLeft = 0f;
        }
    }

    private bool IsGrounded() {
        return Physics.OverlapBox(transform.position + Vector3.down * maxGroundDistance, new Vector3(collisionWidth/2, maxGroundDistance/2, collisionWidth/2) - collisionReduction, Quaternion.identity, groundLayerMask).Length != 0;
    }

    private void HandleInteractHeld() {
        if (interactHeld && InteractHeldIsHovering) {
            interactHeldTime += Time.deltaTime;
            interactHeldItem.OnInteractHeld(this, interactHeldTime);
        } else {
            interactHeldTime = 0f;
        }
    }
    
    #endregion

    #region Interactions

    public void PickupItem(HoldableItem item)
    {
        PickupItemServerRpc(item);
    }

    [Rpc(SendTo.Server)]
    private void PickupItemServerRpc(NetworkBehaviourReference itemObject, RpcParams rpcParams = default)
    {
        if (itemObject.TryGet(out HoldableItem item))
        {
            item.NetworkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
            PickupItemClientRpc(item, RpcTarget.ClientsAndHost);
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void PickupItemClientRpc(NetworkBehaviourReference itemObject, RpcParams rpcParams)
    {
        if (itemObject.TryGet(out HoldableItem item))
        {
            item.GetComponent<Rigidbody>().isKinematic = true;
            item.GetComponent<Collider>().enabled = false;

            item.OnPickup(this);

            heldItem = item;
        }
    }

    public void DropHeldItem()
    {
        DropHeldItemServerRpc(orientation.forward);
    }

    [Rpc(SendTo.Server)]
    private void DropHeldItemServerRpc(Vector3 clientThrowDirection)
    {
        // errors here rn because heldItem will be null to any late clients who didn't see the player pick up the item
        heldItem.NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
        HoldableItem item = heldItem;
        DropHeldItemClientRpc();

        item.GetComponent<Rigidbody>().AddForce(clientThrowDirection * throwForce);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void DropHeldItemClientRpc()
    {
        heldItem.GetComponent<Rigidbody>().isKinematic = false;
        heldItem.GetComponent<Collider>().enabled = true;
        
        heldItem.OnDrop();

        heldItem = null;
    }

    public HoldableItem GetHeldItem() {
        return heldItem;
    }

    #endregion

    public void DisableControls(bool changeMouseState = true)
    {
        controlsDisabled = true;
        FirstPersonCamera.LocalInstance.Disable(changeMouseState);
    }
    public void EnableControls(bool changeMouseState = true)
    {
        controlsDisabled = false;
        FirstPersonCamera.LocalInstance.Enable(changeMouseState);
    }
}
