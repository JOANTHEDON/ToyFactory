using UnityEngine;

public class ToyObject : MonoBehaviour {
    [SerializeField] private ToyObjectSO toyObjectSO;
    [SerializeField] private Material originalMaterial;

    private IToyObjectParent toyObjectParent;
    private bool isColored = false;

    public ToyObjectSO GetToyObjectSO() {
        return toyObjectSO;
    }

    public Material GetOriginalMaterial() {
        return originalMaterial;
    }

    public bool HasOriginalMaterial() {
        return originalMaterial != null;
    }

    public bool IsColored() {
        return isColored;
    }

    public void ApplyOriginalMaterial() {
        if (originalMaterial != null) {
            ApplyMaterial(originalMaterial);
            isColored = true;
        }
    }

    public void ApplyMaterial(Material material) {
        if (material == null) return;
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in meshRenderers) {
            meshRenderer.material = material;
        }
        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers) {
            skinnedMeshRenderer.material = material;
        }
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

    public void DestroySelf() {
        toyObjectParent.ClearToyObject();
        Destroy(gameObject);
    }
    public static ToyObject SpawnToyObject(ToyObjectSO toyObjectSO, IToyObjectParent toyObjectParent) {
        Transform toyObjectTransform = Instantiate(toyObjectSO.prefab);
        ToyObject toyObject = toyObjectTransform.GetComponent<ToyObject>();
        toyObject.SetToyObjectParent(toyObjectParent);

        return toyObject;
    }
}
