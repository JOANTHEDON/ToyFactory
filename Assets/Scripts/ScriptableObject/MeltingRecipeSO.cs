using UnityEngine;
[CreateAssetMenu()]
public class MeltingRecipeSO : ScriptableObject {
    public ToyObjectSO input;
    public ToyObjectSO output;
    public ToyObjectSO holderOutput;
    public int meltingProgressMax;
}
