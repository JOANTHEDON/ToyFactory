using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Start Menu scene UI: logo, Play / How to Play / Settings / Quit, best score.
/// Everything is built in code from UIResources so it shares the in-game look.
/// </summary>
public class StartMenuUI : MonoBehaviour {

    [SerializeField] private Color backgroundTop = new Color(0.16f, 0.11f, 0.08f);
    [SerializeField] private Color backgroundBottom = new Color(0.33f, 0.2f, 0.12f);

    private RectTransform logoRect;
    private GameObject mainColumn;
    private GameObject settingsPanel;
    private GameObject howToPlayPanel;
    private GameObject modalBackdrop;
    private TextMeshProUGUI bestScoreText;

    private void Start() {
        Build();
    }

    private void Build() {
        // Background: two layers give a cheap vertical gradient
        Image bottom = UIFactory.CreateImage("Background", transform, null, backgroundBottom, Image.Type.Simple, true);
        UIFactory.Stretch(bottom.rectTransform);
        Image top = UIFactory.CreateImage("BackgroundTop", transform, null, backgroundTop, Image.Type.Simple, false);
        top.rectTransform.anchorMin = new Vector2(0f, 0.45f);
        top.rectTransform.anchorMax = new Vector2(1f, 1f);
        top.rectTransform.offsetMin = Vector2.zero;
        top.rectTransform.offsetMax = Vector2.zero;

        // Decorative gold band behind the logo
        Image band = UIFactory.CreateImage("Band", transform, UIResources.White, new Color(UIFactory.Gold.r, UIFactory.Gold.g, UIFactory.Gold.b, 0.12f), Image.Type.Simple);
        band.rectTransform.anchorMin = new Vector2(0f, 0.55f);
        band.rectTransform.anchorMax = new Vector2(1f, 0.95f);
        band.rectTransform.offsetMin = Vector2.zero;
        band.rectTransform.offsetMax = Vector2.zero;

        mainColumn = UIFactory.CreateRect("MainColumn", transform).gameObject;
        UIFactory.Stretch(mainColumn.GetComponent<RectTransform>());

        // Logo / title
        if (UIResources.Logo != null) {
            Image logo = UIFactory.CreateImage("Logo", mainColumn.transform, UIResources.Logo, Color.white, Image.Type.Simple);
            logo.preserveAspect = true;
            UIFactory.Anchor(logo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(460f, 360f));
            logoRect = logo.rectTransform;
        } else {
            TextMeshProUGUI title = UIFactory.CreateText("Title", mainColumn.transform, "TOY FACTORY", 110f, UIFactory.Gold, TextAlignmentOptions.Center, true, FontStyles.Bold);
            UIFactory.Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -140f), new Vector2(1200f, 160f));
            logoRect = title.rectTransform;
        }

        TextMeshProUGUI tagline = UIFactory.CreateText("Tagline", mainColumn.transform, "Melt, mold, carve and paint toys for the customers at your window!", 26f, UIFactory.Cream, TextAlignmentOptions.Center);
        UIFactory.Anchor(tagline.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -425f), new Vector2(1200f, 40f));

        // Buttons
        RectTransform buttons = UIFactory.CreateRect("Buttons", mainColumn.transform);
        UIFactory.Anchor(buttons, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(460f, 440f));
        UIFactory.AddVerticalLayout(buttons.gameObject, 18f, new RectOffset(0, 0, 0, 0), TextAnchor.LowerCenter, false, false);

        AddButton(buttons, "PLAY", new Vector2(460f, 100f), 44f, () => Loader.Load(Loader.Scene.GameScene));
        AddButton(buttons, "HOW TO PLAY", new Vector2(400f, 80f), 32f, ShowHowToPlay);
        AddButton(buttons, "SETTINGS", new Vector2(400f, 80f), 32f, ShowSettings);
        if (Application.platform != RuntimePlatform.WebGLPlayer) {
            AddButton(buttons, "QUIT", new Vector2(400f, 80f), 32f, () => {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }

        bestScoreText = UIFactory.CreateText("BestScore", mainColumn.transform, "BEST SCORE  " + GameSettings.BestScore.ToString("N0"), 24f, UIFactory.Gold, TextAlignmentOptions.Left, true, FontStyles.Bold);
        UIFactory.Anchor(bestScoreText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(30f, 24f), new Vector2(500f, 34f));

        TextMeshProUGUI version = UIFactory.CreateText("Version", mainColumn.transform, "v" + Application.version, 18f, new Color(1f, 1f, 1f, 0.5f), TextAlignmentOptions.Right);
        UIFactory.Anchor(version.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 24f), new Vector2(300f, 30f));

        // Modals
        modalBackdrop = UIFactory.CreateBackdrop("ModalBackdrop", transform, ShowMain).gameObject;
        modalBackdrop.SetActive(false);

        settingsPanel = SettingsPanelUI.Build(transform, ShowMain, true);
        settingsPanel.SetActive(false);

        howToPlayPanel = BuildHowToPlay();
        howToPlayPanel.SetActive(false);
    }

    private void AddButton(Transform parent, string label, Vector2 size, float fontSize, UnityEngine.Events.UnityAction onClick) {
        Button button = UIFactory.CreateButton(label, parent, label, size, () => {
            SoundManager.PlayUIClick();
            onClick();
        }, null, fontSize);
        UIFactory.SetLayout(button.gameObject, size.x, size.y);
    }

    private GameObject BuildHowToPlay() {
        Image panel = UIFactory.CreatePanel("HowToPlayPanel", transform, new Vector2(1040f, 680f));

        TextMeshProUGUI title = UIFactory.CreateText("Title", panel.transform, "HOW TO PLAY", 46f, UIFactory.Brown, TextAlignmentOptions.Center, true, FontStyles.Bold);
        UIFactory.Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(800f, 60f));

        RectTransform columns = UIFactory.CreateRect("Columns", panel.transform);
        columns.anchorMin = new Vector2(0f, 0f);
        columns.anchorMax = new Vector2(1f, 1f);
        columns.offsetMin = new Vector2(50f, 120f);
        columns.offsetMax = new Vector2(-50f, -110f);
        UIFactory.AddHorizontalLayout(columns.gameObject, 24f, new RectOffset(0, 0, 0, 0), TextAnchor.UpperLeft, true, true);

        AddInfoCard(columns, "THE GOAL",
            "Customers line up at the delivery window, each holding a speech bubble with the toy they want.\n\n" +
            "Build the toy, paint it, and hand it over before their patience bar runs out.\n\n" +
            "Serve them in any order: the right toy always goes to whoever asked for it first.");

        AddInfoCard(columns, "THREE PRODUCTION LINES",
            "<b>Metal:</b> Metal  >  Melter (hit MELT)  >  Mold Maker  >  Paint\n\n" +
            "<b>Plastic:</b> Plastic  >  Melter (hit MELT)  >  Mold Maker  >  Paint\n\n" +
            "<b>Wood:</b> Wood  >  Wood Cutter  >  Wood Shaper  >  Paint\n\n" +
            "Tap any order card in the game to see its full step-by-step recipe.");

        AddInfoCard(columns, "CONTROLS",
            "<b>Move:</b> WASD / Arrow keys, or the joystick on touch screens\n\n" +
            "<b>Interact (E):</b> pick up, place, take, deliver\n\n" +
            "<b>Melt (F):</b> hammer the melter\n\n" +
            "<b>Pause (ESC):</b> settings, restart, main menu\n\n" +
            "A prompt above each machine tells you what you can do there.");

        Button gotIt = UIFactory.CreateButton("GotItButton", panel.transform, "GOT IT!", new Vector2(240f, 70f), () => {
            SoundManager.PlayUIClick();
            ShowMain();
        }, null, 30f);
        UIFactory.Anchor(gotIt.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(240f, 70f));

        return panel.gameObject;
    }

    private void AddInfoCard(Transform parent, string heading, string body) {
        Image card = UIFactory.CreateCard(heading, parent, new Color(1f, 1f, 1f, 0.55f));
        UIFactory.SetLayout(card.gameObject, -1f, -1f, 1f);
        UIFactory.AddVerticalLayout(card.gameObject, 10f, new RectOffset(18, 18, 16, 16), TextAnchor.UpperLeft, true, true);

        TextMeshProUGUI head = UIFactory.CreateText("Heading", card.transform, heading, 26f, UIFactory.GoldDark, TextAlignmentOptions.Left, true, FontStyles.Bold);
        UIFactory.SetLayout(head.gameObject, -1f, 34f);

        TextMeshProUGUI text = UIFactory.CreateText("Body", card.transform, body, 20f, UIFactory.Brown, TextAlignmentOptions.TopLeft);
        UIFactory.SetLayout(text.gameObject, -1f, -1f, -1f, -1f, 300f);
    }

    private void ShowMain() {
        modalBackdrop.SetActive(false);
        settingsPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        if (bestScoreText != null) bestScoreText.text = "BEST SCORE  " + GameSettings.BestScore.ToString("N0");
    }

    private void ShowSettings() {
        modalBackdrop.SetActive(true);
        settingsPanel.SetActive(true);
        settingsPanel.transform.SetAsLastSibling();
    }

    private void ShowHowToPlay() {
        modalBackdrop.SetActive(true);
        howToPlayPanel.SetActive(true);
        howToPlayPanel.transform.SetAsLastSibling();
    }

    private void Update() {
        if (logoRect != null) {
            float bob = Mathf.Sin(Time.unscaledTime * 1.6f) * 8f;
            logoRect.anchoredPosition = new Vector2(0f, -40f + bob);
        }
    }
}
