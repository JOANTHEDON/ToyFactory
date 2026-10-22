using TMPro;
using UnityEngine;

/// <summary>
/// Per-scene bag of sprites and fonts used by code-built UI (recipe modal, HUD, pause menu, start menu, customer bubbles).
/// Every field is optional: when a sprite is missing the UI falls back to flat colored rectangles.
/// </summary>
public class UIResources : MonoBehaviour {

    private static UIResources instance;

    /// <summary>Resolved lazily so UI that builds itself in Awake works regardless of initialization order.</summary>
    public static UIResources Instance {
        get {
            if (instance == null) {
                instance = FindFirstObjectByType<UIResources>(FindObjectsInactive.Include);
            }
            return instance;
        }
        private set { instance = value; }
    }

    [Header("Panels")]
    public Sprite panelSprite;
    public Sprite cardSprite;
    public Sprite whiteSprite;

    [Header("Buttons")]
    public Sprite buttonSprite;
    public Sprite buttonChevronSprite;
    public Sprite closeSprite;
    public Sprite arrowSprite;
    public Sprite knobSprite;

    [Header("Icons")]
    public Sprite logoSprite;
    public Sprite deliverySprite;
    public Sprite pickupSprite;
    public Sprite dropSprite;

    [Header("Fonts")]
    public TMP_FontAsset titleFont;
    public TMP_FontAsset bodyFont;

    private void Awake() {
        Instance = this;
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    public static Sprite Panel => Instance != null ? Instance.panelSprite : null;
    public static Sprite Card => Instance != null ? Instance.cardSprite : null;
    public static Sprite White => Instance != null ? Instance.whiteSprite : null;
    public static Sprite ButtonBg => Instance != null ? Instance.buttonSprite : null;
    public static Sprite ButtonChevron => Instance != null ? Instance.buttonChevronSprite : null;
    public static Sprite Close => Instance != null ? Instance.closeSprite : null;
    public static Sprite Arrow => Instance != null ? Instance.arrowSprite : null;
    public static Sprite Knob => Instance != null ? Instance.knobSprite : null;
    public static Sprite Logo => Instance != null ? Instance.logoSprite : null;
    public static Sprite Delivery => Instance != null ? Instance.deliverySprite : null;

    public static TMP_FontAsset TitleFont {
        get {
            if (Instance != null && Instance.titleFont != null) return Instance.titleFont;
            return BodyFont;
        }
    }

    public static TMP_FontAsset BodyFont {
        get {
            if (Instance != null && Instance.bodyFont != null) return Instance.bodyFont;
            return TMP_Settings.defaultFontAsset;
        }
    }
}
