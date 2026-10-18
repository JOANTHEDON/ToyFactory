using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeDetailsUI : MonoBehaviour {

    public static RecipeDetailsUI Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject modalPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button closeButton;

    [Header("Step Template & Container")]
    [SerializeField] private Transform stepsContainer;
    [SerializeField] private GameObject stepTemplate;

    [Header("Sprites for Recipe Steps")]
    [SerializeField] private Sprite plasticSprite;
    [SerializeField] private Sprite metalSprite;
    [SerializeField] private Sprite meltedPlasticSprite;
    [SerializeField] private Sprite meltedMetalSprite;
    [SerializeField] private Sprite arrowSprite;

    private List<GameObject> spawnedStepObjects = new List<GameObject>();

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There is more than one RecipeDetailsUI instance!");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (closeButton != null) {
            closeButton.onClick.AddListener(Hide);
        }

        if (stepTemplate != null) {
            stepTemplate.SetActive(false);
        }
    }

    private void Start() {
        Hide();
    }

    public void ShowRecipe(ToyObjectSO toyObjectSO) {
        if (toyObjectSO == null) return;

        ToyRecipeDatabase.ToyRecipe recipe = ToyRecipeDatabase.GetRecipeForToy(
            toyObjectSO, plasticSprite, metalSprite, meltedPlasticSprite, meltedMetalSprite
        );

        if (recipe == null) return;

        // Clear previous step items
        foreach (GameObject obj in spawnedStepObjects) {
            Destroy(obj);
        }
        spawnedStepObjects.Clear();

        if (titleText != null) {
            titleText.text = "Recipe: " + recipe.toyName;
        }

        if (stepsContainer != null && stepTemplate != null) {
            for (int i = 0; i < recipe.steps.Count; i++) {
                ToyRecipeDatabase.RecipeStep step = recipe.steps[i];

                // Spawn Step Item
                GameObject stepObj = Instantiate(stepTemplate, stepsContainer);
                stepObj.SetActive(true);
                spawnedStepObjects.Add(stepObj);

                // Set step title text
                TextMeshProUGUI stepTitle = stepObj.transform.Find("StepTitle")?.GetComponent<TextMeshProUGUI>();
                if (stepTitle == null) {
                    stepTitle = stepObj.GetComponentInChildren<TextMeshProUGUI>(true);
                }
                if (stepTitle != null) {
                    stepTitle.text = step.stepName;
                }

                // Set step description text
                TextMeshProUGUI stepDesc = stepObj.transform.Find("StepDesc")?.GetComponent<TextMeshProUGUI>();
                if (stepDesc != null) {
                    stepDesc.text = step.description;
                }

                // Set step icon
                Image stepIcon = stepObj.transform.Find("StepIcon")?.GetComponent<Image>();
                if (stepIcon == null) {
                    Image[] images = stepObj.GetComponentsInChildren<Image>(true);
                    foreach (Image img in images) {
                        if (img.gameObject != stepObj) {
                            stepIcon = img;
                            break;
                        }
                    }
                }
                if (stepIcon != null) {
                    if (step.icon != null) {
                        stepIcon.sprite = step.icon;
                        stepIcon.gameObject.SetActive(true);
                    } else {
                        stepIcon.gameObject.SetActive(false);
                    }
                }

                // If not last step and arrow sprite is assigned, spawn an arrow separator
                if (i < recipe.steps.Count - 1 && arrowSprite != null) {
                    GameObject arrowObj = new GameObject("ArrowSeparator", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    arrowObj.transform.SetParent(stepsContainer, false);

                    RectTransform arrowRect = arrowObj.GetComponent<RectTransform>();
                    arrowRect.sizeDelta = new Vector2(32f, 32f);

                    Image arrowImage = arrowObj.GetComponent<Image>();
                    arrowImage.sprite = arrowSprite;
                    arrowImage.preserveAspect = true;

                    spawnedStepObjects.Add(arrowObj);
                }
            }
        }

        Show();
    }

    public void Show() {
        if (modalPanel != null) {
            modalPanel.SetActive(true);
        } else {
            gameObject.SetActive(true);
        }
    }

    public void Hide() {
        if (modalPanel != null) {
            modalPanel.SetActive(false);
        } else {
            gameObject.SetActive(false);
        }
    }
}
