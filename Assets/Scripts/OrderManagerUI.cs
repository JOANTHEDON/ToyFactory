using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Top-left order bar. One card per queued order, in queue order, each with the toy icon, name,
/// queue number and a patience bar. Tapping a card opens its recipe.
/// </summary>
public class OrderManagerUI : MonoBehaviour {

    [SerializeField] private Transform orderContainer;
    [SerializeField] private Transform orderTemplate;

    private readonly List<GameObject> spawnedOrderUIList = new List<GameObject>();
    private TextMeshProUGUI emptyText;
    private TextMeshProUGUI headerText;

    private void Awake() {
        if (orderTemplate != null) {
            orderTemplate.gameObject.SetActive(false);
        }
    }

    private void Start() {
        BuildChrome();

        if (OrderManager.Instance != null) {
            OrderManager.Instance.OnOrderListChanged += OrderManager_OnOrderListChanged;
        }

        UpdateVisual();
    }

    private void BuildChrome() {
        RectTransform rect = GetComponent<RectTransform>();

        Image header = UIFactory.CreateChip("Header", transform, "ORDERS", UIFactory.Brown, UIFactory.Cream, 22f, new Vector2(150f, 36f));
        UIFactory.Anchor(header.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, 6f), new Vector2(150f, 36f));
        headerText = header.GetComponentInChildren<TextMeshProUGUI>();

        emptyText = UIFactory.CreateText("EmptyText", transform, "Customers are on their way...", 22f, UIFactory.Cream, TextAlignmentOptions.Left);
        UIFactory.Anchor(emptyText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -58f), new Vector2(600f, 36f));
        emptyText.fontStyle = FontStyles.Italic;
    }

    private void OrderManager_OnOrderListChanged(object sender, System.EventArgs e) {
        UpdateVisual();
    }

    private void Update() {
        if (OrderManager.Instance == null || headerText == null) return;
        float next = OrderManager.Instance.GetTimeUntilNextOrder();
        int count = OrderManager.Instance.GetActiveOrderList().Count;
        headerText.text = count > 0 ? "ORDERS  " + count : "ORDERS";
        if (emptyText != null) {
            emptyText.gameObject.SetActive(count == 0);
            if (count == 0 && next >= 0f) {
                emptyText.text = "Next customer in " + Mathf.CeilToInt(next) + "s...";
            }
        }
    }

    private void UpdateVisual() {
        foreach (GameObject obj in spawnedOrderUIList) {
            Destroy(obj);
        }
        spawnedOrderUIList.Clear();

        if (OrderManager.Instance == null || orderContainer == null || orderTemplate == null) return;

        List<OrderManager.Order> activeOrders = OrderManager.Instance.GetActiveOrderList();
        for (int i = 0; i < activeOrders.Count; i++) {
            OrderManager.Order order = activeOrders[i];
            if (order == null || order.toyObjectSO == null) continue;

            Transform orderTransform = Instantiate(orderTemplate, orderContainer);
            orderTransform.gameObject.SetActive(true);
            spawnedOrderUIList.Add(orderTransform.gameObject);

            OrderItemUI orderItemUI = orderTransform.GetComponent<OrderItemUI>();
            if (orderItemUI != null) {
                orderItemUI.SetOrder(order, i + 1);
            }
        }
    }

    private void OnDestroy() {
        if (OrderManager.Instance != null) {
            OrderManager.Instance.OnOrderListChanged -= OrderManager_OnOrderListChanged;
        }
    }
}
