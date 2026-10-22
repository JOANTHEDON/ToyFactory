using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Polished, animated loading screen overlay for the game.
/// Displays the game logo, a styled progress bar, animated spinner/dots,
/// and smooth canvas group fade out when initial load completes.
/// </summary>
public class LoadingScreenUI : MonoBehaviour {

    [Header("UI Components")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image logoImage;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private Transform spinnerTransform;

    [Header("Settings")]
    [SerializeField] private float minimumDisplayDuration = 2.0f;
    [SerializeField] private float fadeDuration = 0.6f;

    private readonly string[] funTips = new string[] {
        "Tip: Deliver matching toys to customers to complete orders!",
        "Tip: Use the Wood Shaper to carve adorable wooden toys!",
        "Tip: The Molten Forge melts raw materials ready for molding!",
        "Tip: Tap the orders in the top bar to inspect required recipe steps!"
    };

    private void Awake() {
        if (canvasGroup == null) {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        // Ensure visible and blocking raycasts during load
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (tipText != null && funTips.Length > 0) {
            tipText.text = funTips[Random.Range(0, funTips.Length)];
        }
    }

    private void Start() {
        StartCoroutine(LoadingRoutine());
    }

    private void Update() {
        if (spinnerTransform != null) {
            spinnerTransform.Rotate(0f, 0f, -180f * Time.unscaledDeltaTime);
        }
    }

    private IEnumerator LoadingRoutine() {
        float elapsed = 0f;

        while (elapsed < minimumDisplayDuration) {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / minimumDisplayDuration);

            // Ease progress for smooth visuals
            float smoothedProgress = Mathf.SmoothStep(0f, 1f, progress);

            if (progressBarFill != null) {
                progressBarFill.fillAmount = smoothedProgress;
            }

            if (statusText != null) {
                int percent = Mathf.RoundToInt(smoothedProgress * 100f);
                int dots = (int)(elapsed * 3f) % 4;
                string dotStr = new string('.', dots);
                statusText.text = $"Setting up workshop... {percent}%{dotStr}";
            }

            yield return null;
        }

        if (progressBarFill != null) progressBarFill.fillAmount = 1f;
        if (statusText != null) statusText.text = "Workshop Ready!";

        yield return new WaitForSecondsRealtime(0.25f);

        // Smooth fade out
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeDuration) {
            fadeElapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);
            if (canvasGroup != null) {
                canvasGroup.alpha = alpha;
            }
            yield return null;
        }

        if (canvasGroup != null) {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        gameObject.SetActive(false);
    }
}
