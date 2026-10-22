using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World-space speech bubble above a customer: the toy they want, its name, their queue number,
/// and a patience bar that drains and turns red. Always faces the camera.
/// </summary>
public class CustomerOrderBubbleUI : MonoBehaviour {

    private OrderManager.Order order;
    private Image fill;
    private Image icon;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI queueText;
    private Image panel;
    private RectTransform root;
    private bool reacting;

    public static CustomerOrderBubbleUI Create(Transform parent, OrderManager.Order order, float height) {
        GameObject go = new GameObject("OrderBubble", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0f, height, 0f);
        go.transform.localScale = Vector3.one * 0.012f;

        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300f, 190f);
        rect.pivot = new Vector2(0.5f, 0f);

        CustomerOrderBubbleUI bubble = go.AddComponent<CustomerOrderBubbleUI>();
        bubble.Build(order);
        return bubble;
    }

    private void Build(OrderManager.Order order) {
        this.order = order;
        root = GetComponent<RectTransform>();

        panel = UIFactory.CreatePanel("Panel", transform, new Vector2(300f, 190f), false);
        UIFactory.Stretch(panel.rectTransform);

        // Little tail under the bubble
        Image tail = UIFactory.CreateImage("Tail", transform, UIResources.White, UIResources.Panel != null ? new Color(0.98f, 0.93f, 0.8f) : UIFactory.Cream, Image.Type.Simple);
        UIFactory.Anchor(tail.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 2f), new Vector2(26f, 26f));
        tail.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

        icon = UIFactory.CreateImage("Icon", transform, order.toyObjectSO != null ? order.toyObjectSO.sprite : null, Color.white, Image.Type.Simple);
        icon.preserveAspect = true;
        UIFactory.Anchor(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(96f, 96f));

        nameText = UIFactory.CreateText("Name", transform, order.GetToyName(), 28f, UIFactory.Brown, TextAlignmentOptions.Center, true, FontStyles.Bold);
        UIFactory.Anchor(nameText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 46f), new Vector2(280f, 36f));
        nameText.enableAutoSizing = true;
        nameText.fontSizeMax = 28f;
        nameText.fontSizeMin = 16f;

        Image badge = UIFactory.CreateChip("Queue", transform, "#1", UIFactory.Gold, Color.white, 22f, new Vector2(44f, 44f));
        UIFactory.Anchor(badge.rectTransform, new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(22f, -22f), new Vector2(44f, 44f));
        queueText = badge.GetComponentInChildren<TextMeshProUGUI>();

        Image bar = UIFactory.CreateProgressBar("Patience", transform, new Vector2(240f, 16f), new Color(0f, 0f, 0f, 0.22f), UIFactory.Green, out fill);
        UIFactory.Anchor(bar.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(240f, 16f));

        Refresh();
    }

    public void SetQueueNumber(int number) {
        if (queueText != null) queueText.text = "#" + number;
    }

    public void ShowReaction(bool happy) {
        reacting = true;
        if (icon != null) icon.gameObject.SetActive(false);
        if (fill != null) fill.transform.parent.gameObject.SetActive(false);
        if (queueText != null) queueText.transform.parent.gameObject.SetActive(false);
        if (nameText != null) {
            nameText.text = happy ? "THANKS!" : "TOO SLOW!";
            nameText.color = happy ? UIFactory.Green : UIFactory.Red;
            nameText.fontSizeMax = 40f;
            UIFactory.Anchor(nameText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), new Vector2(280f, 80f));
        }
        if (panel != null && !happy) panel.color = new Color(1f, 0.85f, 0.85f);
        if (panel != null && happy) panel.color = new Color(0.88f, 1f, 0.88f);
    }

    private void LateUpdate() {
        Camera camera = Camera.main;
        if (camera != null) {
            transform.forward = camera.transform.forward;
        }
        if (!reacting) Refresh();
    }

    private void Refresh() {
        if (order == null || fill == null) return;
        float t = order.GetTimeNormalized();
        fill.fillAmount = t;
        fill.color = t > 0.5f ? UIFactory.Green : (t > 0.25f ? UIFactory.Gold : UIFactory.Red);

        if (root != null) {
            float pulse = order.IsUrgent() ? 1f + Mathf.Sin(Time.time * 9f) * 0.05f : 1f;
            root.localScale = Vector3.one * 0.012f * pulse;
        }
    }
}
