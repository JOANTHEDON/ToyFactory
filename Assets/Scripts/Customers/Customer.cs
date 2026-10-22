using UnityEngine;

/// <summary>
/// One customer standing in the delivery queue. Walks to its queue slot, shows what it wants in a
/// speech bubble above its head, and walks off when served or when patience runs out.
/// </summary>
public class Customer : MonoBehaviour {

    private const string IS_WALKING = "IsWalking";

    private enum State {
        Arriving,
        Waiting,
        Leaving,
    }

    private OrderManager.Order order;
    private State state;
    private Vector3 targetPosition;
    private Transform lookAtTarget;
    private float moveSpeed = 4f;
    private Animator animator;
    private CustomerOrderBubbleUI bubble;
    private int queueIndex;
    private float leaveTimer;

    public void Setup(OrderManager.Order order, Transform visualPrefab, RuntimeAnimatorController controller, float visualScale, Color tint, float moveSpeed, Transform lookAtTarget) {
        this.order = order;
        this.moveSpeed = moveSpeed;
        this.lookAtTarget = lookAtTarget;
        targetPosition = transform.position;
        state = State.Arriving;

        if (visualPrefab != null) {
            Transform visual = Instantiate(visualPrefab, transform);
            visual.localPosition = Vector3.zero;
            visual.localRotation = Quaternion.identity;
            visual.localScale = Vector3.one * visualScale;
            visual.gameObject.name = "Visual";

            // Customers must not block the player or each other
            foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true)) {
                collider.enabled = false;
            }

            animator = visual.GetComponent<Animator>();
            if (animator == null) animator = visual.gameObject.AddComponent<Animator>();
            if (controller != null) animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            ApplyTint(visual, tint);
        } else {
            // Fallback: simple capsule so the queue is still visible
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Visual";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = new Vector3(0f, 1f * visualScale * 0.5f, 0f);
            body.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f) * visualScale;
            Destroy(body.GetComponent<Collider>());
            Renderer renderer = body.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = tint;
        }

        bubble = CustomerOrderBubbleUI.Create(transform, order, 2.1f * visualScale);
    }

    private void ApplyTint(Transform visual, Color tint) {
        foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>()) {
            foreach (Material material in renderer.materials) {
                if (material == null) continue;
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", tint);
                if (material.HasProperty("_Color")) material.SetColor("_Color", tint);
            }
        }
    }

    public void SetQueueTarget(Vector3 position, int queueIndex) {
        if (state == State.Leaving) return;
        this.queueIndex = queueIndex;
        targetPosition = position;
        if (bubble != null) bubble.SetQueueNumber(queueIndex + 1);
    }

    public void Leave(bool happy, Vector3 exitPosition) {
        state = State.Leaving;
        targetPosition = exitPosition;
        leaveTimer = 0f;
        if (bubble != null) bubble.ShowReaction(happy);
    }

    private void Update() {
        if (state == State.Leaving) {
            leaveTimer += Time.deltaTime;
            // Pause a moment so the player can read the reaction, then walk out
            if (leaveTimer < 1.1f) {
                SetWalking(false);
                return;
            }
        }

        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        if (distance > 0.08f) {
            Vector3 direction = toTarget / distance;
            float step = Mathf.Min(distance, moveSpeed * Time.deltaTime);
            transform.position += direction * step;
            transform.forward = Vector3.Slerp(transform.forward, direction, Time.deltaTime * 12f);
            SetWalking(true);
        } else {
            SetWalking(false);
            if (state == State.Leaving) {
                Destroy(gameObject);
                return;
            }
            if (state == State.Arriving) state = State.Waiting;

            // Face the counter while waiting
            if (lookAtTarget != null) {
                Vector3 look = lookAtTarget.position - transform.position;
                look.y = 0f;
                if (look.sqrMagnitude > 0.01f) {
                    transform.forward = Vector3.Slerp(transform.forward, look.normalized, Time.deltaTime * 6f);
                }
            }
        }
    }

    private void SetWalking(bool walking) {
        if (animator != null && animator.runtimeAnimatorController != null) {
            animator.SetBool(IS_WALKING, walking);
        }
    }

    public OrderManager.Order GetOrder() {
        return order;
    }

    public int GetQueueIndex() {
        return queueIndex;
    }
}
