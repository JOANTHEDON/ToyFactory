using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Small toolkit for building consistent, good-looking uGUI in code.
/// All colors come from one warm "toy workshop" palette so every screen reads as one system.
/// </summary>
public static class UIFactory {

    // Palette
    public static readonly Color Cream = Hex("#FFF6E5");
    public static readonly Color CreamDark = Hex("#F1E3C8");
    public static readonly Color Brown = Hex("#4A2E1B");
    public static readonly Color BrownSoft = Hex("#7A5A3C");
    public static readonly Color Gold = Hex("#F2B134");
    public static readonly Color GoldDark = Hex("#C98A12");
    public static readonly Color Green = Hex("#43A047");
    public static readonly Color Red = Hex("#E04E4E");
    public static readonly Color Blue = Hex("#3E8ED0");
    public static readonly Color Navy = Hex("#1E2A3A");
    public static readonly Color Dim = new Color(0f, 0f, 0f, 0.6f);

    public static Color Hex(string hex) {
        return ColorUtility.TryParseHtmlString(hex, out Color c) ? c : Color.magenta;
    }

    // ---------- Rect helpers ----------

    public static RectTransform CreateRect(string name, Transform parent) {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = parent != null ? parent.gameObject.layer : LayerMask.NameToLayer("UI");
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(100f, 100f);
        rt.localScale = Vector3.one;
        return rt;
    }

    public static void Stretch(RectTransform rt, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f) {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    public static void Anchor(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size) {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    // ---------- Primitives ----------

    public static Image CreateImage(string name, Transform parent, Sprite sprite, Color color, Image.Type type = Image.Type.Sliced, bool raycastTarget = false) {
        RectTransform rt = CreateRect(name, parent);
        Image image = rt.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.type = sprite != null ? type : Image.Type.Simple;
        image.raycastTarget = raycastTarget;
        image.preserveAspect = false;
        return image;
    }

    /// <summary>
    /// The workshop sprites are large and ornate: their 9-slice borders can be bigger than the rect we draw them in,
    /// which makes Unity collapse the image. Scale the borders down so they always fit the current rect size.
    /// </summary>
    public static void FitSlicedBorders(Image image) {
        if (image == null || image.sprite == null || image.type != Image.Type.Sliced) return;
        Vector4 border = image.sprite.border;
        Vector2 size = image.rectTransform.rect.size;
        if (size.x <= 0f || size.y <= 0f) size = image.rectTransform.sizeDelta;
        if (size.x <= 0f || size.y <= 0f) return;
        float needWidth = border.x + border.z;
        float needHeight = border.y + border.w;
        float multiplier = Mathf.Max(1f, needWidth / (size.x * 0.75f), needHeight / (size.y * 0.75f));
        image.pixelsPerUnitMultiplier = multiplier;
    }

    /// <summary>
    /// Cream panel. Big panels use the ornate workshop sprite; small ones (cards, HUD boxes) use a flat
    /// outlined rectangle, because the sprite's decorative borders need at least ~180px to look right.
    /// </summary>
    public static Image CreatePanel(string name, Transform parent, Vector2 size, bool raycastTarget = true) {
        bool useSprite = UIResources.Panel != null && size.x >= 180f && size.y >= 180f;
        if (!useSprite) {
            return CreateFlatPanel(name, parent, size, Cream, new Color(Brown.r, Brown.g, Brown.b, 0.55f), raycastTarget);
        }
        Image panel = CreateImage(name, parent, UIResources.Panel, Color.white, Image.Type.Sliced, raycastTarget);
        panel.rectTransform.sizeDelta = size;
        FitSlicedBorders(panel);
        return panel;
    }

    /// <summary>Flat panel: outline rectangle with an inset fill. Children should be parented to the returned image.</summary>
    public static Image CreateFlatPanel(string name, Transform parent, Vector2 size, Color fill, Color outline, bool raycastTarget = true) {
        Image outer = CreateImage(name, parent, UIResources.White, outline, Image.Type.Simple, raycastTarget);
        outer.rectTransform.sizeDelta = size;
        Image inner = CreateImage("Fill", outer.transform, UIResources.White, fill, Image.Type.Simple, false);
        Stretch(inner.rectTransform, 3f, 3f, 3f, 3f);
        return outer;
    }

    /// <summary>Flat card (white sprite tinted) for list rows.</summary>
    public static Image CreateCard(string name, Transform parent, Color color) {
        return CreateImage(name, parent, UIResources.White, color, Image.Type.Simple, false);
    }

    public static Image CreateBackdrop(string name, Transform parent, UnityAction onClick = null) {
        Image backdrop = CreateImage(name, parent, null, Dim, Image.Type.Simple, true);
        Stretch(backdrop.rectTransform);
        if (onClick != null) {
            Button button = backdrop.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(onClick);
        }
        return backdrop;
    }

    public static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center, bool titleFont = false, FontStyles style = FontStyles.Normal) {
        RectTransform rt = CreateRect(name, parent);
        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        TMP_FontAsset font = titleFont ? UIResources.TitleFont : UIResources.BodyFont;
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.fontStyle = style;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Overflow;
        return tmp;
    }

    /// <summary>Chunky flat button: dark base, raised gold face, bold label. Works on any background.</summary>
    public static Button CreateButton(string name, Transform parent, string label, Vector2 size, UnityAction onClick, Color? tint = null, float fontSize = 34f, Color? labelColor = null) {
        Color face = tint ?? Gold;
        Color shadow = new Color(face.r * 0.62f, face.g * 0.55f, face.b * 0.4f, 1f);

        Image bg = CreateImage(name, parent, UIResources.White, shadow, Image.Type.Simple, true);
        bg.rectTransform.sizeDelta = size;

        Image top = CreateImage("Face", bg.transform, UIResources.White, face, Image.Type.Simple, false);
        Stretch(top.rectTransform, 0f, 0f, 0f, Mathf.Clamp(size.y * 0.09f, 4f, 8f));

        Button button = bg.gameObject.AddComponent<Button>();
        button.targetGraphic = top;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.08f, 1.08f, 1.0f, 1f);
        colors.pressedColor = new Color(0.86f, 0.86f, 0.86f, 1f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        if (onClick != null) button.onClick.AddListener(onClick);
        bg.gameObject.AddComponent<ButtonScaleFeedback>();

        TextMeshProUGUI text = CreateText("Label", top.transform, label, fontSize, labelColor ?? Brown, TextAlignmentOptions.Center, true, FontStyles.Bold);
        Stretch(text.rectTransform, 16f, 16f, 4f, 4f);
        text.enableAutoSizing = true;
        text.fontSizeMax = fontSize;
        text.fontSizeMin = Mathf.Max(12f, fontSize * 0.5f);
        return button;
    }

    public static Button CreateIconButton(string name, Transform parent, Sprite icon, Vector2 size, UnityAction onClick, Color? tint = null) {
        Image image = CreateImage(name, parent, icon, tint ?? Color.white, Image.Type.Simple, true);
        if (icon == null) image.color = Gold;
        image.preserveAspect = true;
        image.rectTransform.sizeDelta = size;
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        if (onClick != null) button.onClick.AddListener(onClick);
        image.gameObject.AddComponent<ButtonScaleFeedback>();
        return button;
    }

    public static Image CreateProgressBar(string name, Transform parent, Vector2 size, Color backColor, Color fillColor, out Image fill) {
        Image back = CreateImage(name, parent, UIResources.White, backColor, Image.Type.Simple, false);
        back.rectTransform.sizeDelta = size;
        fill = CreateImage("Fill", back.transform, UIResources.White, fillColor, Image.Type.Simple, false);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 1f;
        Stretch(fill.rectTransform, 2f, 2f, 2f, 2f);
        return back;
    }

    public static Slider CreateSlider(string name, Transform parent, Vector2 size, float value, UnityAction<float> onChanged) {
        RectTransform root = CreateRect(name, parent);
        root.sizeDelta = size;

        Image background = CreateImage("Background", root, UIResources.White, CreamDark, Image.Type.Simple, false);
        Stretch(background.rectTransform, 0f, 0f, size.y * 0.3f, size.y * 0.3f);

        RectTransform fillArea = CreateRect("Fill Area", root);
        Stretch(fillArea, 0f, 0f, size.y * 0.3f, size.y * 0.3f);
        Image fill = CreateImage("Fill", fillArea, UIResources.White, Gold, Image.Type.Simple, false);
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = new Vector2(0f, 1f);
        fill.rectTransform.sizeDelta = Vector2.zero;

        RectTransform handleArea = CreateRect("Handle Slide Area", root);
        Stretch(handleArea, size.y * 0.5f, size.y * 0.5f, 0f, 0f);
        Image handle = CreateImage("Handle", handleArea, UIResources.Knob, Color.white, Image.Type.Simple, true);
        if (UIResources.Knob == null) handle.color = GoldDark;
        handle.rectTransform.sizeDelta = new Vector2(size.y, size.y);

        Slider slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = value;
        if (onChanged != null) slider.onValueChanged.AddListener(onChanged);
        return slider;
    }

    /// <summary>Small pill with a label, e.g. "WOOD CUTTER" or a key cap "E".</summary>
    public static Image CreateChip(string name, Transform parent, string label, Color back, Color textColor, float fontSize = 20f, Vector2? size = null) {
        Image chip = CreateImage(name, parent, UIResources.White, back, Image.Type.Simple, false);
        chip.rectTransform.sizeDelta = size ?? new Vector2(140f, 34f);
        TextMeshProUGUI text = CreateText("Label", chip.transform, label, fontSize, textColor, TextAlignmentOptions.Center, true, FontStyles.Bold);
        Stretch(text.rectTransform, 8f, 8f, 2f, 2f);
        text.enableAutoSizing = true;
        text.fontSizeMax = fontSize;
        text.fontSizeMin = 10f;
        return chip;
    }

    public static VerticalLayoutGroup AddVerticalLayout(GameObject go, float spacing, RectOffset padding, TextAnchor align = TextAnchor.UpperCenter, bool controlWidth = true, bool controlHeight = true) {
        VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.padding = padding;
        layout.childAlignment = align;
        layout.childControlWidth = controlWidth;
        layout.childControlHeight = controlHeight;
        layout.childForceExpandWidth = controlWidth;
        layout.childForceExpandHeight = false;
        return layout;
    }

    public static HorizontalLayoutGroup AddHorizontalLayout(GameObject go, float spacing, RectOffset padding, TextAnchor align = TextAnchor.MiddleLeft, bool controlWidth = false, bool controlHeight = true) {
        HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.padding = padding;
        layout.childAlignment = align;
        layout.childControlWidth = controlWidth;
        layout.childControlHeight = controlHeight;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return layout;
    }

    public static LayoutElement SetLayout(GameObject go, float preferredWidth = -1f, float preferredHeight = -1f, float flexibleWidth = -1f, float minWidth = -1f, float minHeight = -1f) {
        LayoutElement element = go.GetComponent<LayoutElement>();
        if (element == null) element = go.AddComponent<LayoutElement>();
        element.preferredWidth = preferredWidth;
        element.preferredHeight = preferredHeight;
        element.flexibleWidth = flexibleWidth;
        element.minWidth = minWidth;
        element.minHeight = minHeight;
        return element;
    }

    public static ScrollRect CreateVerticalScroll(string name, Transform parent, out RectTransform content) {
        RectTransform root = CreateRect(name, parent);
        Image viewportImage = root.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.001f);
        viewportImage.raycastTarget = true;
        root.gameObject.AddComponent<RectMask2D>();

        content = CreateRect("Content", root);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;
        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = root.gameObject.AddComponent<ScrollRect>();
        scroll.content = content;
        scroll.viewport = root;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;
        return scroll;
    }
}
