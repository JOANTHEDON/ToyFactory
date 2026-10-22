using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {

    [SerializeField] private RectTransform containerRect;
    [SerializeField] private RectTransform handleRect;
    [SerializeField] private float handleRange = 75f;

    private Vector2 inputVector = Vector2.zero;

    private void Awake() {
        if (containerRect == null) {
            containerRect = GetComponent<RectTransform>();
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData) {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint)) {
            localPoint = Vector2.ClampMagnitude(localPoint, handleRange);
            if (handleRect != null) {
                handleRect.anchoredPosition = localPoint;
            }
            inputVector = localPoint / handleRange;
            if (GameInput.Instance != null) {
                GameInput.Instance.SetVirtualMovementVector(inputVector);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        inputVector = Vector2.zero;
        if (handleRect != null) {
            handleRect.anchoredPosition = Vector2.zero;
        }
        if (GameInput.Instance != null) {
            GameInput.Instance.SetVirtualMovementVector(Vector2.zero);
        }
    }

    public Vector2 GetInputVector() {
        return inputVector;
    }
}
