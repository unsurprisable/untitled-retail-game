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
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float collisionWidth;
    [SerializeField] private float maxGroundDistance;
    [SerializeField] private Vector3 collisionReduction;
    [SerializeField] private float jumpInputBuffer;
    private float jumpInputBufferLeft;

    [Header("Interaction")]
    [SerializeField] private LayerMask itemLayerMask;
    [SerializeField] private Transform itemAnchor;
    public Transform cameraAnchor;
    [SerializeField] private Transform playerModelTransform;
    [SerializeField] private float playerRotateSpeed;
    [SerializeField] private float interactionDistance;
    [SerializeField] private IInteractableObject hoveredItem;
    [SerializeField] private float throwForce;
    private HoldableItem heldItem;

    private bool controlsDisabled;


    public override void OnNetworkSpawn()
    {
        if (IsOwner) {
            LocalInstance = this;
            playerModelTransform.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            playerModelTransform.GetChild(0).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        } else {
            enabled = false;
        }
    }

    private void Start()
    {
        rb.isKinematic = false;

        FirstPersonCamera.LocalInstance.Enable();
        SettingsMenuUI.Instance.OnPlayerSpawned();

        #region Events

        GameInput.Instance.OnJump += (sender, args) => {
            jumpInputBufferLeft = jumpInputBuffer;
        };

        GameInput.Instance.MainAction += (sender, args) => {
            if (controlsDisabled) return;
            if (hoveredItem == null) return;
            if (heldItem != null && heldItem.hasUse) {
                heldItem.OnUse(this);
            } else if (heldItem == null && hoveredItem is HoldableItem item) {
                PickupItem(item);
            } else {
                hoveredItem.OnInteract(this);
            }
        };

        GameInput.Instance.SecondaryAction += (sender, args) => {
            if (controlsDisabled) return;
            if (hoveredItem == null) return; 
            if (heldItem != null && heldItem.hasUse) {
                heldItem.OnUseSecondary(this);
            } else {
                hoveredItem.OnInteractSecondary(this);
            }
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
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void LateUpdate()
    {
        HandlePlayerVisual();
    }

    private bool IsGrounded() {
        return Physics.OverlapBox(transform.position + Vector3.down * maxGroundDistance, new Vector3(collisionWidth/2, maxGroundDistance/2, collisionWidth/2) - collisionReduction, Quaternion.identity, groundLayerMask).Length != 0;
    }

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
            } else {
                Debug.LogError("Item is missing an 'IInteractableItem' component! (make sure the only collider is on the parent object with the component)");
            }
        }
        else
        {
            // player is not looking at an item
            hoveredItem?.Unhover();
            hoveredItem = null;
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
        
        jumpInputBufferLeft -= Time.fixedDeltaTime;
        if (jumpInputBufferLeft > 0 && IsGrounded()) {
            rb.velocity -= Vector3.up * rb.velocity.y; // cancel current velocity
            rb.AddForce(Vector3.up * jumpForce);
            jumpInputBufferLeft = 0f;
        }
    }


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
            PickupItemClientRpc(item);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PickupItemClientRpc(NetworkBehaviourReference itemObject)
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

    public void DisableControls()
    {
        controlsDisabled = true;
        FirstPersonCamera.LocalInstance.Disable();
    }
    public void EnableControls()
    {
        controlsDisabled = false;
        FirstPersonCamera.LocalInstance.Enable();
    }
}
