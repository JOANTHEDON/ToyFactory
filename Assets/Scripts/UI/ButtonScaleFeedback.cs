using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Gentle grow-on-hover / shrink-on-press feedback for code-built buttons. Uses unscaled time so it works while paused.</summary>
public class ButtonScaleFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float pressScale = 0.94f;
    [SerializeField] private float speed = 14f;

    private float targetScale = 1f;
    private bool hovered;
    private bool pressed;
    private Vector3 baseScale = Vector3.one;

    private void Awake() {
        baseScale = transform.localScale;
        if (baseScale.sqrMagnitude < 0.0001f) baseScale = Vector3.one;
    }

    private void OnEnable() {
        hovered = false;
        pressed = false;
        targetScale = 1f;
        transform.localScale = baseScale;
    }

    private void Update() {
        float current = transform.localScale.x / Mathf.Max(0.0001f, baseScale.x);
        float next = Mathf.Lerp(current, targetScale, Time.unscaledDeltaTime * speed);
        transform.localScale = baseScale * next;
    }

    public void OnPointerEnter(PointerEventData eventData) { hovered = true; Refresh(); }
    public void OnPointerExit(PointerEventData eventData) { hovered = false; Refresh(); }
    public void OnPointerDown(PointerEventData eventData) { pressed = true; Refresh(); }
    public void OnPointerUp(PointerEventData eventData) { pressed = false; Refresh(); }

    private void Refresh() {
        targetScale = pressed ? pressScale : (hovered ? hoverScale : 1f);
    }
}
