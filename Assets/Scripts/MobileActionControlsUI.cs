using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Touch-screen action buttons. Visibility and labels come from each counter's interaction hints,
/// so the buttons only appear when pressing them would actually do something.
/// The whole control set (buttons + joystick) is hidden on desktop unless enabled in Settings.
/// </summary>
public class MobileActionControlsUI : MonoBehaviour {

    public enum ActionType {
        Pickup,
        Drop,
        Deliver
    }

    [Header("Primary Action Button (Interact / E)")]
    [SerializeField] private Button actionButton;
    [SerializeField] private Image actionButtonImage;
    [SerializeField] private TextMeshProUGUI actionButtonText;

    [Header("Action Sprites")]
    [SerializeField] private Sprite pickupSprite;
    [SerializeField] private Sprite dropSprite;
    [SerializeField] private Sprite deliverySprite;

    [Header("Alternate Action Button (Melt / F)")]
    [SerializeField] private Button alternateActionButton;
    [SerializeField] private TextMeshProUGUI alternateActionButtonText;

    [Header("Joystick")]
    [SerializeField] private GameObject virtualJoystick;

    private BaseCounter currentCounter;
    private bool controlsEnabled = true;

    private void Awake() {
        if (actionButton != null) {
            actionButton.onClick.AddListener(OnActionButtonClicked);
        }

        if (alternateActionButton != null) {
            alternateActionButton.onClick.AddListener(OnAlternateActionButtonClicked);
        }

        // Hide text if icon is used, or keep it clear
        if (actionButtonText != null) {
            actionButtonText.gameObject.SetActive(false);
        }
    }

    private void Start() {
        if (Player.Instance != null) {
            Player.Instance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
        }

        GameSettings.OnSettingsChanged += GameSettings_OnSettingsChanged;
        ApplyControlsVisibility();
        UpdateButtons(null);
    }

    private void OnDestroy() {
        if (Player.Instance != null) {
            Player.Instance.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
        }
        GameSettings.OnSettingsChanged -= GameSettings_OnSettingsChanged;
    }

    private void GameSettings_OnSettingsChanged() {
        ApplyControlsVisibility();
    }

    private void ApplyControlsVisibility() {
        controlsEnabled = GameSettings.ShowMobileControls;
        if (virtualJoystick != null) {
            virtualJoystick.SetActive(controlsEnabled);
        }
        if (!controlsEnabled) {
            SetActionButtonVisible(false);
            SetAlternateButtonVisible(false);
            if (GameInput.Instance != null) GameInput.Instance.SetVirtualMovementVector(Vector2.zero);
        }
    }

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e) {
        UpdateButtons(e.selectedCounter);
    }

    private void OnActionButtonClicked() {
        if (GameInput.Instance != null) {
            GameInput.Instance.TriggerInteractAction();
        }
    }

    private void OnAlternateActionButtonClicked() {
        if (GameInput.Instance != null) {
            GameInput.Instance.TriggerInteractAlternateAction();
        }
    }

    private void UpdateButtons(BaseCounter counter) {
        currentCounter = counter;
    }

    private void LateUpdate() {
        if (!controlsEnabled) return;

        Player player = Player.Instance;
        BaseCounter counter = currentCounter;
        if (player == null || counter == null || GamePause.IsPaused) {
            SetActionButtonVisible(false);
            SetAlternateButtonVisible(false);
            return;
        }

        string hint = counter.GetInteractHint(player);
        string alternateHint = counter.GetInteractAlternateHint(player);

        if (hint != null) {
            ActionType actionType;
            if (counter is DeliveryCounter) {
                actionType = ActionType.Deliver;
            } else if (player.HasToyObject()) {
                actionType = ActionType.Drop;
            } else {
                actionType = ActionType.Pickup;
            }
            SetActionButtonVisible(true, actionType);
        } else {
            SetActionButtonVisible(false);
        }

        SetAlternateButtonVisible(alternateHint != null, alternateHint);
    }

    private void SetActionButtonVisible(bool visible, ActionType actionType = ActionType.Pickup) {
        if (actionButton != null) {
            actionButton.gameObject.SetActive(visible);
            if (visible && actionButtonImage != null) {
                Sprite targetSprite = null;
                switch (actionType) {
                    case ActionType.Pickup:
                        targetSprite = pickupSprite;
                        break;
                    case ActionType.Drop:
                        targetSprite = dropSprite;
                        break;
                    case ActionType.Deliver:
                        targetSprite = deliverySprite;
                        break;
                }

                if (targetSprite != null) {
                    actionButtonImage.sprite = targetSprite;
                    actionButtonImage.color = Color.white;
                }
            }
        }
    }

    private void SetAlternateButtonVisible(bool visible, string label = "") {
        if (alternateActionButton != null) {
            alternateActionButton.gameObject.SetActive(visible);
            if (visible && alternateActionButtonText != null && !string.IsNullOrEmpty(label)) {
                alternateActionButtonText.text = label;
            }
        }
    }
}
