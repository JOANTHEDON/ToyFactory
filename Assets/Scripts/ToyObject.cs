using System;
using UnityEngine;

public class ToyObject : MonoBehaviour {

    /// <summary>Fired whenever any toy object is parented to the player (taken from a container, picked up from a counter...).</summary>
    public static event EventHandler OnAnyObjectPickedUp;
    /// <summary>Fired whenever any toy object is placed on a counter.</summary>
    public static event EventHandler OnAnyObjectPlaced;

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
            return;
        }
        if (this.toyObjectParent != null) {
            this.toyObjectParent.ClearToyObject();
        }

        if (toyObjectParent.HasToyObject()) {
            Debug.LogError("Counter already has a ToyObject");
        }
        this.toyObjectParent = toyObjectParent;

        toyObjectParent.SetToyObject(this);

        transform.parent = toyObjectParent.GetToyObjectFollowTransform();
        transform.localPosition = Vector3.zero;

        if (toyObjectParent is Player) {
            OnAnyObjectPickedUp?.Invoke(this, EventArgs.Empty);
        } else {
            OnAnyObjectPlaced?.Invoke(this, EventArgs.Empty);
        }
    }

    public IToyObjectParent GetToyObjectParent() {
        return toyObjectParent;
    }

    public void DestroySelf() {
        if (toyObjectParent != null) {
            toyObjectParent.ClearToyObject();
        }
        Destroy(gameObject);
    }
    public static ToyObject SpawnToyObject(ToyObjectSO toyObjectSO, IToyObjectParent toyObjectParent) {
        Transform toyObjectTransform = Instantiate(toyObjectSO.prefab);
        ToyObject toyObject = toyObjectTransform.GetComponent<ToyObject>();
        toyObject.SetToyObjectParent(toyObjectParent);

        return toyObject;
    }

    public static void ResetStaticData() {
        OnAnyObjectPickedUp = null;
        OnAnyObjectPlaced = null;
    }
}
