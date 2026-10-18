using System;
using UnityEngine;
public class MelterCounter : BaseCounter, IHasProgress {

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    [SerializeField] private MeltingRecipeSO[] meltingRecipeSOArray;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private ParticleSystem particleSystem;

    private int meltingProgress;
    private bool isMelted;

    public override Transform GetToyObjectFollowTransform() {
        if (isMelted && endPoint != null) {
            return endPoint;
        }
        if (startPoint != null) {
            return startPoint;
        }
        return base.GetToyObjectFollowTransform();
    }

    private void Start() {
        if (particleSystem != null) {
            particleSystem.Stop();
        }
    }
    public override void Interact(Player player) {
        if (!HasToyObject()) {
            //There is no ToyObject
            if (player.HasToyObject()) {
                //Player is carrying Toy Object
                if (HasRecipeWithInput(player.GetToyObject().GetToyObjectSO())) {
                    //Player carrying something that can be Melted
                    isMelted = false;
                    player.GetToyObject().SetToyObjectParent(this);
                    meltingProgress = 0;

                    MeltingRecipeSO meltingRecipeSO = GetMeltingRecipeSOWithInput(GetToyObject().GetToyObjectSO());

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
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
                if (isMelted) {
                    MeltingRecipeSO meltingRecipeSO = GetMeltingRecipeSOWithOutput(GetToyObject().GetToyObjectSO());
                    if (meltingRecipeSO != null && meltingRecipeSO.holderOutput != null) {
                        GetToyObject().DestroySelf();
                        ToyObject.SpawnToyObject(meltingRecipeSO.holderOutput, player);
                    } else {
                        GetToyObject().SetToyObjectParent(player);
                    }
                } else {
                    GetToyObject().SetToyObjectParent(player);
                }

                isMelted = false;

                if (particleSystem != null) {
                    particleSystem.Stop();
                }

                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                    progressNormalized = 0f
                });
            }
        }
    }

    public override void InteractAlternate(Player player) {
        if (HasToyObject() && HasRecipeWithInput(GetToyObject().GetToyObjectSO())) {
            //There is a Toy Object Melt it
            meltingProgress++;
            MeltingRecipeSO meltingRecipeSO = GetMeltingRecipeSOWithInput(GetToyObject().GetToyObjectSO());

            if (particleSystem != null) {
                particleSystem.Play();
            }

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                progressNormalized = (float)meltingProgress / meltingRecipeSO.meltingProgressMax
            });
            if (meltingProgress >= meltingRecipeSO.meltingProgressMax) {
                ToyObjectSO outputToyObjectSO = GetOutputForInput(GetToyObject().GetToyObjectSO());
                GetToyObject().DestroySelf();

                isMelted = true;
                ToyObject.SpawnToyObject(outputToyObjectSO, this);

                if (particleSystem != null) {
                    particleSystem.Stop();
                }

                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                    progressNormalized = 0f
                });
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

    private MeltingRecipeSO GetMeltingRecipeSOWithOutput(ToyObjectSO outputToyObjectSO) {
        foreach (MeltingRecipeSO meltingRecipeSO in meltingRecipeSOArray) {
            if (meltingRecipeSO.output == outputToyObjectSO) {
                return meltingRecipeSO;
            }
        }
        return null;
    }
}
