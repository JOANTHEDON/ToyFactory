using System;

public class TrashCounter : BaseCounter {

    public static event EventHandler OnAnyObjectTrashed;

    public override void Interact(Player player) {
        if (player.HasToyObject()) {
            player.GetToyObject().DestroySelf();
            OnAnyObjectTrashed?.Invoke(this, EventArgs.Empty);
        }
    }

    public override string GetInteractHint(Player player) {
        if (!player.HasToyObject()) return null;
        return "Throw away " + ToyObjectSO.GetDisplayName(player.GetToyObject().GetToyObjectSO());
    }

    public override string GetStationName() {
        return "Trash";
    }

    public static void ResetStaticData() {
        OnAnyObjectTrashed = null;
    }
}
