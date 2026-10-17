using System;
using UnityEngine;
public class MelterCounter : BaseCounter {

    public event EventHandler<OnProgressChangedEventArgs> OnProgressChanged;
    public class OnProgressChangedEventArgs : EventArgs {
        public float progressNormalized;
    }
    [SerializeField] private MeltingRecipeSO[] meltingRecipeSOArray;

    private int meltingProgress;
    public override void Interact(Player player) {
        if (!HasToyObject()) {
            //There is no ToyObject
            if (player.HasToyObject()) {
                //Player is carrying Toy Object
                if (HasRecipeWithInput(player.GetToyObject().GetToyObjectSO())) {
                    //Player carrying somthing that can be Melted
                    player.GetToyObject().SetToyObjectParent(this);
                    meltingProgress = 0;

                    MeltingRecipeSO meltingRecipeSO = GetMeltingRecipeSOWithInput(GetToyObject().GetToyObjectSO());

                    OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs {
                        progressNormalized = (float)meltingProgress / meltingRecipeSO.meltingProgressMax
                    });
                }
            } else {
                //Player not carring anything
            }
        } else {
            //There is ToyObject here
            if (player.HasToyObject()) {
                //player is carrying something
            } else {
                //Player is not carrying anything
                GetToyObject().SetToyObjectParent(player);


            }
        }
    }

    public override void InteractAlternate(Player player) {
        if (HasToyObject() && HasRecipeWithInput(GetToyObject().GetToyObjectSO())) {
            //There is a Toy Object Melt it
            meltingProgress++;
            MeltingRecipeSO meltingRecipeSO = GetMeltingRecipeSOWithInput(GetToyObject().GetToyObjectSO());

            OnProgressChanged?.Invoke(this, new OnProgressChangedEventArgs {
                progressNormalized = (float)meltingProgress / meltingRecipeSO.meltingProgressMax
            });
            if (meltingProgress >= meltingRecipeSO.meltingProgressMax) {
                ToyObjectSO outputToyObjectSO = GetOutputForInput(GetToyObject().GetToyObjectSO());
                GetToyObject().DestroySelf();

                ToyObject.SpawnToyObject(outputToyObjectSO, this);
            }

        }
    }

    private ToyObjectSO GetOutputForInput(ToyObjectSO inputToyObjectSO) {
        MeltingRecipeSO meltingRecipeSO = GetMeltingRecipeSOWithInput(inputToyObjectSO);
        if (meltingRecipeSO != null) {
            return meltingRecipeSO.output;
        } else {
            return null;
        }
    }
    private bool HasRecipeWithInput(ToyObjectSO inputToyObjectSO) {
        MeltingRecipeSO meltingRecipeSO = GetMeltingRecipeSOWithInput(inputToyObjectSO);
        return meltingRecipeSO != null;
    }

    private MeltingRecipeSO GetMeltingRecipeSOWithInput(ToyObjectSO inputToyObjectSO) {
        foreach (MeltingRecipeSO meltingRecipeSO in meltingRecipeSOArray) {
            if (meltingRecipeSO.input == inputToyObjectSO) {
                return meltingRecipeSO;
            }
        }
        return null;
    }
}
