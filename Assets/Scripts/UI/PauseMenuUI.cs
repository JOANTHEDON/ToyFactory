using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>Pause button (top-right) and the pause overlay: Resume, Settings, Restart, Main Menu. ESC toggles it.</summary>
public class PauseMenuUI : MonoBehaviour {

    private GameObject overlay;
    private GameObject menuPanel;
    private GameObject settingsPanel;
    private Button pauseButton;
    private bool isOpen;

    private void Awake() {
        Build();
    }

    private void OnDestroy() {
        if (isOpen) GamePause.Pop();
    }

    private void Build() {
        pauseButton = UIFactory.CreateButton("PauseButton", transform, "II", new Vector2(72f, 72f), Toggle, null, 34f);
        UIFactory.Anchor(pauseButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -16f), new Vector2(72f, 72f));

        overlay = UIFactory.CreateRect("Overlay", transform).gameObject;
        UIFactory.Stretch(overlay.GetComponent<RectTransform>());
        UIFactory.CreateBackdrop("Backdrop", overlay.transform, null);

        Image panel = UIFactory.CreatePanel("MenuPanel", overlay.transform, new Vector2(560f, 620f));
        menuPanel = panel.gameObject;

        TextMeshProUGUI title = UIFactory.CreateText("Title", panel.transform, "PAUSED", 54f, UIFactory.Brown, TextAlignmentOptions.Center, true, FontStyles.Bold);
        UIFactory.Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(480f, 70f));

        RectTransform buttons = UIFactory.CreateRect("Buttons", panel.transform);
        UIFactory.Anchor(buttons, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(380f, 400f));
        UIFactory.AddVerticalLayout(buttons.gameObject, 18f, new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter, false, false);

        AddMenuButton(buttons, "RESUME", Hide);
        AddMenuButton(buttons, "SETTINGS", ShowSettings);
        AddMenuButton(buttons, "RESTART", () => Loader.Load(Loader.Scene.GameScene));
        AddMenuButton(buttons, "MAIN MENU", () => Loader.Load(Loader.Scene.StartMenu));

        settingsPanel = SettingsPanelUI.Build(overlay.transform, ShowMenu, false);
        settingsPanel.SetActive(false);

        overlay.SetActive(false);
    }

    private void AddMenuButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick) {
        Button button = UIFactory.CreateButton(label, parent, label, new Vector2(380f, 80f), () => {
            SoundManager.PlayUIClick();
            onClick();
        }, null, 32f);
        UIFactory.SetLayout(button.gameObject, 380f, 80f);
    }

    private void Update() {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;

        // The recipe modal handles ESC itself while open
        if (RecipeDetailsUI.Instance != null && RecipeDetailsUI.Instance.IsVisible()) return;

        if (isOpen && settingsPanel.activeSelf) {
            ShowMenu();
        } else {
            Toggle();
        }
    }

    public void Toggle() {
        if (isOpen) Hide(); else Show();
    }

    public void Show() {
        if (isOpen) return;
        isOpen = true;
        GamePause.Push();
        overlay.SetActive(true);
        ShowMenu();
        pauseButton.gameObject.SetActive(false);
    }

    public void Hide() {
        if (!isOpen) return;
        isOpen = false;
        GamePause.Pop();
        overlay.SetActive(false);
        pauseButton.gameObject.SetActive(true);
    }

    private void ShowMenu() {
        menuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    private void ShowSettings() {
        menuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public bool IsOpen() {
        return isOpen;
    }
}
