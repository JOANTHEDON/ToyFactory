using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour {

    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    private PlayerInputActions playerInputActions;
    private Vector2 virtualMovementVector = Vector2.zero;

    private void Awake() {
        Instance = this;
        playerInputActions = new PlayerInputActions();

        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.InteractAlternate.performed += InteractAlternate_performed;
    }

    private void InteractAlternate_performed(InputAction.CallbackContext context) {
        TriggerInteractAlternateAction();
    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        TriggerInteractAction();
    }

    public void TriggerInteractAction() {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    public void TriggerInteractAlternateAction() {
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
    }

    public void SetVirtualMovementVector(Vector2 vector) {
        virtualMovementVector = vector;
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
        if (virtualMovementVector != Vector2.zero) {
            return virtualMovementVector.normalized;
        }

        if (playerInputActions == null)
            return Vector2.zero;

        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        return inputVector.normalized;
    }
}
