
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody rb;

    [Header("Movement")]
    [SerializeField] private Transform orientation;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float drag;
    [SerializeField] private float jumpForce;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        GameInput.Instance.OnJump += (sender, args) => {rb.AddForce(Vector3.up * jumpForce);};
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector3 dragVelocity = -rb.velocity;
        dragVelocity.y = 0;
        rb.AddForce(drag * dragVelocity);
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        float theta = -orientation.localRotation.eulerAngles.y * Mathf.Deg2Rad;
        Vector2 relMoveDir = new Vector2(
            inputVector.x * Mathf.Cos(theta) - inputVector.y * Mathf.Sin(theta),
            inputVector.x * Mathf.Sin(theta) + inputVector.y * Mathf.Cos(theta)
        );

        Vector3 moveDir = new Vector3(relMoveDir.x, 0f, relMoveDir.y) * moveSpeed;

        rb.AddForce(moveDir);
    }

}
