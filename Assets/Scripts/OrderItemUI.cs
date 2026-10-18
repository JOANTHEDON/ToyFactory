using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderItemUI : MonoBehaviour {

    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI toyNameText;
    [SerializeField] private Button recipeButton;

    private ToyObjectSO toyObjectSO;

    private void Awake() {
        if (iconImage == null) {
            Transform iconT = transform.Find("ToyIcon") ?? transform.Find("icon") ?? transform.Find("Icon");
            if (iconT != null) {
                iconImage = iconT.GetComponent<Image>();
            } else {
                Image buttonImg = GetComponent<Image>();
                Image[] allImages = GetComponentsInChildren<Image>(true);
                foreach (Image img in allImages) {
                    if (img != buttonImg) {
                        iconImage = img;
                        break;
                    }
                }
            }
        }

        if (toyNameText == null) {
            toyNameText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (recipeButton == null) {
            recipeButton = GetComponent<Button>();
        }
    }

    public void SetToyObjectSO(ToyObjectSO toyObjectSO) {
        if (iconImage == null || toyNameText == null || recipeButton == null) {
            Awake();
        }

        this.toyObjectSO = toyObjectSO;

        if (toyObjectSO == null) {
            if (toyNameText != null) toyNameText.text = "";
            if (iconImage != null) iconImage.gameObject.SetActive(false);
            return;
        }

        string displayName = !string.IsNullOrEmpty(toyObjectSO.objectName) ? toyObjectSO.objectName : toyObjectSO.name;

        if (toyNameText != null) {
            toyNameText.text = displayName;
        }

        if (iconImage != null) {
            if (toyObjectSO.sprite != null) {
                iconImage.sprite = toyObjectSO.sprite;
                iconImage.gameObject.SetActive(true);
            } else {
                iconImage.gameObject.SetActive(false);
            }
        }

        // Setup button click
        Button targetButton = recipeButton != null ? recipeButton : GetComponent<Button>();
        if (targetButton != null) {
            targetButton.onClick.RemoveAllListeners();
            targetButton.onClick.AddListener(() => {
                if (RecipeDetailsUI.Instance != null) {
                    RecipeDetailsUI.Instance.ShowRecipe(toyObjectSO);
                }
            });
        }
    }
}
