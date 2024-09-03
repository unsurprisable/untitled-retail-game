
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnJump;
    public event EventHandler OnInteract;
    public event EventHandler OnDrop;
    public event EventHandler OnUse;

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        Instance = this;

        playerInputActions = new PlayerInputActions();

        playerInputActions.Player.Enable();
        playerInputActions.Player.Jump.performed     += (context) => {OnJump?.Invoke(this, EventArgs.Empty);};
        playerInputActions.Player.Interact.performed += (context) => {OnInteract?.Invoke(this, EventArgs.Empty);};
        playerInputActions.Player.Drop.performed     += (context) => {OnDrop?.Invoke(this, EventArgs.Empty);};
        playerInputActions.Player.Use.performed      += (context) => {OnUse?.Invoke(this, EventArgs.Empty);};
    }

    public Vector2 GetMovementVectorNormalized() {
        return playerInputActions.Player.Move.ReadValue<Vector2>();
    }

    public Vector2 GetMouseDelta() {
        return playerInputActions.Player.Camera.ReadValue<Vector2>();
    }

    public bool GetIsSprinting() {
        return playerInputActions.Player.Sprint.IsPressed();
    }
}
