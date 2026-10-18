using System.Collections.Generic;
using UnityEngine;

public class OrderManagerUI : MonoBehaviour {

    [SerializeField] private Transform orderContainer;
    [SerializeField] private Transform orderTemplate;

    private List<GameObject> spawnedOrderUIList = new List<GameObject>();

    private void Awake() {
        if (orderTemplate != null) {
            orderTemplate.gameObject.SetActive(false);
        }
    }

    private void Start() {
        if (OrderManager.Instance != null) {
            OrderManager.Instance.OnOrderSpawned += OrderManager_OnOrderSpawned;
            OrderManager.Instance.OnOrderCompleted += OrderManager_OnOrderCompleted;
        }

        UpdateVisual();
    }

    private void OrderManager_OnOrderSpawned(object sender, System.EventArgs e) {
        UpdateVisual();
    }

    private void OrderManager_OnOrderCompleted(object sender, System.EventArgs e) {
        UpdateVisual();
    }

    private void UpdateVisual() {
        foreach (GameObject obj in spawnedOrderUIList) {
            Destroy(obj);
        }
        spawnedOrderUIList.Clear();

        if (OrderManager.Instance == null || orderContainer == null || orderTemplate == null) return;

        List<ToyObjectSO> activeOrders = OrderManager.Instance.GetActiveOrderList();
        foreach (ToyObjectSO toySO in activeOrders) {
            if (toySO == null) continue;

            Transform orderTransform = Instantiate(orderTemplate, orderContainer);
            orderTransform.gameObject.SetActive(true);
            spawnedOrderUIList.Add(orderTransform.gameObject);

            OrderItemUI orderItemUI = orderTransform.GetComponent<OrderItemUI>();
            if (orderItemUI != null) {
                orderItemUI.SetToyObjectSO(toySO);
            }
        }
    }

    private void OnDestroy() {
        if (OrderManager.Instance != null) {
            OrderManager.Instance.OnOrderSpawned -= OrderManager_OnOrderSpawned;
            OrderManager.Instance.OnOrderCompleted -= OrderManager_OnOrderCompleted;
        }
    }
}
