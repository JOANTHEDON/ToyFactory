using UnityEngine;

public class BaseCounter : MonoBehaviour, IToyObjectParent {

    [SerializeField] private Transform counterTopPoint;

    private ToyObject toyObject;

    public virtual void Interact(Player player) {
        Debug.LogError("BaseCounter.Interact():");
    }
    public virtual void InteractAlternate(Player player) {
        //Debug.LogError("BaseCounter.InteractAlternate():");
    }

    /// <summary>
    /// Short label for the primary action the player can do here right now (e.g. "Pick up Wood").
    /// Return null when nothing would happen, so prompts and mobile buttons hide themselves.
    /// </summary>
    public virtual string GetInteractHint(Player player) {
        if (HasToyObject() && !player.HasToyObject()) {
            return "Pick up " + ToyObjectSO.GetDisplayName(GetToyObject().GetToyObjectSO());
        }
        if (!HasToyObject() && player.HasToyObject()) {
            return "Place " + ToyObjectSO.GetDisplayName(player.GetToyObject().GetToyObjectSO());
        }
        return null;
    }

    /// <summary>Short label for the alternate action (F / MELT button), or null when there is none.</summary>
    public virtual string GetInteractAlternateHint(Player player) {
        return null;
    }

    /// <summary>A short status line shown while the counter is busy (e.g. "Molding..."), or null.</summary>
    public virtual string GetBusyHint() {
        return null;
    }

    /// <summary>Human readable station name used by prompts and recipe steps.</summary>
    public virtual string GetStationName() {
        return "Counter";
    }

    public virtual Transform GetToyObjectFollowTransform() {
        return counterTopPoint;
    }

    public void SetToyObject(ToyObject toyObject) {
        this.toyObject = toyObject;
    }

    public ToyObject GetToyObject() {
        return toyObject;
    }

    public void ClearToyObject() {
        toyObject = null;
    }

    public bool HasToyObject() {
        return toyObject != null;
    }


}
