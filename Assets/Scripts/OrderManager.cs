using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour {

    public static OrderManager Instance { get; private set; }

    public event EventHandler OnOrderSpawned;
    public event EventHandler OnOrderCompleted;

    [Header("Available Toy Recipes (Orders)")]
    [SerializeField] private List<ToyObjectSO> availableToyOrderList;

    private List<ToyObjectSO> activeOrderList;
    private int maxOrderCount = 4;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There is more than one OrderManager instance: " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        activeOrderList = new List<ToyObjectSO>();
    }

    private void Start() {
        InitializeOrders();
    }

    private void InitializeOrders() {
        if (availableToyOrderList != null) {
            availableToyOrderList.RemoveAll(item => item == null);
        }

        if (availableToyOrderList == null || availableToyOrderList.Count == 0) {
            Debug.LogWarning("OrderManager: availableToyOrderList is empty!");
            return;
        }

        while (activeOrderList.Count < maxOrderCount) {
            SpawnRandomOrder();
        }
    }

    private void SpawnRandomOrder() {
        if (availableToyOrderList == null || availableToyOrderList.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, availableToyOrderList.Count);
        ToyObjectSO randomToy = availableToyOrderList[randomIndex];
        if (randomToy != null) {
            activeOrderList.Add(randomToy);
            OnOrderSpawned?.Invoke(this, EventArgs.Empty);
        }
    }

    public void CompleteOrder(ToyObjectSO toyObjectSO) {
        if (toyObjectSO == null) return;

        int index = activeOrderList.IndexOf(toyObjectSO);
        if (index != -1) {
            activeOrderList.RemoveAt(index);
            OnOrderCompleted?.Invoke(this, EventArgs.Empty);

            // Refill to keep 4 active orders
            if (activeOrderList.Count < maxOrderCount) {
                SpawnRandomOrder();
            }
        }
    }

    public bool IsOrderActive(ToyObjectSO toyObjectSO) {
        if (toyObjectSO == null || activeOrderList == null) return false;
        return activeOrderList.Contains(toyObjectSO);
    }

    public List<ToyObjectSO> GetActiveOrderList() {
        return activeOrderList;
    }

    public List<ToyObjectSO> GetAvailableToyOrderList() {
        return availableToyOrderList;
    }
}
