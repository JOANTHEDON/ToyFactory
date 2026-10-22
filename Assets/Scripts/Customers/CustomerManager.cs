using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns one visible customer per order from OrderManager, walks them to the queue outside the
/// delivery window, and sends them away (happy or angry) when their order is served or expires.
/// Queue positions always mirror OrderManager's list order, so the #1 card in the HUD is the
/// customer standing closest to the counter.
/// </summary>
public class CustomerManager : MonoBehaviour {

    public static CustomerManager Instance { get; private set; }

    [Header("Visuals")]
    [Tooltip("Character model to instantiate for each customer (the same Idle model the player uses works fine).")]
    [SerializeField] private Transform customerVisualPrefab;
    [SerializeField] private RuntimeAnimatorController animatorController;
    [SerializeField] private float visualScale = 2f;
    [SerializeField] private Color[] tintColors;

    [Header("Path")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform exitPoint;
    [Tooltip("Queue slots, first = closest to the delivery counter. Extra customers line up behind the last slot.")]
    [SerializeField] private Transform[] queueSlots;
    [SerializeField] private Transform lookAtTarget;
    [SerializeField] private float moveSpeed = 4f;

    private readonly List<Customer> customers = new List<Customer>();

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (tintColors == null || tintColors.Length == 0) {
            tintColors = new Color[] {
                new Color(1f, 0.55f, 0.55f),
                new Color(0.5f, 0.75f, 1f),
                new Color(0.55f, 1f, 0.6f),
                new Color(1f, 0.9f, 0.35f),
                new Color(0.8f, 0.6f, 1f),
                new Color(1f, 0.7f, 0.35f),
            };
        }
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
        if (OrderManager.Instance != null) {
            OrderManager.Instance.OnOrderSpawned -= OrderManager_OnOrderSpawned;
            OrderManager.Instance.OnOrderCompleted -= OrderManager_OnOrderCompleted;
            OrderManager.Instance.OnOrderExpired -= OrderManager_OnOrderExpired;
        }
    }

    private void Start() {
        if (OrderManager.Instance != null) {
            OrderManager.Instance.OnOrderSpawned += OrderManager_OnOrderSpawned;
            OrderManager.Instance.OnOrderCompleted += OrderManager_OnOrderCompleted;
            OrderManager.Instance.OnOrderExpired += OrderManager_OnOrderExpired;

            // Orders that already exist (e.g. when this manager is enabled late)
            foreach (OrderManager.Order order in OrderManager.Instance.GetActiveOrderList()) {
                SpawnCustomer(order);
            }
        }

        if (lookAtTarget == null && DeliveryCounter.Instance != null) {
            lookAtTarget = DeliveryCounter.Instance.transform;
        }
    }

    private void OrderManager_OnOrderSpawned(object sender, OrderManager.OrderEventArgs e) {
        SpawnCustomer(e.order);
    }

    private void OrderManager_OnOrderCompleted(object sender, OrderManager.OrderEventArgs e) {
        Customer customer = FindCustomer(e.order);
        if (customer != null) {
            customers.Remove(customer);
            customer.Leave(true, GetExitPosition());
        }
        ReassignSlots();
    }

    private void OrderManager_OnOrderExpired(object sender, OrderManager.OrderEventArgs e) {
        Customer customer = FindCustomer(e.order);
        if (customer != null) {
            customers.Remove(customer);
            customer.Leave(false, GetExitPosition());
        }
        ReassignSlots();
    }

    private void SpawnCustomer(OrderManager.Order order) {
        if (order == null || FindCustomer(order) != null) return;

        GameObject customerGO = new GameObject("Customer " + order.id);
        customerGO.transform.SetParent(transform, false);
        customerGO.transform.position = GetSpawnPosition();

        Customer customer = customerGO.AddComponent<Customer>();
        Color tint = tintColors[order.id % tintColors.Length];
        customer.Setup(order, customerVisualPrefab, animatorController, visualScale, tint, moveSpeed, lookAtTarget);
        customers.Add(customer);

        ReassignSlots();
    }

    private void ReassignSlots() {
        for (int i = 0; i < customers.Count; i++) {
            customers[i].SetQueueTarget(GetSlotPosition(i), i);
        }
    }

    private Customer FindCustomer(OrderManager.Order order) {
        foreach (Customer customer in customers) {
            if (customer.GetOrder() == order) return customer;
        }
        return null;
    }

    private Vector3 GetSpawnPosition() {
        if (spawnPoint != null) return spawnPoint.position;
        return transform.position;
    }

    private Vector3 GetExitPosition() {
        if (exitPoint != null) return exitPoint.position;
        return GetSpawnPosition();
    }

    public Vector3 GetSlotPosition(int index) {
        if (queueSlots == null || queueSlots.Length == 0) {
            return transform.position + Vector3.forward * (index * 2.5f);
        }
        if (index < queueSlots.Length && queueSlots[index] != null) {
            return queueSlots[index].position;
        }

        // Past the last slot: keep extending the line in the same direction
        Transform last = queueSlots[queueSlots.Length - 1];
        Vector3 direction = Vector3.forward * 2.5f;
        if (queueSlots.Length >= 2 && queueSlots[queueSlots.Length - 2] != null && last != null) {
            direction = last.position - queueSlots[queueSlots.Length - 2].position;
        }
        return (last != null ? last.position : transform.position) + direction * (index - queueSlots.Length + 1);
    }

    public int GetCustomerCount() {
        return customers.Count;
    }
}
