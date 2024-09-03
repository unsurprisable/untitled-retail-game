
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnJump;

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        Instance = this;

        playerInputActions = new PlayerInputActions();

        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed += (context) => {OnJump?.Invoke(this, EventArgs.Empty);};
    }

    public Vector2 GetMovementVectorNormalized() {
        return playerInputActions.Player.Move.ReadValue<Vector2>();
    }

    public Vector2 GetMouseDelta() {
        return playerInputActions.Player.Camera.ReadValue<Vector2>();
    }
}
