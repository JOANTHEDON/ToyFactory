using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Guided navigation UI for the first 4 orders.
/// Displays an interactive banner guiding the player step-by-step:
/// what to collect, which machines to use, and where to deliver.
/// Also features an animated world-space guidance beacon pointing directly at the target counter.
/// Automatically steps aside once the player has delivered their first 4 orders.
/// </summary>
public class OrderNavigationUI : MonoBehaviour {

    public static OrderNavigationUI Instance { get; private set; }

    private RectTransform canvasRect;
    private Camera mainCamera;

    // HUD banner elements
    private GameObject bannerRoot;
    private Image bannerCard;
    private Image toyIcon;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI actionText;
    private TextMeshProUGUI detailText;
    private TextMeshProUGUI stepBadgeText;
    private TextMeshProUGUI orderCounterBadgeText;
    private Button openRecipeButton;

    // World-space target beacon (arrow & pulsing halo hovering over target machine/counter)
    private GameObject worldBeaconRoot;
    private Image beaconArrow;
    private Image beaconHalo;
    private TextMeshProUGUI beaconLabel;

    private BaseCounter currentTargetCounter;
    private string currentTargetStationName;
    private OrderManager.Order trackedOrder;
    private ToyRecipeDatabase.ToyRecipe trackedRecipe;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) {
            canvas = FindFirstObjectByType<Canvas>();
        }
        canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;
    }

    private void Start() {
        BuildHUD();
        BuildWorldBeacon();

        if (OrderManager.Instance != null) {
            OrderManager.Instance.OnOrderListChanged += OrderManager_OnOrderListChanged;
            OrderManager.Instance.OnOrderCompleted += OrderManager_OnOrderCompleted;
        }

        RefreshTracking();
    }

    private void OnDestroy() {
        if (OrderManager.Instance != null) {
            OrderManager.Instance.OnOrderListChanged -= OrderManager_OnOrderListChanged;
            OrderManager.Instance.OnOrderCompleted -= OrderManager_OnOrderCompleted;
        }
        if (Instance == this) Instance = null;
    }

    private void BuildHUD() {
        Transform parentTransform = canvasRect != null ? canvasRect.transform : transform;

        bannerRoot = new GameObject("OrderNavigationBanner", typeof(RectTransform));
        bannerRoot.transform.SetParent(parentTransform, false);

        RectTransform bannerRect = bannerRoot.GetComponent<RectTransform>();
        // Top-center HUD banner position
        UIFactory.Anchor(bannerRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(60f, -16f), new Vector2(560f, 96f));

        // Background shadow
        Image shadow = UIFactory.CreateImage("Shadow", bannerRoot.transform, UIResources.White, new Color(0f, 0f, 0f, 0.35f), Image.Type.Simple);
        UIFactory.Anchor(shadow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -4f), new Vector2(560f, 96f));

        // Outer border
        Image border = UIFactory.CreateImage("Border", bannerRoot.transform, UIResources.White, UIFactory.Brown, Image.Type.Simple);
        UIFactory.Stretch(border.rectTransform);

        // Inner card body
        bannerCard = UIFactory.CreateImage("CardBody", border.transform, UIResources.White, UIFactory.Cream, Image.Type.Simple);
        UIFactory.Stretch(bannerCard.rectTransform, 3f, 3f, 3f, 3f);

        // Left Toy Icon Frame
        Image iconFrame = UIFactory.CreateImage("IconFrame", bannerCard.transform, UIResources.White, new Color(1f, 1f, 1f, 0.95f), Image.Type.Simple);
        UIFactory.Anchor(iconFrame.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(74f, 74f));

        toyIcon = UIFactory.CreateImage("ToyIcon", iconFrame.transform, null, Color.white, Image.Type.Simple);
        toyIcon.preserveAspect = true;
        UIFactory.Stretch(toyIcon.rectTransform, 4f, 4f, 4f, 4f);

        // Step number badge overlay on toy icon
        Image stepBadge = UIFactory.CreateChip("StepBadge", iconFrame.transform, "1/4", UIFactory.Gold, Color.white, 16f, new Vector2(36f, 22f));
        UIFactory.Anchor(stepBadge.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(-4f, 4f), new Vector2(36f, 22f));
        stepBadgeText = stepBadge.GetComponentInChildren<TextMeshProUGUI>();

        // Text area
        titleText = UIFactory.CreateText("TitleText", bannerCard.transform, "TUTORIAL ORDER 1 OF 4", 17f, UIFactory.GoldDark, TextAlignmentOptions.Left, true, FontStyles.Bold);
        UIFactory.Anchor(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(96f, -10f), new Vector2(340f, 22f));

        actionText = UIFactory.CreateText("ActionText", bannerCard.transform, "Take Wood from Wood Container", 22f, UIFactory.Brown, TextAlignmentOptions.Left, true, FontStyles.Bold);
        UIFactory.Anchor(actionText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(96f, -32f), new Vector2(360f, 28f));
        actionText.enableAutoSizing = true;
        actionText.fontSizeMin = 15f;
        actionText.fontSizeMax = 22f;

        detailText = UIFactory.CreateText("DetailText", bannerCard.transform, "Follow the yellow arrow over the machine", 17f, UIFactory.BrownSoft, TextAlignmentOptions.Left, false, FontStyles.Normal);
        UIFactory.Anchor(detailText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(96f, -60f), new Vector2(360f, 24f));
        detailText.enableAutoSizing = true;
        detailText.fontSizeMin = 13f;
        detailText.fontSizeMax = 17f;

        // Tutorial progress counter badge, top-right
        Image orderProgressChip = UIFactory.CreateChip("OrderProgress", bannerCard.transform, "GUIDE ON", UIFactory.Green, Color.white, 15f, new Vector2(86f, 26f));
        UIFactory.Anchor(orderProgressChip.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-10f, -10f), new Vector2(86f, 26f));
        orderCounterBadgeText = orderProgressChip.GetComponentInChildren<TextMeshProUGUI>();

        // "Recipe" help button on right side
        openRecipeButton = UIFactory.CreateButton("RecipeButton", bannerCard.transform, "RECIPE", new Vector2(86f, 36f), () => {
            if (trackedOrder != null && trackedOrder.toyObjectSO != null && RecipeDetailsUI.Instance != null) {
                RecipeDetailsUI.Instance.ShowRecipe(trackedOrder.toyObjectSO);
            }
        }, UIFactory.Gold, 18f, Color.white);
        UIFactory.Anchor(openRecipeButton.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-10f, 10f), new Vector2(86f, 36f));

        bannerRoot.SetActive(false);
    }

    private void BuildWorldBeacon() {
        Transform parentTransform = canvasRect != null ? canvasRect.transform : transform;

        worldBeaconRoot = new GameObject("TutorialWorldBeacon", typeof(RectTransform));
        worldBeaconRoot.transform.SetParent(parentTransform, false);

        RectTransform rootRect = worldBeaconRoot.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(100f, 100f);

        // Pulsing glow halo
        beaconHalo = UIFactory.CreateImage("BeaconHalo", worldBeaconRoot.transform, UIResources.White, new Color(UIFactory.Gold.r, UIFactory.Gold.g, UIFactory.Gold.b, 0.4f), Image.Type.Simple);
        UIFactory.Anchor(beaconHalo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(56f, 56f));

        // Pointing down indicator arrow
        Sprite arrowSprite = UIResources.Arrow != null ? UIResources.Arrow : UIResources.White;
        beaconArrow = UIFactory.CreateImage("BeaconArrow", worldBeaconRoot.transform, arrowSprite, UIFactory.Gold, Image.Type.Simple);
        UIFactory.Anchor(beaconArrow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(48f, 48f));
        if (UIResources.Arrow != null) {
            // Point downwards
            beaconArrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -90f);
        }

        // Floating helper label above the arrow
        Image labelChip = UIFactory.CreateChip("BeaconChip", worldBeaconRoot.transform, "HERE", UIFactory.Brown, UIFactory.Cream, 17f, new Vector2(130f, 28f));
        UIFactory.Anchor(labelChip.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(130f, 28f));
        beaconLabel = labelChip.GetComponentInChildren<TextMeshProUGUI>();

        worldBeaconRoot.SetActive(false);
    }

    private void OrderManager_OnOrderListChanged(object sender, EventArgs e) {
        RefreshTracking();
    }

    private void OrderManager_OnOrderCompleted(object sender, OrderManager.OrderEventArgs e) {
        RefreshTracking();
    }

    private void RefreshTracking() {
        if (OrderManager.Instance == null) return;

        int delivered = OrderManager.Instance.GetDeliveredCount();
        if (delivered >= 4) {
            // Tutorial finished! Hide banner & beacon completely
            if (bannerRoot != null) bannerRoot.SetActive(false);
            if (worldBeaconRoot != null) worldBeaconRoot.SetActive(false);
            return;
        }

        List<OrderManager.Order> orders = OrderManager.Instance.GetActiveOrderList();
        if (orders == null || orders.Count == 0) {
            trackedOrder = null;
            trackedRecipe = null;
            if (bannerRoot != null) bannerRoot.SetActive(false);
            if (worldBeaconRoot != null) worldBeaconRoot.SetActive(false);
            return;
        }

        // Track the earliest order in the queue
        OrderManager.Order firstOrder = orders[0];
        if (trackedOrder != firstOrder) {
            trackedOrder = firstOrder;
            trackedRecipe = ToyRecipeDatabase.GetRecipeForToy(trackedOrder.toyObjectSO);
        }

        if (bannerRoot != null) bannerRoot.SetActive(true);
    }

    private void Update() {
        if (OrderManager.Instance == null) return;
        int delivered = OrderManager.Instance.GetDeliveredCount();
        if (delivered >= 4) {
            if (bannerRoot != null && bannerRoot.activeSelf) bannerRoot.SetActive(false);
            if (worldBeaconRoot != null && worldBeaconRoot.activeSelf) worldBeaconRoot.SetActive(false);
            return;
        }

        if (trackedOrder == null || trackedOrder.toyObjectSO == null) {
            RefreshTracking();
            return;
        }

        UpdateNavigationStep();
        UpdateWorldBeaconPosition();
    }

    private void UpdateNavigationStep() {
        int delivered = OrderManager.Instance.GetDeliveredCount();
        if (orderCounterBadgeText != null) {
            orderCounterBadgeText.text = "ORDER " + (delivered + 1) + "/4";
        }
        if (titleText != null) {
            titleText.text = "TUTORIAL ORDER " + (delivered + 1) + " OF 4  (" + trackedOrder.GetToyName().ToUpper() + ")";
        }
        if (toyIcon != null && trackedOrder.toyObjectSO != null) {
            toyIcon.sprite = trackedOrder.toyObjectSO.sprite;
            toyIcon.gameObject.SetActive(toyIcon.sprite != null);
        }

        Player player = Player.Instance;
        ToyObject carriedToy = player != null ? player.GetToyObject() : null;
        ToyObjectSO carriedSO = carriedToy != null ? carriedToy.GetToyObjectSO() : null;

        // Determine which stage the player is currently on:
        // Stage 1: Player has nothing -> Needs to collect raw material
        // Stage 2: Player has raw material -> Needs to process at Cutter / Melter
        // Stage 3: Item processed (or in Melter/Cutter) -> Needs to pick up or mold/shape
        // Stage 4: Molded toy -> Needs to paint at Paint Station
        // Stage 5: Painted toy -> Deliver at Delivery Counter

        string mainAction = "";
        string detail = "";
        BaseCounter targetCounter = null;
        string stepNumberStr = "1";

        if (carriedToy != null) {
            // Player is carrying an item
            if (carriedSO == trackedOrder.toyObjectSO) {
                // Carrying target toy!
                if (carriedToy.IsColored()) {
                    // Finished! Take to delivery
                    stepNumberStr = "4/4";
                    mainAction = "Deliver the " + trackedOrder.GetToyName() + "!";
                    detail = "Walk to the Delivery Counter window to serve customer.";
                    targetCounter = DeliveryCounter.Instance;
                } else {
                    // Needs painting
                    stepNumberStr = "3/4";
                    mainAction = "Paint the " + trackedOrder.GetToyName();
                    detail = "Place toy on the Paint Station counter.";
                    targetCounter = FindFirstObjectByType<ColorCounter>();
                }
            } else {
                // Carrying intermediate or raw material
                string matName = ToyObjectSO.GetDisplayName(carriedSO);
                bool isRaw = matName.Contains("Wood") && !matName.Contains("Cutted")
                          || matName.Contains("Plastic") && !matName.Contains("Melted")
                          || matName.Contains("Metal") && !matName.Contains("Melted");

                if (isRaw) {
                    stepNumberStr = "2/4";
                    if (matName.Contains("Wood")) {
                        mainAction = "Place " + matName + " on Wood Cutter";
                        detail = "Wait for the saw to cut it into shaped wood.";
                        targetCounter = FindFirstObjectByType<WoodCounter>();
                    } else {
                        mainAction = "Place " + matName + " in Melter";
                        detail = "Press [F] or Melt button repeatedly to melt it.";
                        targetCounter = FindFirstObjectByType<MelterCounter>();
                    }
                } else {
                    // Cut wood or molten container -> Take to Mold Maker or Wood Shaper
                    stepNumberStr = "3/4";
                    if (matName.Contains("Cut")) {
                        mainAction = "Load " + matName + " into Wood Shaper";
                        detail = "Choose " + trackedOrder.GetToyName() + " to carve it.";
                        targetCounter = FindFirstObjectByType<WoodShaperCounter>();
                    } else {
                        mainAction = "Pour molten material into Mold Maker";
                        detail = "Choose " + trackedOrder.GetToyName() + " to mold it.";
                        targetCounter = FindFirstObjectByType<MoldMakerCounter>();
                    }
                }
            }
        } else {
            // Player is not carrying anything.
            // Check if intermediate / finished toy is waiting on any machine:
            ColorCounter colorCounter = FindFirstObjectByType<ColorCounter>();
            if (colorCounter != null && colorCounter.HasToyObject()) {
                stepNumberStr = "4/4";
                mainAction = "Pick up painted " + trackedOrder.GetToyName();
                detail = "Take from Paint Station and walk to Delivery Counter.";
                targetCounter = colorCounter;
            } else {
                MoldMakerCounter moldCounter = FindFirstObjectByType<MoldMakerCounter>();
                WoodShaperCounter shaperCounter = FindFirstObjectByType<WoodShaperCounter>();

                if (moldCounter != null && moldCounter.HasToyObject() && moldCounter.GetState() != MoldMakerCounter.State.ChoosingRecipe) {
                    stepNumberStr = "3/4";
                    mainAction = "Pick up molded " + trackedOrder.GetToyName();
                    detail = "Take it to the Paint Station to color it.";
                    targetCounter = moldCounter;
                } else if (shaperCounter != null && shaperCounter.HasToyObject() && shaperCounter.GetState() != WoodShaperCounter.State.ChoosingRecipe) {
                    stepNumberStr = "3/4";
                    mainAction = "Pick up carved " + trackedOrder.GetToyName();
                    detail = "Take it to the Paint Station to color it.";
                    targetCounter = shaperCounter;
                } else {
                    MelterCounter melter = FindFirstObjectByType<MelterCounter>();
                    WoodCounter cutter = FindFirstObjectByType<WoodCounter>();

                    if (melter != null && melter.HasToyObject()) {
                        stepNumberStr = "2/4";
                        mainAction = "Pick up melted material from Melter";
                        detail = "Carry it to the Mold Maker.";
                        targetCounter = melter;
                    } else if (cutter != null && cutter.HasToyObject()) {
                        stepNumberStr = "2/4";
                        mainAction = "Pick up cut wood from Wood Cutter";
                        detail = "Carry it to the Wood Shaper.";
                        targetCounter = cutter;
                    } else {
                        // Start step 1: Collect raw material
                        stepNumberStr = "1/4";
                        string rawNeeded = GetRawMaterialNameForTrackedOrder();
                        mainAction = "Collect " + rawNeeded + " from Container";
                        detail = "Walk to the " + rawNeeded + " container and press [E].";
                        targetCounter = FindContainerForMaterial(rawNeeded);
                    }
                }
            }
        }

        if (actionText != null) actionText.text = mainAction;
        if (detailText != null) detailText.text = detail;
        if (stepBadgeText != null) stepBadgeText.text = stepNumberStr;

        currentTargetCounter = targetCounter;
        currentTargetStationName = targetCounter != null ? targetCounter.GetStationName() : "TARGET";
    }

    private void UpdateWorldBeaconPosition() {
        if (worldBeaconRoot == null) return;

        if (currentTargetCounter == null) {
            worldBeaconRoot.SetActive(false);
            return;
        }

        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null || canvasRect == null) {
            worldBeaconRoot.SetActive(false);
            return;
        }

        // Float beacon 2.8 units above counter
        Vector3 worldPos = currentTargetCounter.transform.position + Vector3.up * 2.8f;
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPos);

        if (screenPoint.z < 0f) {
            worldBeaconRoot.SetActive(false);
            return;
        }

        worldBeaconRoot.SetActive(true);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPos)) {
            // Gentle bobbing motion
            float bob = Mathf.Sin(Time.time * 5f) * 8f;
            worldBeaconRoot.GetComponent<RectTransform>().anchoredPosition = localPos + new Vector2(0f, bob);
        }

        // Pulse the halo
        if (beaconHalo != null) {
            float haloScale = 1f + Mathf.Sin(Time.time * 6f) * 0.2f;
            beaconHalo.rectTransform.localScale = Vector3.one * haloScale;
        }

        if (beaconLabel != null) {
            beaconLabel.text = currentTargetStationName.ToUpper();
        }
    }

    private string GetRawMaterialNameForTrackedOrder() {
        if (trackedRecipe != null && !string.IsNullOrEmpty(trackedRecipe.materialName) && trackedRecipe.materialName != "Mystery") {
            return trackedRecipe.materialName;
        }
        string name = trackedOrder.GetToyName().ToLower();
        if (name.Contains("sword") || name.Contains("car") || name.Contains("bear") || name.Contains("wood")) {
            return "Wood";
        }
        if (name.Contains("robot") || name.Contains("train") || name.Contains("metal")) {
            return "Metal";
        }
        return "Plastic";
    }

    private BaseCounter FindContainerForMaterial(string matName) {
        foreach (ContainerCounter container in FindObjectsByType<ContainerCounter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
            if (container.GetToyObjectSO() != null) {
                string containerToy = ToyObjectSO.GetDisplayName(container.GetToyObjectSO());
                if (containerToy.IndexOf(matName, StringComparison.OrdinalIgnoreCase) >= 0) {
                    return container;
                }
            }
        }
        // Fallback: any container
        return FindFirstObjectByType<ContainerCounter>();
    }
}
