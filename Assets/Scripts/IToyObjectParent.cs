using UnityEngine;

public interface IToyObjectParent {
    public Transform GetToyObjectFollowTransform();

    public void SetToyObject(ToyObject toyObject);

    public ToyObject GetToyObject();

    public void ClearToyObject();

    public bool HasToyObject();
}
