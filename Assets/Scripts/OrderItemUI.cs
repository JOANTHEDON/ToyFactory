using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>One order card in the top bar: icon, name, queue number and a live patience bar.</summary>
public class OrderItemUI : MonoBehaviour {

    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI toyNameText;
    [SerializeField] private Button recipeButton;

    private ToyObjectSO toyObjectSO;
    private OrderManager.Order order;
    private Image timerFill;
    private Image timerBack;
    private TextMeshProUGUI queueText;
    private RectTransform rectTransform;
    private bool chromeBuilt;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();

        if (iconImage == null) {
            Transform iconT = transform.Find("ToyIcon") ?? transform.Find("icon") ?? transform.Find("Icon");
            if (iconT != null) {
                iconImage = iconT.GetComponent<Image>();
            } else {
                Image buttonImg = GetComponent<Image>();
                Image[] allImages = GetComponentsInChildren<Image>(true);
                foreach (Image img in allImages) {
                    if (img != buttonImg) {
                        iconImage = img;
                        break;
                    }
                }
            }
        }

        if (toyNameText == null) {
            toyNameText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (recipeButton == null) {
            recipeButton = GetComponent<Button>();
        }
    }

    private void BuildChrome() {
        if (chromeBuilt) return;
        chromeBuilt = true;

        // Flat cream card with a soft outline and drop shadow
        Image cardImage = GetComponent<Image>();
        if (cardImage != null) {
            cardImage.sprite = UIResources.White;
            cardImage.type = Image.Type.Simple;
            cardImage.color = new Color(UIFactory.Brown.r, UIFactory.Brown.g, UIFactory.Brown.b, 0.85f);

            Image fill = UIFactory.CreateImage("Fill", transform, UIResources.White, UIFactory.Cream, Image.Type.Simple, false);
            UIFactory.Stretch(fill.rectTransform, 3f, 3f, 3f, 3f);
            fill.transform.SetAsFirstSibling();

            // Inner icon frame to give depth and highlight
            Image iconBg = UIFactory.CreateImage("IconBg", fill.transform, UIResources.White, new Color(1f, 1f, 1f, 0.95f), Image.Type.Simple, false);
            UIFactory.Anchor(iconBg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 14f), new Vector2(88f, 88f));
            iconBg.transform.SetAsFirstSibling();
        }

        // Queue number badge, top-left
        Image badge = UIFactory.CreateChip("QueueBadge", transform, "#1", UIFactory.Gold, Color.white, 20f, new Vector2(38f, 38f));
        UIFactory.Anchor(badge.rectTransform, new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(22f, -22f), new Vector2(38f, 38f));
        queueText = badge.GetComponentInChildren<TextMeshProUGUI>();

        // Patience bar at the bottom
        timerBack = UIFactory.CreateProgressBar("TimerBar", transform, new Vector2(10f, 10f), new Color(0f, 0f, 0f, 0.25f), UIFactory.Green, out timerFill);
        timerBack.rectTransform.anchorMin = new Vector2(0.1f, 0f);
        timerBack.rectTransform.anchorMax = new Vector2(0.9f, 0f);
        timerBack.rectTransform.pivot = new Vector2(0.5f, 0f);
        timerBack.rectTransform.anchoredPosition = new Vector2(0f, 10f);
        timerBack.rectTransform.sizeDelta = new Vector2(0f, 10f);

        if (recipeButton != null && recipeButton.GetComponent<ButtonScaleFeedback>() == null) {
            recipeButton.gameObject.AddComponent<ButtonScaleFeedback>();
        }
    }

    public void SetOrder(OrderManager.Order order, int queueNumber) {
        this.order = order;
        SetToyObjectSO(order != null ? order.toyObjectSO : null);
        BuildChrome();
        if (queueText != null) queueText.text = "#" + queueNumber;
        RefreshTimer();
    }

    public void SetToyObjectSO(ToyObjectSO toyObjectSO) {
        if (iconImage == null || toyNameText == null || recipeButton == null) {
            Awake();
        }

        this.toyObjectSO = toyObjectSO;

        if (toyObjectSO == null) {
            if (toyNameText != null) toyNameText.text = "";
            if (iconImage != null) iconImage.gameObject.SetActive(false);
            return;
        }

        string displayName = ToyObjectSO.GetDisplayName(toyObjectSO);

        if (toyNameText != null) {
            toyNameText.text = displayName;
        }

        if (iconImage != null) {
            if (toyObjectSO.sprite != null) {
                iconImage.sprite = toyObjectSO.sprite;
                iconImage.preserveAspect = true;
                iconImage.gameObject.SetActive(true);
            } else {
                iconImage.gameObject.SetActive(false);
            }
        }

        // Setup button click
        Button targetButton = recipeButton != null ? recipeButton : GetComponent<Button>();
        if (targetButton != null) {
            targetButton.onClick.RemoveAllListeners();
            targetButton.onClick.AddListener(() => {
                if (RecipeDetailsUI.Instance != null) {
                    RecipeDetailsUI.Instance.ShowRecipe(toyObjectSO);
                }
            });
        }
    }

    private void Update() {
        RefreshTimer();
    }

    private void RefreshTimer() {
        if (order == null || timerFill == null) return;

        float t = order.GetTimeNormalized();
        timerFill.fillAmount = t;
        timerFill.color = t > 0.5f ? UIFactory.Green : (t > 0.25f ? UIFactory.Gold : UIFactory.Red);

        // Pulse the whole card when the customer is about to leave
        if (rectTransform != null) {
            float pulse = order.IsUrgent() ? 1f + Mathf.Sin(Time.unscaledTime * 10f) * 0.03f : 1f;
            rectTransform.localScale = Vector3.one * pulse;
        }
    }

    public OrderManager.Order GetOrder() {
        return order;
    }
}
