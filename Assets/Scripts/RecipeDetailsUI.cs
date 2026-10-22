using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Modal that explains, step by step, how to build a toy. The step list is generated from the
/// recipe assets (see ToyRecipeDatabase) and laid out as scrollable cards, so it stays correct
/// whenever recipes change. The game is paused while the modal is open.
/// </summary>
public class RecipeDetailsUI : MonoBehaviour {

    public static RecipeDetailsUI Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject modalPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button closeButton;

    [Header("Step Template & Container (legacy, hidden at runtime)")]
    [SerializeField] private Transform stepsContainer;
    [SerializeField] private GameObject stepTemplate;

    [Header("Sprites for Recipe Steps (legacy)")]
    [SerializeField] private Sprite plasticSprite;
    [SerializeField] private Sprite metalSprite;
    [SerializeField] private Sprite meltedPlasticSprite;
    [SerializeField] private Sprite meltedMetalSprite;
    [SerializeField] private Sprite arrowSprite;

    [Header("Layout")]
    [SerializeField] private Vector2 modalSize = new Vector2(1120f, 720f);

    private readonly List<GameObject> spawnedStepObjects = new List<GameObject>();
    private bool built;
    private bool isVisible;
    private Image backdrop;
    private Image toyIconImage;
    private TextMeshProUGUI subtitleText;
    private RectTransform stepsContent;
    private ScrollRect scrollRect;
    private RectTransform modalRect;
    private Coroutine showRoutine;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("There is more than one RecipeDetailsUI instance!");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (stepTemplate != null) {
            stepTemplate.SetActive(false);
        }
        if (stepsContainer != null) {
            stepsContainer.gameObject.SetActive(false);
        }
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
        if (isVisible) GamePause.Pop();
    }

    private void Start() {
        HideImmediate();
    }

    private void Update() {
        if (!isVisible) return;
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) {
            Hide();
        }
    }

    // ------------------------------------------------------------------ build

    private void EnsureBuilt() {
        if (built) return;
        built = true;

        if (modalPanel == null) {
            modalPanel = UIFactory.CreatePanel("ModalPanel", transform, modalSize).gameObject;
        }
        modalRect = modalPanel.GetComponent<RectTransform>();
        modalRect.anchorMin = new Vector2(0.5f, 0.5f);
        modalRect.anchorMax = new Vector2(0.5f, 0.5f);
        modalRect.pivot = new Vector2(0.5f, 0.5f);
        modalRect.anchoredPosition = Vector2.zero;
        modalRect.sizeDelta = modalSize;

        Image panelImage = modalPanel.GetComponent<Image>();
        if (panelImage != null) {
            if (UIResources.Panel != null) {
                panelImage.sprite = UIResources.Panel;
                panelImage.type = Image.Type.Sliced;
                panelImage.color = Color.white;
            }
            panelImage.raycastTarget = true;
            UIFactory.FitSlicedBorders(panelImage);
        }

        // Dim backdrop behind the modal (click to close)
        backdrop = UIFactory.CreateBackdrop("Backdrop", transform, Hide);
        backdrop.transform.SetSiblingIndex(0);

        // Header
        toyIconImage = UIFactory.CreateImage("ToyIconBig", modalPanel.transform, null, Color.white, Image.Type.Simple);
        UIFactory.Anchor(toyIconImage.rectTransform, new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(110f, -95f), new Vector2(120f, 120f));
        toyIconImage.preserveAspect = true;

        if (titleText == null) {
            titleText = UIFactory.CreateText("Title", modalPanel.transform, "Recipe", 46f, UIFactory.Brown, TextAlignmentOptions.Left, true, FontStyles.Bold);
        }
        titleText.transform.SetParent(modalPanel.transform, false);
        UIFactory.Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(190f, -52f), new Vector2(760f, 56f));
        titleText.alignment = TextAlignmentOptions.Left;
        titleText.enableAutoSizing = true;
        titleText.fontSizeMax = 46f;
        titleText.fontSizeMin = 24f;

        subtitleText = UIFactory.CreateText("Subtitle", modalPanel.transform, "", 22f, UIFactory.BrownSoft, TextAlignmentOptions.Left);
        UIFactory.Anchor(subtitleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(192f, -112f), new Vector2(760f, 34f));

        // Close button (reuse the one from the scene if present)
        if (closeButton == null) {
            closeButton = UIFactory.CreateIconButton("CloseButton", modalPanel.transform, UIResources.Close, new Vector2(64f, 64f), null);
        }
        closeButton.transform.SetParent(modalPanel.transform, false);
        closeButton.transform.localScale = Vector3.one;
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        UIFactory.Anchor(closeRect, new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-58f, -58f), new Vector2(64f, 64f));
        if (closeButton.GetComponent<ButtonScaleFeedback>() == null) closeButton.gameObject.AddComponent<ButtonScaleFeedback>();
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(Hide);

        // Divider
        Image divider = UIFactory.CreateImage("Divider", modalPanel.transform, UIResources.White, new Color(UIFactory.Gold.r, UIFactory.Gold.g, UIFactory.Gold.b, 0.8f), Image.Type.Simple);
        divider.rectTransform.anchorMin = new Vector2(0.05f, 1f);
        divider.rectTransform.anchorMax = new Vector2(0.95f, 1f);
        divider.rectTransform.pivot = new Vector2(0.5f, 1f);
        divider.rectTransform.anchoredPosition = new Vector2(0f, -160f);
        divider.rectTransform.sizeDelta = new Vector2(0f, 3f);

        // Scrollable step list
        scrollRect = UIFactory.CreateVerticalScroll("Steps", modalPanel.transform, out stepsContent);
        RectTransform scrollRectTransform = scrollRect.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.045f, 0f);
        scrollRectTransform.anchorMax = new Vector2(0.955f, 1f);
        scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
        scrollRectTransform.offsetMin = new Vector2(0f, 96f);
        scrollRectTransform.offsetMax = new Vector2(0f, -172f);
        UIFactory.AddVerticalLayout(stepsContent.gameObject, 12f, new RectOffset(6, 6, 6, 6), TextAnchor.UpperCenter, true, true);

        // Footer
        TextMeshProUGUI hint = UIFactory.CreateText("Hint", modalPanel.transform, "Tap an order card any time to see its recipe", 18f, UIFactory.BrownSoft, TextAlignmentOptions.Left);
        UIFactory.Anchor(hint.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0.5f), new Vector2(60f, 50f), new Vector2(600f, 30f));

        Button gotIt = UIFactory.CreateButton("GotItButton", modalPanel.transform, "GOT IT!", new Vector2(220f, 64f), Hide, null, 30f);
        UIFactory.Anchor(gotIt.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0.5f), new Vector2(-56f, 50f), new Vector2(220f, 64f));
    }

    // ------------------------------------------------------------------ show / hide

    public void ShowRecipe(ToyObjectSO toyObjectSO) {
        if (toyObjectSO == null) return;

        ToyRecipeDatabase.ToyRecipe recipe = ToyRecipeDatabase.GetRecipeForToy(toyObjectSO);
        if (recipe == null) return;

        EnsureBuilt();

        foreach (GameObject obj in spawnedStepObjects) {
            Destroy(obj);
        }
        spawnedStepObjects.Clear();

        titleText.text = recipe.toyName + " Recipe";
        subtitleText.text = recipe.materialName + " toy  -  " + recipe.steps.Count + " steps  -  about " + Mathf.RoundToInt(recipe.GetMachineSeconds()) + "s of machine time";

        if (toyIconImage != null) {
            toyIconImage.sprite = toyObjectSO.sprite;
            toyIconImage.enabled = toyObjectSO.sprite != null;
        }

        for (int i = 0; i < recipe.steps.Count; i++) {
            spawnedStepObjects.Add(BuildStepCard(recipe.steps[i], i + 1, i == recipe.steps.Count - 1));
        }

        Show();
        if (scrollRect != null) {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private GameObject BuildStepCard(ToyRecipeDatabase.RecipeStep step, int number, bool isLast) {
        Image card = UIFactory.CreateCard("Step" + number, stepsContent, isLast ? new Color(0.84f, 0.95f, 0.84f, 0.9f) : new Color(1f, 1f, 1f, 0.62f));
        UIFactory.SetLayout(card.gameObject, -1f, 124f, 1f, -1f, 124f);
        UIFactory.AddHorizontalLayout(card.gameObject, 16f, new RectOffset(16, 18, 10, 10), TextAnchor.MiddleLeft, true, true);

        // Number badge
        Image badge = UIFactory.CreateChip("Badge", card.transform, number.ToString(), isLast ? UIFactory.Green : UIFactory.Gold, Color.white, 28f, new Vector2(46f, 46f));
        UIFactory.SetLayout(badge.gameObject, 46f, 46f, 0f, 46f, 46f);

        // Icon
        Image icon = UIFactory.CreateImage("Icon", card.transform, step.icon, Color.white, Image.Type.Simple);
        icon.preserveAspect = true;
        icon.enabled = step.icon != null;
        UIFactory.SetLayout(icon.gameObject, 88f, 88f, 0f, 88f, 88f);

        // Text column (takes all remaining width)
        RectTransform column = UIFactory.CreateRect("Text", card.transform);
        UIFactory.SetLayout(column.gameObject, -1f, 100f, 1f, 200f, -1f);
        UIFactory.AddVerticalLayout(column.gameObject, 4f, new RectOffset(0, 0, 0, 0), TextAnchor.MiddleLeft, true, true);

        TextMeshProUGUI title = UIFactory.CreateText("Title", column, step.stepName, 27f, UIFactory.Brown, TextAlignmentOptions.Left, true, FontStyles.Bold);
        title.textWrappingMode = TextWrappingModes.NoWrap;
        title.overflowMode = TextOverflowModes.Ellipsis;
        UIFactory.SetLayout(title.gameObject, -1f, 34f);

        TextMeshProUGUI desc = UIFactory.CreateText("Description", column, step.description, 18f, UIFactory.BrownSoft, TextAlignmentOptions.TopLeft);
        desc.overflowMode = TextOverflowModes.Ellipsis;
        UIFactory.SetLayout(desc.gameObject, -1f, 62f);

        // Station + key column
        RectTransform side = UIFactory.CreateRect("Side", card.transform);
        UIFactory.SetLayout(side.gameObject, 180f, 100f, 0f, 180f, -1f);
        UIFactory.AddVerticalLayout(side.gameObject, 8f, new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter, true, true);

        Image station = UIFactory.CreateChip("Station", side, step.station.ToUpperInvariant(), new Color(UIFactory.Brown.r, UIFactory.Brown.g, UIFactory.Brown.b, 0.9f), UIFactory.Cream, 16f, new Vector2(180f, 32f));
        UIFactory.SetLayout(station.gameObject, -1f, 32f);

        if (!string.IsNullOrEmpty(step.keyHint)) {
            string keyLabel = GameSettings.ShowMobileControls ? "TAP " + (step.keyHint.StartsWith("F") ? "MELT" : "ACTION") : "KEY  " + step.keyHint;
            Image key = UIFactory.CreateChip("Key", side, keyLabel, UIFactory.CreamDark, UIFactory.Brown, 16f, new Vector2(180f, 30f));
            UIFactory.SetLayout(key.gameObject, -1f, 30f);
        }

        return card.gameObject;
    }

    public void Show() {
        EnsureBuilt();
        if (!isVisible) {
            isVisible = true;
            GamePause.Push();
        }
        gameObject.SetActive(true);
        if (backdrop != null) backdrop.gameObject.SetActive(true);
        modalPanel.SetActive(true);

        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(PopIn());
    }

    private IEnumerator PopIn() {
        float duration = 0.18f;
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scale = Mathf.LerpUnclamped(0.85f, 1f, 1f - Mathf.Pow(1f - t, 3f));
            modalRect.localScale = Vector3.one * scale;
            yield return null;
        }
        modalRect.localScale = Vector3.one;
        showRoutine = null;
    }

    public void Hide() {
        if (isVisible) {
            isVisible = false;
            GamePause.Pop();
        }
        HideImmediate();
    }

    private void HideImmediate() {
        if (backdrop != null) backdrop.gameObject.SetActive(false);
        if (modalPanel != null) {
            modalPanel.SetActive(false);
        } else {
            gameObject.SetActive(false);
        }
    }

    public bool IsVisible() {
        return isVisible;
    }
}
