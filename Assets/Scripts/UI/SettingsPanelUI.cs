using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Builds the shared Settings panel (music, sound, touch controls, quality) used by the Start Menu and the Pause Menu.</summary>
public static class SettingsPanelUI {

    public static readonly Vector2 PanelSize = new Vector2(720f, 620f);

    public static GameObject Build(Transform parent, Action onBack, bool showResetProgress) {
        Image panel = UIFactory.CreatePanel("SettingsPanel", parent, PanelSize);

        TextMeshProUGUI title = UIFactory.CreateText("Title", panel.transform, "SETTINGS", 46f, UIFactory.Brown, TextAlignmentOptions.Center, true, FontStyles.Bold);
        UIFactory.Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(600f, 60f));

        RectTransform rows = UIFactory.CreateRect("Rows", panel.transform);
        rows.anchorMin = new Vector2(0f, 0f);
        rows.anchorMax = new Vector2(1f, 1f);
        rows.offsetMin = new Vector2(50f, 120f);
        rows.offsetMax = new Vector2(-50f, -120f);
        UIFactory.AddVerticalLayout(rows.gameObject, 18f, new RectOffset(0, 0, 0, 0), TextAnchor.UpperCenter, true, true);

        BuildSliderRow(rows, "Music volume", GameSettings.MusicVolume, value => GameSettings.MusicVolume = value);
        BuildSliderRow(rows, "Sound effects", GameSettings.SfxVolume, value => {
            GameSettings.SfxVolume = value;
        });

        BuildCycleRow(rows, "Touch controls",
            () => GameSettings.MobileControls.ToString().ToUpperInvariant() + (GameSettings.MobileControls == GameSettings.MobileControlsMode.Auto ? (GameSettings.ShowMobileControls ? " (on)" : " (off)") : ""),
            () => GameSettings.MobileControls = (GameSettings.MobileControlsMode)(((int)GameSettings.MobileControls + 1) % 3));

        string[] qualityNames = QualitySettings.names;
        if (qualityNames != null && qualityNames.Length > 1) {
            BuildCycleRow(rows, "Graphics quality",
                () => qualityNames[Mathf.Clamp(GameSettings.QualityLevel, 0, qualityNames.Length - 1)].ToUpperInvariant(),
                () => GameSettings.QualityLevel = (GameSettings.QualityLevel + 1) % qualityNames.Length);
        }

        if (showResetProgress) {
            RectTransform resetRow = CreateRow(rows, "Best score");
            TextMeshProUGUI bestValue = null;
            Button reset = UIFactory.CreateButton("ResetButton", resetRow, "RESET  " + GameSettings.BestScore.ToString("N0"), new Vector2(260f, 56f), null, UIFactory.CreamDark, 22f);
            reset.onClick.AddListener(() => {
                GameSettings.ResetProgress();
                bestValue = reset.GetComponentInChildren<TextMeshProUGUI>();
                if (bestValue != null) bestValue.text = "RESET  0";
                SoundManager.PlayUIClick();
            });
            UIFactory.SetLayout(reset.gameObject, 260f, 56f);
        }

        Button back = UIFactory.CreateButton("BackButton", panel.transform, "BACK", new Vector2(240f, 70f), () => {
            SoundManager.PlayUIClick();
            onBack?.Invoke();
        }, null, 30f);
        UIFactory.Anchor(back.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(240f, 70f));

        return panel.gameObject;
    }

    private static RectTransform CreateRow(RectTransform rows, string label) {
        Image row = UIFactory.CreateCard("Row", rows, new Color(1f, 1f, 1f, 0.5f));
        UIFactory.SetLayout(row.gameObject, -1f, 76f);
        UIFactory.AddHorizontalLayout(row.gameObject, 16f, new RectOffset(22, 22, 8, 8), TextAnchor.MiddleLeft, false, true);

        TextMeshProUGUI text = UIFactory.CreateText("Label", row.transform, label, 26f, UIFactory.Brown, TextAlignmentOptions.Left, true, FontStyles.Bold);
        UIFactory.SetLayout(text.gameObject, 260f, 50f, 1f);
        return row.rectTransform;
    }

    private static void BuildSliderRow(RectTransform rows, string label, float value, Action<float> onChanged) {
        RectTransform row = CreateRow(rows, label);
        TextMeshProUGUI percent = null;
        Slider slider = UIFactory.CreateSlider("Slider", row, new Vector2(260f, 36f), value, v => {
            onChanged(v);
            if (percent != null) percent.text = Mathf.RoundToInt(v * 100f) + "%";
        });
        UIFactory.SetLayout(slider.gameObject, 260f, 36f);
        percent = UIFactory.CreateText("Percent", row, Mathf.RoundToInt(value * 100f) + "%", 24f, UIFactory.BrownSoft, TextAlignmentOptions.Right);
        UIFactory.SetLayout(percent.gameObject, 70f, 40f);
    }

    private static void BuildCycleRow(RectTransform rows, string label, Func<string> getValue, Action cycle) {
        RectTransform row = CreateRow(rows, label);
        Button button = UIFactory.CreateButton("CycleButton", row, getValue(), new Vector2(340f, 56f), null, UIFactory.CreamDark, 22f);
        button.onClick.AddListener(() => {
            cycle();
            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = getValue();
            SoundManager.PlayUIClick();
        });
        UIFactory.SetLayout(button.gameObject, 340f, 56f);
    }
}
