using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single source of truth for customer orders.
/// Orders arrive one by one, each with its own patience timer, and sit in a queue.
/// The player may serve them in any order: delivering a toy completes the earliest order that wants that toy.
/// Customers (CustomerManager) and the top order bar (OrderManagerUI) both mirror this list.
/// </summary>
public class OrderManager : MonoBehaviour {

    public static OrderManager Instance { get; private set; }

    public class Order {
        public int id;
        public ToyObjectSO toyObjectSO;
        public float timeMax;
        public float timeRemaining;

        public float GetTimeNormalized() {
            return timeMax > 0f ? Mathf.Clamp01(timeRemaining / timeMax) : 0f;
        }

        public bool IsUrgent() {
            return GetTimeNormalized() < 0.25f;
        }

        public string GetToyName() {
            return ToyObjectSO.GetDisplayName(toyObjectSO);
        }
    }

    public class OrderEventArgs : EventArgs {
        public Order order;
        public int reward;
        public int queueIndex;
    }

    public event EventHandler<OrderEventArgs> OnOrderSpawned;
    public event EventHandler<OrderEventArgs> OnOrderCompleted;
    public event EventHandler<OrderEventArgs> OnOrderExpired;
    public event EventHandler OnOrderListChanged;
    public event EventHandler OnScoreChanged;

    [Header("Available Toy Recipes (Orders)")]
    [SerializeField] private List<ToyObjectSO> availableToyOrderList;

    [Header("Queue")]
    [SerializeField] private int maxOrderCount = 4;
    [SerializeField] private float firstOrderDelay = 3f;
    [SerializeField] private float spawnIntervalMin = 9f;
    [SerializeField] private float spawnIntervalMax = 16f;

    [Header("Patience")]
    [Tooltip("Seconds a customer waits before leaving angry.")]
    [SerializeField] private float orderTimeMax = 110f;
    [Tooltip("Patience multiplier for the first 4 tutorial orders so player has ample time to complete them.")]
    [SerializeField] private float tutorialOrderTimeMultiplier = 2.5f;

    [Header("Scoring")]
    [SerializeField] private int rewardPerOrder = 100;
    [SerializeField] private int speedBonusMax = 50;

    private readonly List<Order> activeOrderList = new List<Order>();
    private float spawnTimer;
    private int nextOrderId = 1;
    private int score;
    private int deliveredCount;
    private int missedCount;
    private ToyObjectSO lastSpawnedToy;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("There is more than one OrderManager instance: " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    private void Start() {
        if (availableToyOrderList != null) {
            availableToyOrderList.RemoveAll(item => item == null);
        }

        if (availableToyOrderList == null || availableToyOrderList.Count == 0) {
            Debug.LogWarning("OrderManager: availableToyOrderList is empty!");
        }

        spawnTimer = firstOrderDelay;
    }

    private void Update() {
        if (availableToyOrderList == null || availableToyOrderList.Count == 0) return;

        float deltaTime = Time.deltaTime;

        // Spawn new orders while there is room in the queue
        if (activeOrderList.Count < maxOrderCount) {
            spawnTimer -= deltaTime;
            if (spawnTimer <= 0f) {
                SpawnRandomOrder();
                spawnTimer = UnityEngine.Random.Range(spawnIntervalMin, spawnIntervalMax);
            }
        }

        // Tick patience timers (time slows down during tutorial orders so player can complete them comfortably)
        float patienceDelta = deltaTime;
        if (deliveredCount < 4) {
            patienceDelta *= 0.4f; // 2.5x slower patience drain during first 4 orders
        }

        for (int i = activeOrderList.Count - 1; i >= 0; i--) {
            Order order = activeOrderList[i];
            order.timeRemaining -= patienceDelta;
            if (order.timeRemaining <= 0f) {
                order.timeRemaining = 0f;
                activeOrderList.RemoveAt(i);
                missedCount++;
                OnOrderExpired?.Invoke(this, new OrderEventArgs { order = order, reward = 0, queueIndex = i });
                OnOrderListChanged?.Invoke(this, EventArgs.Empty);
                OnScoreChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void SpawnRandomOrder() {
        ToyObjectSO toy = PickNextToy();
        if (toy == null) return;

        float effectiveTimeMax = orderTimeMax;
        if (deliveredCount < 4) {
            effectiveTimeMax *= tutorialOrderTimeMultiplier;
        }

        Order order = new Order {
            id = nextOrderId++,
            toyObjectSO = toy,
            timeMax = effectiveTimeMax,
            timeRemaining = effectiveTimeMax,
        };
        activeOrderList.Add(order);
        lastSpawnedToy = toy;

        OnOrderSpawned?.Invoke(this, new OrderEventArgs { order = order, queueIndex = activeOrderList.Count - 1 });
        OnOrderListChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Prefers toys that are not already in the queue so the player sees variety.</summary>
    private ToyObjectSO PickNextToy() {
        if (availableToyOrderList == null || availableToyOrderList.Count == 0) return null;

        List<ToyObjectSO> candidates = new List<ToyObjectSO>();
        foreach (ToyObjectSO toy in availableToyOrderList) {
            if (toy == null) continue;
            if (toy == lastSpawnedToy) continue;
            bool alreadyQueued = false;
            foreach (Order order in activeOrderList) {
                if (order.toyObjectSO == toy) { alreadyQueued = true; break; }
            }
            if (!alreadyQueued) candidates.Add(toy);
        }

        if (candidates.Count == 0) {
            candidates.AddRange(availableToyOrderList);
        }

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    /// <summary>
    /// Try to serve a toy. The earliest queued order asking for this toy is completed, whichever position it is in.
    /// Returns the completed order, or null when nobody ordered that toy.
    /// </summary>
    public Order TryDeliver(ToyObjectSO toyObjectSO) {
        return TryDeliver(toyObjectSO, out _);
    }

    public Order TryDeliver(ToyObjectSO toyObjectSO, out int rewardEarned) {
        rewardEarned = 0;
        if (toyObjectSO == null) return null;

        for (int i = 0; i < activeOrderList.Count; i++) {
            Order order = activeOrderList[i];
            if (order.toyObjectSO != toyObjectSO) continue;

            activeOrderList.RemoveAt(i);
            deliveredCount++;

            int reward = rewardPerOrder + Mathf.RoundToInt(speedBonusMax * order.GetTimeNormalized());
            rewardEarned = reward;
            score += reward;
            if (score > GameSettings.BestScore) {
                GameSettings.BestScore = score;
            }

            OnOrderCompleted?.Invoke(this, new OrderEventArgs { order = order, reward = reward, queueIndex = i });
            OnOrderListChanged?.Invoke(this, EventArgs.Empty);
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
            return order;
        }

        return null;
    }

    /// <summary>Legacy entry point kept for older callers.</summary>
    public void CompleteOrder(ToyObjectSO toyObjectSO) {
        TryDeliver(toyObjectSO);
    }

    public bool IsOrderActive(ToyObjectSO toyObjectSO) {
        if (toyObjectSO == null) return false;
        foreach (Order order in activeOrderList) {
            if (order.toyObjectSO == toyObjectSO) return true;
        }
        return false;
    }

    public List<Order> GetActiveOrderList() {
        return activeOrderList;
    }

    public int GetQueueIndex(Order order) {
        return activeOrderList.IndexOf(order);
    }

    public List<ToyObjectSO> GetAvailableToyOrderList() {
        return availableToyOrderList;
    }

    public int GetScore() { return score; }
    public int GetDeliveredCount() { return deliveredCount; }
    public int GetMissedCount() { return missedCount; }
    public int GetMaxOrderCount() { return maxOrderCount; }
    public float GetTimeUntilNextOrder() { return activeOrderList.Count < maxOrderCount ? Mathf.Max(0f, spawnTimer) : -1f; }
}
