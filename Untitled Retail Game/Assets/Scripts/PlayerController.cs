
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody rb;

    [Header("Movement")]
    [SerializeField] private Transform orientation;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float drag;
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float collisionWidth;
    [SerializeField] private float maxGroundDistance;

    [Header("Interaction")]
    [SerializeField] private LayerMask itemLayerMask;
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private float interactionDistance;
    [SerializeField] private InteractableObject hoveredItem;
    [SerializeField] private Transform itemAnchor;
    private HoldableItem heldItem;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        #region Events

        GameInput.Instance.OnJump += (sender, args) => {
            // should switch to BoxCast or something similar; right now its just a single, centered ray, which is bad for edges
            if (Physics.Raycast(transform.position + Vector3.up*maxGroundDistance, Vector3.down, 2*maxGroundDistance, groundLayerMask)) {
                rb.AddForce(Vector3.up * jumpForce);
            }
        };

        GameInput.Instance.OnInteract += (sender, args) => {
            if (hoveredItem == null) return; 
            if (heldItem == null && hoveredItem is HoldableItem item) {
                PickupItem(item);
            } else {
                hoveredItem.OnInteract(this);
            }
        };

        GameInput.Instance.OnInteractAlternate += (sender, args) => {
            if (hoveredItem == null) return; 
            hoveredItem.OnInteractAlternate(this);
        };

        GameInput.Instance.OnDrop += (sender, args) => {
            if (heldItem == null) return;
            DropHeldItem();
        };

        GameInput.Instance.OnUse += (sender, args) => {
            if (heldItem == null) return;
            heldItem.OnUse(this);
        };

        #endregion
    }

    private void Update()
    {
        HandleItemHovering();
    }

    private void FixedUpdate()
    {
        HandleMovement();
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
    }


    public void PickupItem(HoldableItem item)
    {
        item.GetComponent<Rigidbody>().isKinematic = true;
        item.GetComponent<Collider>().enabled = false;
        item.transform.SetParent(itemAnchor, false);
        item.transform.localPosition = item.heldPositionOffset;
        item.transform.localRotation = Quaternion.Euler(item.heldRotationValues);

        item.OnPickup(this);

        heldItem = item;
    }

    public void DropHeldItem()
    {
        heldItem.GetComponent<Rigidbody>().isKinematic = false;
        heldItem.GetComponent<Collider>().enabled = true;
        heldItem.transform.SetParent(null);
        
        heldItem.OnDrop();

        heldItem = null;
    }

    public HoldableItem GetHeldItem() {
        return heldItem;
    }
}
