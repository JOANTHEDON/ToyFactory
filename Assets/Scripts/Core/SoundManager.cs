using UnityEngine;

/// <summary>Plays short sound effects for pickups, drops, deliveries and warnings. Volume follows GameSettings.</summary>
public class SoundManager : MonoBehaviour {

    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioClip[] pickupClips;
    [SerializeField] private AudioClip[] dropClips;
    [SerializeField] private AudioClip[] deliverySuccessClips;
    [SerializeField] private AudioClip[] deliveryFailClips;
    [SerializeField] private AudioClip[] trashClips;
    [SerializeField] private AudioClip[] warningClips;
    [SerializeField] private AudioClip[] meltHitClips;

    private AudioSource audioSource;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.ignoreListenerPause = true;
    }

    private void Start() {
        ToyObject.OnAnyObjectPickedUp += ToyObject_OnAnyObjectPickedUp;
        ToyObject.OnAnyObjectPlaced += ToyObject_OnAnyObjectPlaced;
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
        MelterCounter.OnAnyMeltHit += MelterCounter_OnAnyMeltHit;

        if (DeliveryCounter.Instance != null) {
            DeliveryCounter.Instance.OnDeliveryResult += DeliveryCounter_OnDeliveryResult;
        }
        if (OrderManager.Instance != null) {
            OrderManager.Instance.OnOrderExpired += OrderManager_OnOrderExpired;
        }
    }

    private void OnDestroy() {
        ToyObject.OnAnyObjectPickedUp -= ToyObject_OnAnyObjectPickedUp;
        ToyObject.OnAnyObjectPlaced -= ToyObject_OnAnyObjectPlaced;
        TrashCounter.OnAnyObjectTrashed -= TrashCounter_OnAnyObjectTrashed;
        MelterCounter.OnAnyMeltHit -= MelterCounter_OnAnyMeltHit;
        if (DeliveryCounter.Instance != null) {
            DeliveryCounter.Instance.OnDeliveryResult -= DeliveryCounter_OnDeliveryResult;
        }
        if (OrderManager.Instance != null) {
            OrderManager.Instance.OnOrderExpired -= OrderManager_OnOrderExpired;
        }
        if (Instance == this) Instance = null;
    }

    private void ToyObject_OnAnyObjectPickedUp(object sender, System.EventArgs e) { Play(pickupClips); }
    private void ToyObject_OnAnyObjectPlaced(object sender, System.EventArgs e) { Play(dropClips); }
    private void TrashCounter_OnAnyObjectTrashed(object sender, System.EventArgs e) { Play(trashClips); }
    private void MelterCounter_OnAnyMeltHit(object sender, System.EventArgs e) { Play(meltHitClips, 0.7f); }
    private void OrderManager_OnOrderExpired(object sender, OrderManager.OrderEventArgs e) { Play(warningClips); }

    private void DeliveryCounter_OnDeliveryResult(object sender, DeliveryCounter.DeliveryEventArgs e) {
        Play(e.success ? deliverySuccessClips : deliveryFailClips);
    }

    public void Play(AudioClip[] clips, float volumeMultiplier = 1f) {
        if (clips == null || clips.Length == 0 || audioSource == null) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;
        audioSource.PlayOneShot(clip, GameSettings.SfxVolume * volumeMultiplier);
    }

    public static void PlayUIClick() {
        if (Instance != null) Instance.Play(Instance.pickupClips, 0.5f);
    }
}
