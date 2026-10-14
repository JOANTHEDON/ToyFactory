
using System;
using UnityEngine;

public class GameInput : MonoBehaviour {

    public event EventHandler OnInteractAction;
    private PlayerInputActions playerInputActions;

    private void Awake() {
        playerInputActions = new PlayerInputActions();

        playerInputActions.Player.Interact.performed += Interact_performed;
    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnInteractAction?.Invoke(this, EventArgs.Empty);

    }

    private void OnEnable() {
        playerInputActions?.Player.Enable();
    }

    private void OnDisable() {
        playerInputActions?.Player.Disable();
    }

    private void OnDestroy() {
        playerInputActions?.Dispose();
        playerInputActions = null;
    }

    public Vector2 GetMovementVectorNormalized() {
        if (playerInputActions == null)
            return Vector2.zero;

        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        return inputVector.normalized;
    }
}
