using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World-space speech bubble above a customer: the toy they want, its name, their queue number,
/// and a patience bar that drains and turns red. Always faces the camera.
/// Styled with an attractive polished card, soft drop shadow, and crisp readability.
/// </summary>
public class CustomerOrderBubbleUI : MonoBehaviour {

    private OrderManager.Order order;
    private Image fill;
    private Image icon;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI queueText;
    private TextMeshProUGUI timeText;
    private Image panelOuter;
    private Image panelInner;
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
        rect.sizeDelta = new Vector2(320f, 210f);
        rect.pivot = new Vector2(0.5f, 0f);

        CustomerOrderBubbleUI bubble = go.AddComponent<CustomerOrderBubbleUI>();
        bubble.Build(order);
        return bubble;
    }

    private void Build(OrderManager.Order order) {
        this.order = order;
        root = GetComponent<RectTransform>();

        // Soft drop shadow behind bubble
        Image shadow = UIFactory.CreateImage("Shadow", transform, UIResources.White, new Color(0f, 0f, 0f, 0.28f), Image.Type.Simple);
        UIFactory.Anchor(shadow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -8f), new Vector2(320f, 210f));

        // Outer border frame (warm dark chocolate outline)
        panelOuter = UIFactory.CreateImage("PanelOuter", transform, UIResources.White, UIFactory.Brown, Image.Type.Simple);
        UIFactory.Anchor(panelOuter.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320f, 210f));

        // Inner card body (warm glossy cream background)
        panelInner = UIFactory.CreateImage("PanelInner", panelOuter.transform, UIResources.White, UIFactory.Cream, Image.Type.Simple);
        UIFactory.Stretch(panelInner.rectTransform, 4f, 4f, 4f, 4f);

        // Header ribbon with queue badge and title
        Image headerRibbon = UIFactory.CreateImage("HeaderRibbon", panelInner.transform, UIResources.White, new Color(UIFactory.Brown.r, UIFactory.Brown.g, UIFactory.Brown.b, 0.08f), Image.Type.Simple);
        UIFactory.Anchor(headerRibbon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(312f, 46f));

        // Queue number badge, top-left
        Image badge = UIFactory.CreateChip("Queue", headerRibbon.transform, "#1", UIFactory.Gold, Color.white, 22f, new Vector2(46f, 36f));
        UIFactory.Anchor(badge.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(46f, 36f));
        queueText = badge.GetComponentInChildren<TextMeshProUGUI>();

        // Timer badge in header, top-right
        timeText = UIFactory.CreateText("TimeRemaining", headerRibbon.transform, "", 20f, UIFactory.BrownSoft, TextAlignmentOptions.Right, false, FontStyles.Bold);
        UIFactory.Anchor(timeText.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(100f, 36f));

        // Little speech tail under the bubble pointing towards the customer
        Image tailShadow = UIFactory.CreateImage("TailShadow", transform, UIResources.White, new Color(0f, 0f, 0f, 0.28f), Image.Type.Simple);
        UIFactory.Anchor(tailShadow.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, -8f), new Vector2(28f, 28f));
        tailShadow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

        Image tail = UIFactory.CreateImage("Tail", transform, UIResources.White, UIFactory.Brown, Image.Type.Simple);
        UIFactory.Anchor(tail.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(26f, 26f));
        tail.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

        Image tailInner = UIFactory.CreateImage("TailInner", tail.transform, UIResources.White, UIFactory.Cream, Image.Type.Simple);
        UIFactory.Stretch(tailInner.rectTransform, 3f, 3f, 3f, 3f);

        // Circular frame for the toy icon
        Image iconFrame = UIFactory.CreateImage("IconFrame", panelInner.transform, UIResources.White, new Color(1f, 1f, 1f, 0.9f), Image.Type.Simple);
        UIFactory.Anchor(iconFrame.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(92f, 92f));

        icon = UIFactory.CreateImage("Icon", iconFrame.transform, order.toyObjectSO != null ? order.toyObjectSO.sprite : null, Color.white, Image.Type.Simple);
        icon.preserveAspect = true;
        UIFactory.Stretch(icon.rectTransform, 4f, 4f, 4f, 4f);

        // Toy Name
        nameText = UIFactory.CreateText("Name", panelInner.transform, order.GetToyName(), 26f, UIFactory.Brown, TextAlignmentOptions.Center, true, FontStyles.Bold);
        UIFactory.Anchor(nameText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 32f), new Vector2(290f, 30f));
        nameText.enableAutoSizing = true;
        nameText.fontSizeMax = 26f;
        nameText.fontSizeMin = 16f;

        // Patience bar at bottom
        Image bar = UIFactory.CreateProgressBar("Patience", panelInner.transform, new Vector2(280f, 14f), new Color(0f, 0f, 0f, 0.18f), UIFactory.Green, out fill);
        UIFactory.Anchor(bar.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(280f, 14f));

        Refresh();
    }

    public void SetQueueNumber(int number) {
        if (queueText != null) queueText.text = "#" + number;
    }

    public void ShowReaction(bool happy) {
        reacting = true;
        if (icon != null && icon.transform.parent != null) icon.transform.parent.gameObject.SetActive(false);
        if (fill != null && fill.transform.parent != null) fill.transform.parent.gameObject.SetActive(false);
        if (queueText != null && queueText.transform.parent != null) queueText.transform.parent.gameObject.SetActive(false);
        if (timeText != null) timeText.gameObject.SetActive(false);

        if (nameText != null) {
            nameText.text = happy ? "THANKS!" : "TOO SLOW!";
            nameText.color = happy ? UIFactory.Green : UIFactory.Red;
            nameText.fontSizeMax = 38f;
            UIFactory.Anchor(nameText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280f, 80f));
        }

        if (panelInner != null) {
            panelInner.color = happy ? new Color(0.88f, 1f, 0.88f) : new Color(1f, 0.88f, 0.88f);
        }
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

        if (timeText != null) {
            int secondsLeft = Mathf.CeilToInt(order.timeRemaining);
            timeText.text = secondsLeft + "s";
            timeText.color = t > 0.25f ? UIFactory.BrownSoft : UIFactory.Red;
        }

        if (root != null) {
            float pulse = order.IsUrgent() ? 1f + Mathf.Sin(Time.time * 9f) * 0.05f : 1f;
            root.localScale = Vector3.one * 0.012f * pulse;
        }
    }
}
