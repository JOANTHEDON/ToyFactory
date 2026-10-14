using UnityEngine;

public class ToyObject : MonoBehaviour {
    [SerializeField] private ToyObjectSO toyObjectSO;

    private IToyObjectParent toyObjectParent;
    public ToyObjectSO GetToyObjectSO() {
        return toyObjectSO;
    }

    public void SetToyObjectParent(IToyObjectParent toyObjectParent) {
        if (toyObjectParent == null) {
            //toyObjectParent.SetToyObjectParent(this);

            return;
        }
        if (this.toyObjectParent != null) {
            this.toyObjectParent.ClearToyObject();
        }


        //this.toyObjectParent = toyObjectParent;
        if (toyObjectParent.HasToyObject()) {
            Debug.LogError("Counter already has a ToyObject");
        }
        this.toyObjectParent = toyObjectParent;

        toyObjectParent.SetToyObject(this);

        transform.parent = toyObjectParent.GetToyObjectFollowTransform();
        transform.localPosition = Vector3.zero;
    }

    public IToyObjectParent GetToyObjectParent() {
        return toyObjectParent;
    }
}
