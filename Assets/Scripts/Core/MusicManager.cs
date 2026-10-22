using UnityEngine;

/// <summary>Background music that survives scene loads. Lives in both scenes; the second copy destroys itself.</summary>
public class MusicManager : MonoBehaviour {

    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioClip musicClip;
    [SerializeField] private float baseVolume = 0.35f;

    private AudioSource audioSource;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.ignoreListenerPause = true;
        ApplyVolume();

        if (musicClip != null) audioSource.Play();
    }

    private void OnEnable() {
        GameSettings.OnSettingsChanged += ApplyVolume;
    }

    private void OnDisable() {
        GameSettings.OnSettingsChanged -= ApplyVolume;
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    private void ApplyVolume() {
        if (audioSource != null) audioSource.volume = baseVolume * GameSettings.MusicVolume;
    }
}
