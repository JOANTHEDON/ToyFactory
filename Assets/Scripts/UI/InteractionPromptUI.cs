using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Floating prompt above the counter the player is facing: "[E] Pick up Wood", "[F] Melt (3/10)", "Molding...".
/// Text comes from each counter's GetInteractHint / GetInteractAlternateHint / GetBusyHint, so it is always accurate.
/// </summary>
public class InteractionPromptUI : MonoBehaviour {

    [SerializeField] private float worldHeightOffset = 3.6f;
    [SerializeField] private string interactKeyLabel = "E";
    [SerializeField] private string alternateKeyLabel = "F";

    private RectTransform canvasRect;
    private RectTransform root;
    private GameObject primaryPill;
    private GameObject alternatePill;
    private GameObject busyPill;
    private GameObject primaryKey;
    private GameObject alternateKey;
    private TextMeshProUGUI primaryLabel;
    private TextMeshProUGUI alternateLabel;
    private TextMeshProUGUI busyLabel;
    private Camera mainCamera;

    private void Awake() {
        // Ensure OrderNavigationUI is active on the Canvas to guide first 4 orders
        if (OrderNavigationUI.Instance == null && transform.parent != null) {
            transform.parent.gameObject.AddComponent<OrderNavigationUI>();
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;

        root = UIFactory.CreateRect("PromptRoot", transform);
        root.sizeDelta = new Vector2(10f, 10f);
        UIFactory.AddVerticalLayout(root.gameObject, 6f, new RectOffset(0, 0, 0, 0), TextAnchor.LowerCenter, false, false);
        ContentSizeFitter rootFitter = root.gameObject.AddComponent<ContentSizeFitter>();
        rootFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        rootFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        busyPill = BuildPill("BusyPill", null, "Working...", new Color(0.2f, 0.2f, 0.2f, 0.8f), UIFactory.Cream, out busyLabel, out GameObject unusedKey);
        alternatePill = BuildPill("AlternatePill", alternateKeyLabel, "Melt", new Color(UIFactory.Brown.r, UIFactory.Brown.g, UIFactory.Brown.b, 0.92f), UIFactory.Cream, out alternateLabel, out alternateKey);
        primaryPill = BuildPill("PrimaryPill", interactKeyLabel, "Pick up", new Color(UIFactory.Cream.r, UIFactory.Cream.g, UIFactory.Cream.b, 0.96f), UIFactory.Brown, out primaryLabel, out primaryKey);

        root.gameObject.SetActive(false);
    }

    private GameObject BuildPill(string name, string keyLabel, string text, Color back, Color textColor, out TextMeshProUGUI label, out GameObject keyChip) {
        Image pill = UIFactory.CreateImage(name, root, UIResources.White, back, Image.Type.Simple);
        UIFactory.AddHorizontalLayout(pill.gameObject, 10f, new RectOffset(12, 16, 8, 8), TextAnchor.MiddleLeft, false, true);
        ContentSizeFitter fitter = pill.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        keyChip = null;
        if (!string.IsNullOrEmpty(keyLabel)) {
            Image chip = UIFactory.CreateChip("Key", pill.transform, keyLabel, textColor, back.a < 0.95f ? UIFactory.Brown : UIFactory.Cream, 22f, new Vector2(38f, 38f));
            UIFactory.SetLayout(chip.gameObject, 38f, 38f);
            keyChip = chip.gameObject;
        }

        label = UIFactory.CreateText("Label", pill.transform, text, 26f, textColor, TextAlignmentOptions.Left, true, FontStyles.Bold);
        label.textWrappingMode = TextWrappingModes.NoWrap;
        UIFactory.SetLayout(label.gameObject, -1f, 38f);
        return pill.gameObject;
    }

    private void LateUpdate() {
        Player player = Player.Instance;
        BaseCounter counter = player != null ? player.GetSelectedCounter() : null;

        if (player == null || counter == null || GamePause.IsPaused) {
            root.gameObject.SetActive(false);
            return;
        }

        string primary = counter.GetInteractHint(player);
        string alternate = counter.GetInteractAlternateHint(player);
        string busy = counter.GetBusyHint();

        bool anything = primary != null || alternate != null || busy != null;
        root.gameObject.SetActive(anything);
        if (!anything) return;

        bool showKeys = !GameSettings.ShowMobileControls;

        primaryPill.SetActive(primary != null);
        if (primary != null) {
            primaryLabel.text = primary;
            primaryKey.SetActive(showKeys);
        }

        alternatePill.SetActive(alternate != null);
        if (alternate != null) {
            alternateLabel.text = alternate;
            alternateKey.SetActive(showKeys);
        }

        busyPill.SetActive(busy != null);
        if (busy != null) busyLabel.text = busy;

        // Follow the counter on screen
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null || canvasRect == null) return;

        Vector3 worldPosition = counter.transform.position + Vector3.up * worldHeightOffset;
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPosition);
        if (screenPoint.z < 0f) {
            root.gameObject.SetActive(false);
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint)) {
            root.anchoredPosition = localPoint + new Vector2(0f, 10f);
        }
    }
}
