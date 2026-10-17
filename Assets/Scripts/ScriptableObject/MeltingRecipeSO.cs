using UnityEngine;
[CreateAssetMenu()]
public class MeltingRecipeSO : ScriptableObject {
    public ToyObjectSO input;
    public ToyObjectSO output;
    public int meltingProgressMax;
}
