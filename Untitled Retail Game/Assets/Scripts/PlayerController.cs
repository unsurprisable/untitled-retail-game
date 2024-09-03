
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

    [Header("Interaction")]
    [SerializeField] private LayerMask itemLayerMask;
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private float interactionDistance;
    [SerializeField] private InteractableItem lastHoveredItem;
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
            rb.AddForce(Vector3.up * jumpForce);
        };

        GameInput.Instance.OnInteract += (sender, args) => {
            if (lastHoveredItem == null) return;
            lastHoveredItem.OnInteract(this);
        };

        GameInput.Instance.OnDrop += (sender, args) => {
            if (heldItem == null) return;
            heldItem.OnDrop();
        };
        #endregion
    }

    private void Update()
    {
        if (Physics.Raycast(cameraAnchor.position, orientation.forward, out RaycastHit hit, interactionDistance, itemLayerMask)) {
            if (hit.transform.TryGetComponent<InteractableItem>(out InteractableItem hoveredItem))
            {
                // player is looking at an item
                if (lastHoveredItem != hoveredItem) {
                    hoveredItem.OnHovered();
                    if (lastHoveredItem != null) {
                        lastHoveredItem.OnUnhovered();
                    }
                    lastHoveredItem = hoveredItem;
                }
            } else {
                Debug.LogError("player interacted with an object on the Item layermask without an InteractableItem component!");
            }
        }
        else
        {
            // player is not looking at an item
            if (lastHoveredItem != null) {
                lastHoveredItem.OnUnhovered();
            }
            lastHoveredItem = null;
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
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

    // always called on interaction, even if the player already has an item
    public bool TryPickupItem(HoldableItem item)
    {
        if (heldItem != null) {
            Debug.Log("already holding an item.");
            return false;
        }
        heldItem = item;
        heldItem.GetComponent<Rigidbody>().isKinematic = true;
        item.transform.position = itemAnchor.position;
        item.transform.SetParent(itemAnchor);
        return true;
    }

    public void DropHeldItem()
    {
        heldItem.GetComponent<Rigidbody>().isKinematic = false;
        heldItem.transform.SetParent(null);
    }

}
