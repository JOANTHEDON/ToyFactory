using System;
using UnityEngine;

public class ContainerCounter : BaseCounter {

    public event EventHandler OnPlayerGrabbedObject;

    [SerializeField]
    private ToyObjectSO toyObjectSO;
    public override void Interact(Player player) {
        if (!player.HasToyObject()) {
            //Player is not carrying anything
            ToyObject.SpawnToyObject(toyObjectSO, player);

            OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
        }
    }

    public override string GetInteractHint(Player player) {
        if (player.HasToyObject()) return null;
        return "Take " + ToyObjectSO.GetDisplayName(toyObjectSO);
    }

    public override string GetStationName() {
        return ToyObjectSO.GetDisplayName(toyObjectSO) + " Container";
    }

    public ToyObjectSO GetToyObjectSO() {
        return toyObjectSO;
    }
}
