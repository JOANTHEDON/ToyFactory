using UnityEngine;

public class ContainerCounterVisual : MonoBehaviour {
    private const string OPEN_CLOSE = "OpenClose";
    [SerializeField] private ContainerCounter containerCounter;

    [SerializeField] private MeshRenderer quadMeshRenderer;
    [SerializeField] private SpriteRenderer iconSpriteRenderer;

    private Animator animator;
    private void Awake() {
        animator = GetComponent<Animator>();
        if (quadMeshRenderer == null) {
            quadMeshRenderer = GetComponentInChildren<MeshRenderer>();
        }
        if (iconSpriteRenderer == null) {
            iconSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Start() {
        containerCounter.OnPlayerGrabbedObject += ContainerCounter_OnPlayerGrabbedObject;
        UpdateVisualIcon();
    }

    private void UpdateVisualIcon() {
        if (containerCounter == null) return;
        ToyObjectSO toyObjectSO = containerCounter.GetToyObjectSO();
        if (toyObjectSO == null || toyObjectSO.sprite == null) return;

        if (quadMeshRenderer != null) {
            quadMeshRenderer.material.mainTexture = toyObjectSO.sprite.texture;
        }

        if (iconSpriteRenderer != null) {
            iconSpriteRenderer.sprite = toyObjectSO.sprite;
        }
    }

    private void ContainerCounter_OnPlayerGrabbedObject(object sender, System.EventArgs e) {
        animator.SetTrigger(OPEN_CLOSE);
    }
}
