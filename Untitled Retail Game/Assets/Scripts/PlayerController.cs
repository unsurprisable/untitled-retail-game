
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public static PlayerController Instance { get; private set; }

    private Rigidbody rb;

    [Header("Movement")]
    [SerializeField] private FirstPersonCamera fpCamera;
    [SerializeField] private Transform orientation;
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
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private Transform playerModelTransform;
    [SerializeField] private float playerRotateSpeed;
    [SerializeField] private float interactionDistance;
    [SerializeField] private InteractableObject hoveredItem;
    [SerializeField] private float throwForce;
    private HoldableItem heldItem;

    [SerializeField] private bool inMenu;


    private void Awake()
    {
        Instance = this;

        rb = GetComponent<Rigidbody>();
        
        fpCamera.Enable();
    }

    private void Start()
    {
        #region Events

        GameInput.Instance.OnJump += (sender, args) => {
            jumpInputBufferLeft = jumpInputBuffer;
        };

        GameInput.Instance.MainAction += (sender, args) => {
            if (inMenu) return;
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
            if (inMenu) return;
            if (hoveredItem == null) return; 
            hoveredItem.OnInteractSecondary(this);
        };

        GameInput.Instance.OnDrop += (sender, args) => {
            if (inMenu) return;
            if (heldItem == null) return;
            DropHeldItem();
        };

        #endregion
    }

    private void Update()
    {
        if (inMenu) return;

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
            if (hit.transform.TryGetComponent<InteractableObject>(out InteractableObject item))
            {
                // player is looking at an item
                if (hoveredItem != item) {
                    item.Hover();
                    if (hoveredItem != null) {
                        hoveredItem.Unhover();
                    }
                    hoveredItem = item;
                }
            } else {
                Debug.LogError("Item is missing 'InteractableItem' component! (make sure the only collider is on the parent object with the component)");
            }
        }
        else
        {
            // player is not looking at an item
            if (hoveredItem != null) {
                hoveredItem.Unhover();
            }
            hoveredItem = null;
        }
    }

    private void HandleMovement()
    {
        // horizontal drag
        Vector3 dragVelocity = -rb.velocity;
        dragVelocity.y = 0;
        rb.AddForce(drag * dragVelocity);

        if (inMenu) return;
        
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
        item.GetComponent<Rigidbody>().isKinematic = true;
        item.GetComponent<Collider>().enabled = false;
        item.transform.localPosition = item.heldPositionOffset;
        item.transform.localRotation = Quaternion.Euler(item.heldRotationValues);

        item.OnPickup(this);

        heldItem = item;
    }

    public void DropHeldItem()
    {
        heldItem.GetComponent<Rigidbody>().isKinematic = false;
        heldItem.GetComponent<Collider>().enabled = true;

        heldItem.GetComponent<Rigidbody>().AddForce(orientation.forward * throwForce);
        
        heldItem.OnDrop();

        heldItem = null;
    }

    public HoldableItem GetHeldItem() {
        return heldItem;
    }

    public FirstPersonCamera GetFirstPersonCamera() {
        return fpCamera;
    }

    public void OnMenuOpened()
    {
        inMenu = true;
        fpCamera.Disable();
    }
    public void OnMenuClosed()
    {
        inMenu = false;
        fpCamera.Enable();
    }
}
