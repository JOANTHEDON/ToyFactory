
using UnityEngine;

public class GameInput : MonoBehaviour {
    private PlayerInputActions playerInputActions;

    private void Awake() {
        playerInputActions = new PlayerInputActions();
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
