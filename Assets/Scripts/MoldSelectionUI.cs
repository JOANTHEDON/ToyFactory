using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoldSelectionUI : MonoBehaviour {

    [SerializeField] private MoldMakerCounter moldMakerCounter;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private Transform optionTemplateButton;

    private List<GameObject> spawnedButtonsList = new List<GameObject>();

    private void Awake() {
        if (optionTemplateButton != null) {
            optionTemplateButton.gameObject.SetActive(false);
        }
    }

    private void Start() {
        if (moldMakerCounter == null) {
            moldMakerCounter = GetComponentInParent<MoldMakerCounter>();
        }

        if (moldMakerCounter != null) {
            moldMakerCounter.OnMoldingStateChanged += MoldMakerCounter_OnMoldingStateChanged;
        }

        Hide();
    }

    private void MoldMakerCounter_OnMoldingStateChanged(object sender, MoldMakerCounter.OnMoldingStateChangedEventArgs e) {
        if (e.state == MoldMakerCounter.State.ChoosingRecipe) {
            UpdateMenu();
            Show();
        } else {
            Hide();
        }
    }

    private void UpdateMenu() {
        // Clear previous buttons
        foreach (GameObject btn in spawnedButtonsList) {
            Destroy(btn);
        }
        spawnedButtonsList.Clear();

        MoldingRecipeSO activeRecipeSO = moldMakerCounter.GetActiveRecipeSO();
        if (activeRecipeSO == null || activeRecipeSO.availableOutputs == null) {
            return;
        }

        // Dynamically instantiate button for each available output toy
        foreach (ToyObjectSO outputSO in activeRecipeSO.availableOutputs) {
            if (outputSO == null) continue;

            Transform buttonTransform = Instantiate(optionTemplateButton, optionsContainer);
            buttonTransform.gameObject.SetActive(true);
            spawnedButtonsList.Add(buttonTransform.gameObject);

            // Set button label if Text or TextMeshProUGUI is found
            string displayName = !string.IsNullOrEmpty(outputSO.objectName) ? outputSO.objectName : outputSO.name;

            TextMeshProUGUI tmpText = buttonTransform.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmpText != null) {
                tmpText.text = displayName;
            } else {
                Text legacyText = buttonTransform.GetComponentInChildren<Text>(true);
                if (legacyText != null) {
                    legacyText.text = displayName;
                }
            }

            // Set icon image if available
            Transform iconTransform = buttonTransform.Find("Icon");
            if (iconTransform != null && iconTransform.TryGetComponent(out Image iconImage)) {
                if (outputSO.sprite != null) {
                    iconImage.sprite = outputSO.sprite;
                    iconImage.gameObject.SetActive(true);
                } else {
                    iconImage.gameObject.SetActive(false);
                }
            }

            // Hook up onClick
            Button button = buttonTransform.GetComponent<Button>();
            if (button != null) {
                ToyObjectSO capturedOutput = outputSO;
                button.onClick.AddListener(() => {
                    moldMakerCounter.SelectOutput(capturedOutput);
                    Hide();
                });
            }
        }
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        if (moldMakerCounter != null) {
            moldMakerCounter.OnMoldingStateChanged -= MoldMakerCounter_OnMoldingStateChanged;
        }
    }
}
