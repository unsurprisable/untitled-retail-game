
using UnityEngine;

public class PlayerController : MonoBehaviour
{   

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = 5 * Time.deltaTime;

        transform.position += moveDistance * moveDir;
    }

}
