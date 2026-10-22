using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Score panel (top-right) plus floating feedback messages for deliveries, new orders and lost customers.</summary>
public class GameHUD : MonoBehaviour {

    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI statsText;
    private TextMeshProUGUI bestText;
    private RectTransform feedbackAnchor;

    private void Awake() {
        BuildScorePanel();

        feedbackAnchor = UIFactory.CreateRect("Feedback", transform);
        UIFactory.Anchor(feedbackAnchor, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 300f), new Vector2(10f, 10f));
    }

    private void Start() {
        if (OrderManager.Instance != null) {
            OrderManager.Instance.OnScoreChanged += OrderManager_OnScoreChanged;
            OrderManager.Instance.OnOrderSpawned += OrderManager_OnOrderSpawned;
            OrderManager.Instance.OnOrderExpired += OrderManager_OnOrderExpired;
        }
        if (DeliveryCounter.Instance != null) {
            DeliveryCounter.Instance.OnDeliveryResult += DeliveryCounter_OnDeliveryResult;
        }
        RefreshScore();
    }

    private void OnDestroy() {
        if (OrderManager.Instance != null) {
            OrderManager.Instance.OnScoreChanged -= OrderManager_OnScoreChanged;
            OrderManager.Instance.OnOrderSpawned -= OrderManager_OnOrderSpawned;
            OrderManager.Instance.OnOrderExpired -= OrderManager_OnOrderExpired;
        }
        if (DeliveryCounter.Instance != null) {
            DeliveryCounter.Instance.OnDeliveryResult -= DeliveryCounter_OnDeliveryResult;
        }
    }

    private void BuildScorePanel() {
        Image panel = UIFactory.CreatePanel("ScorePanel", transform, new Vector2(300f, 124f), false);
        UIFactory.Anchor(panel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-104f, -16f), new Vector2(300f, 124f));

        TextMeshProUGUI label = UIFactory.CreateText("Label", panel.transform, "SCORE", 18f, UIFactory.BrownSoft, TextAlignmentOptions.Left, true, FontStyles.Bold);
        UIFactory.Anchor(label.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, -14f), new Vector2(150f, 24f));

        scoreText = UIFactory.CreateText("Score", panel.transform, "0", 44f, UIFactory.Brown, TextAlignmentOptions.Left, true, FontStyles.Bold);
        UIFactory.Anchor(scoreText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, -38f), new Vector2(250f, 50f));

        bestText = UIFactory.CreateText("Best", panel.transform, "BEST 0", 16f, UIFactory.GoldDark, TextAlignmentOptions.Right, true, FontStyles.Bold);
        UIFactory.Anchor(bestText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-26f, -14f), new Vector2(140f, 24f));

        statsText = UIFactory.CreateText("Stats", panel.transform, "Delivered 0   Missed 0", 17f, UIFactory.BrownSoft, TextAlignmentOptions.Left);
        UIFactory.Anchor(statsText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(26f, 16f), new Vector2(260f, 24f));
    }

    private void OrderManager_OnScoreChanged(object sender, System.EventArgs e) {
        RefreshScore();
        StartCoroutine(Punch(scoreText.rectTransform));
    }

    private void OrderManager_OnOrderSpawned(object sender, OrderManager.OrderEventArgs e) {
        ShowFeedback("New order: " + e.order.GetToyName(), UIFactory.Blue, e.order.toyObjectSO != null ? e.order.toyObjectSO.sprite : null);
    }

    private void OrderManager_OnOrderExpired(object sender, OrderManager.OrderEventArgs e) {
        ShowFeedback("A customer left without their " + e.order.GetToyName() + "!", UIFactory.Red, null);
    }

    private void DeliveryCounter_OnDeliveryResult(object sender, DeliveryCounter.DeliveryEventArgs e) {
        ShowFeedback(e.message, e.success ? UIFactory.Green : UIFactory.Red, e.success && e.toyObjectSO != null ? e.toyObjectSO.sprite : null);
    }

    private void RefreshScore() {
        if (OrderManager.Instance == null) return;
        scoreText.text = OrderManager.Instance.GetScore().ToString("N0");
        statsText.text = "Delivered " + OrderManager.Instance.GetDeliveredCount() + "   Missed " + OrderManager.Instance.GetMissedCount();
        bestText.text = "BEST " + GameSettings.BestScore.ToString("N0");
    }

    public void ShowFeedback(string message, Color color, Sprite icon) {
        Image pill = UIFactory.CreateImage("FeedbackPill", feedbackAnchor, UIResources.White, color, Image.Type.Simple);
        UIFactory.AddHorizontalLayout(pill.gameObject, 12f, new RectOffset(22, 26, 10, 10), TextAnchor.MiddleCenter, false, true);
        ContentSizeFitter fitter = pill.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        if (icon != null) {
            Image iconImage = UIFactory.CreateImage("Icon", pill.transform, icon, Color.white, Image.Type.Simple);
            iconImage.preserveAspect = true;
            UIFactory.SetLayout(iconImage.gameObject, 44f, 44f);
        }

        TextMeshProUGUI text = UIFactory.CreateText("Text", pill.transform, message, 28f, Color.white, TextAlignmentOptions.Center, true, FontStyles.Bold);
        text.textWrappingMode = TextWrappingModes.NoWrap;
        UIFactory.SetLayout(text.gameObject, -1f, 44f);

        CanvasGroup group = pill.gameObject.AddComponent<CanvasGroup>();
        StartCoroutine(FloatAndFade(pill.rectTransform, group));
    }

    private IEnumerator FloatAndFade(RectTransform rect, CanvasGroup group) {
        float duration = 2.0f;
        float elapsed = 0f;
        Vector2 start = Vector2.zero;
        while (elapsed < duration) {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            rect.anchoredPosition = start + new Vector2(0f, 70f * t);
            group.alpha = t < 0.15f ? t / 0.15f : (t > 0.7f ? 1f - (t - 0.7f) / 0.3f : 1f);
            float scale = t < 0.15f ? Mathf.Lerp(0.7f, 1f, t / 0.15f) : 1f;
            rect.localScale = Vector3.one * scale;
            yield return null;
        }
        if (rect != null) Destroy(rect.gameObject);
    }

    private IEnumerator Punch(RectTransform rect) {
        float duration = 0.25f;
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            rect.localScale = Vector3.one * (1f + Mathf.Sin(t * Mathf.PI) * 0.25f);
            yield return null;
        }
        rect.localScale = Vector3.one;
    }
}
