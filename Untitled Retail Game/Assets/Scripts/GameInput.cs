
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        Instance = this;

        playerInputActions = new PlayerInputActions();

        playerInputActions.Player.Enable();
    }

    public Vector2 GetMovementVectorNormalized() {
        return playerInputActions.Player.Move.ReadValue<Vector2>();
    }
}
