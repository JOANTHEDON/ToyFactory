using System;
using UnityEngine;

public class DeliveryCounter : BaseCounter {

    public static DeliveryCounter Instance { get; private set; }

    public event EventHandler OnDeliverySuccess;
    public event EventHandler OnDeliveryFailed;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There is more than one DeliveryCounter instance: " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void Interact(Player player) {
        if (player.HasToyObject()) {
            ToyObject toyObject = player.GetToyObject();

            // Object must be colored (finished)
            if (!toyObject.IsColored()) {
                Debug.Log("DeliveryCounter: Toy is not colored yet! Complete coloring before delivering.");
                OnDeliveryFailed?.Invoke(this, EventArgs.Empty);
                return;
            }

            ToyObjectSO toySO = toyObject.GetToyObjectSO();

            // Check if this toy is an active order in OrderManager
            if (OrderManager.Instance != null && OrderManager.Instance.IsOrderActive(toySO)) {
                Debug.Log("DeliveryCounter: Order delivered successfully for " + toySO.objectName + "!");
                
                // Destroy delivered toy
                player.GetToyObject().DestroySelf();

                // Complete order and refill in OrderManager
                OrderManager.Instance.CompleteOrder(toySO);

                OnDeliverySuccess?.Invoke(this, EventArgs.Empty);
            } else {
                Debug.Log("DeliveryCounter: Toy " + (toySO != null ? toySO.objectName : "null") + " is not currently in the active order list!");
                OnDeliveryFailed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
