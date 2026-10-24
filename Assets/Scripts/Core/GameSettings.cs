using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player-facing settings persisted in PlayerPrefs. Shared by the Start Menu and the in-game Pause Menu.
/// </summary>
public static class GameSettings {

    public enum MobileControlsMode {
        Auto,
        On,
        Off,
    }

    private const string MUSIC_VOLUME_KEY = "Settings.MusicVolume";
    private const string SFX_VOLUME_KEY = "Settings.SfxVolume";
    private const string MOBILE_CONTROLS_KEY = "Settings.MobileControls";
    private const string QUALITY_KEY = "Settings.Quality";
    private const string BEST_SCORE_KEY = "Progress.BestScore";

    public static event Action OnSettingsChanged;

    public static float MusicVolume {
        get => PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);
        set { PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, Mathf.Clamp01(value)); Changed(); }
    }

    public static float SfxVolume {
        get => PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.8f);
        set { PlayerPrefs.SetFloat(SFX_VOLUME_KEY, Mathf.Clamp01(value)); Changed(); }
    }

    public static MobileControlsMode MobileControls {
        get => (MobileControlsMode)PlayerPrefs.GetInt(MOBILE_CONTROLS_KEY, (int)MobileControlsMode.Auto);
        set { PlayerPrefs.SetInt(MOBILE_CONTROLS_KEY, (int)value); Changed(); }
    }

    /// <summary>Resolved value: should the on-screen joystick and action buttons be shown right now?</summary>
    public static bool ShowMobileControls {
        get {
            switch (MobileControls) {
                case MobileControlsMode.On: return true;
                case MobileControlsMode.Off: return false;
                default:
                    return Application.isMobilePlatform || Touchscreen.current != null;
            }
        }
    }

    public static int QualityLevel {
        get => PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.GetQualityLevel());
        set {
            int clamped = Mathf.Clamp(value, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
            PlayerPrefs.SetInt(QUALITY_KEY, clamped);
            QualitySettings.SetQualityLevel(clamped, true);
            Changed();
        }
    }

    public static int BestScore {
        get => PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        set { PlayerPrefs.SetInt(BEST_SCORE_KEY, value); PlayerPrefs.Save(); }
    }

    public static void ApplyOnStartup() {
        // Enforce landscape orientation across all platforms and devices
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        if (PlayerPrefs.HasKey(QUALITY_KEY)) {
            int level = Mathf.Clamp(PlayerPrefs.GetInt(QUALITY_KEY), 0, Mathf.Max(0, QualitySettings.names.Length - 1));
            if (level != QualitySettings.GetQualityLevel()) {
                QualitySettings.SetQualityLevel(level, true);
            }
        }
    }

    public static void ResetProgress() {
        PlayerPrefs.DeleteKey(BEST_SCORE_KEY);
        PlayerPrefs.Save();
        Changed();
    }

    private static void Changed() {
        PlayerPrefs.Save();
        OnSettingsChanged?.Invoke();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap() {
        ApplyOnStartup();
    }
}
