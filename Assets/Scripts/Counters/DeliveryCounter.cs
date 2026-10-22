using System;
using UnityEngine;

public class DeliveryCounter : BaseCounter {

    public static DeliveryCounter Instance { get; private set; }

    public class DeliveryEventArgs : EventArgs {
        public bool success;
        public string message;
        public ToyObjectSO toyObjectSO;
        public int reward;
        public OrderManager.Order order;
    }

    public event EventHandler OnDeliverySuccess;
    public event EventHandler OnDeliveryFailed;
    public event EventHandler<DeliveryEventArgs> OnDeliveryResult;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogError("There is more than one DeliveryCounter instance: " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    public override void Interact(Player player) {
        if (!player.HasToyObject()) {
            return;
        }

        ToyObject toyObject = player.GetToyObject();
        ToyObjectSO toySO = toyObject.GetToyObjectSO();
        string toyName = ToyObjectSO.GetDisplayName(toySO);

        // Object must be colored (finished)
        if (!toyObject.IsColored()) {
            Fail("Paint the " + toyName + " first!", toySO);
            return;
        }

        if (OrderManager.Instance == null) {
            Fail("No customers right now.", toySO);
            return;
        }

        OrderManager.Order order = OrderManager.Instance.TryDeliver(toySO, out int reward);
        if (order == null) {
            Fail("Nobody ordered a " + toyName + ".", toySO);
            return;
        }

        // Destroy delivered toy
        toyObject.DestroySelf();

        OnDeliverySuccess?.Invoke(this, EventArgs.Empty);
        OnDeliveryResult?.Invoke(this, new DeliveryEventArgs {
            success = true,
            message = toyName + " delivered!  +" + reward,
            toyObjectSO = toySO,
            order = order,
            reward = reward,
        });
    }

    private void Fail(string message, ToyObjectSO toySO) {
        Debug.Log("DeliveryCounter: " + message);
        OnDeliveryFailed?.Invoke(this, EventArgs.Empty);
        OnDeliveryResult?.Invoke(this, new DeliveryEventArgs {
            success = false,
            message = message,
            toyObjectSO = toySO,
        });
    }

    public override string GetInteractHint(Player player) {
        if (!player.HasToyObject()) return null;
        ToyObject toy = player.GetToyObject();
        string name = ToyObjectSO.GetDisplayName(toy.GetToyObjectSO());
        if (!toy.IsColored()) return "Deliver " + name + " (needs paint)";
        return "Deliver " + name;
    }

    public override string GetStationName() {
        return "Delivery Counter";
    }
}
