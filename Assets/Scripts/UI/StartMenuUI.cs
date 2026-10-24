using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Start Menu scene UI controller.
/// All Canvas UI elements (Background, Logo, Buttons, Texts, Panels) exist directly
/// in the Hierarchy so they can be inspected, moved, styled, and customized in the Unity Editor.
/// </summary>
public class StartMenuUI : MonoBehaviour {

    [Header("Main Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Animated Elements")]
    [SerializeField] private RectTransform logoRect;

    [Header("Dynamic Texts")]
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI versionText;

    [Header("Modal Panels")]
    [SerializeField] private GameObject modalBackdrop;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private Button howToPlayGotItButton;

    private void Awake() {
        // Wire up main menu button click events
        if (playButton != null) {
            playButton.onClick.AddListener(() => {
                SoundManager.PlayUIClick();
                Loader.Load(Loader.Scene.GameScene);
            });
        }

        if (howToPlayButton != null) {
            howToPlayButton.onClick.AddListener(() => {
                SoundManager.PlayUIClick();
                ShowHowToPlay();
            });
        }

        if (settingsButton != null) {
            settingsButton.onClick.AddListener(() => {
                SoundManager.PlayUIClick();
                ShowSettings();
            });
        }

        if (quitButton != null) {
            if (Application.platform == RuntimePlatform.WebGLPlayer) {
                quitButton.gameObject.SetActive(false);
            } else {
                quitButton.onClick.AddListener(() => {
                    SoundManager.PlayUIClick();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });
            }
        }

        // Wire up modal backdrops and sub-buttons
        if (modalBackdrop != null) {
            Button backdropBtn = modalBackdrop.GetComponent<Button>();
            if (backdropBtn != null) {
                backdropBtn.onClick.AddListener(ShowMain);
            }
        }

        if (howToPlayGotItButton != null) {
            howToPlayGotItButton.onClick.AddListener(() => {
                SoundManager.PlayUIClick();
                ShowMain();
            });
        }
    }

    private void Start() {
        // Ensure Settings modal panel is built (SettingsPanelUI creates its content or uses existing)
        if (settingsPanel == null) {
            settingsPanel = SettingsPanelUI.Build(transform, ShowMain, true);
        }

        RefreshScoresAndVersion();
        ShowMain();
    }

    private void OnEnable() {
        GameSettings.OnSettingsChanged += RefreshScoresAndVersion;
    }

    private void OnDisable() {
        GameSettings.OnSettingsChanged -= RefreshScoresAndVersion;
    }

    private void RefreshScoresAndVersion() {
        if (bestScoreText != null) {
            bestScoreText.text = "BEST SCORE  " + GameSettings.BestScore.ToString("N0");
        }
        if (versionText != null) {
            versionText.text = "v" + Application.version;
        }
    }

    public void ShowMain() {
        if (modalBackdrop != null) modalBackdrop.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        RefreshScoresAndVersion();
    }

    public void ShowSettings() {
        if (modalBackdrop != null) {
            modalBackdrop.SetActive(true);
            modalBackdrop.transform.SetAsLastSibling();
        }
        if (settingsPanel != null) {
            settingsPanel.SetActive(true);
            settingsPanel.transform.SetAsLastSibling();
        }
    }

    public void ShowHowToPlay() {
        if (modalBackdrop != null) {
            modalBackdrop.SetActive(true);
            modalBackdrop.transform.SetAsLastSibling();
        }
        if (howToPlayPanel != null) {
            howToPlayPanel.SetActive(true);
            howToPlayPanel.transform.SetAsLastSibling();
        }
    }

    private void Update() {
        if (logoRect != null) {
            float bob = Mathf.Sin(Time.unscaledTime * 1.6f) * 8f;
            logoRect.anchoredPosition = new Vector2(0f, -40f + bob);
        }
    }
}
