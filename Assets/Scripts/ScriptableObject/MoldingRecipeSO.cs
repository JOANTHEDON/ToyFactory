using UnityEngine;

[CreateAssetMenu(fileName = "MoldingRecipeSO", menuName = "ScriptableObjects/MoldingRecipeSO")]
public class MoldingRecipeSO : ScriptableObject {
    public ToyObjectSO input;
    public ToyObjectSO[] availableOutputs;
    public float moldingTimerMax = 4f;
}
