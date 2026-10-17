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
    public Transform GetToyObjectFollowTransform() {
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
