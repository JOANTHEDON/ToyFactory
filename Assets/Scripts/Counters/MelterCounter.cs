using System;
using UnityEngine;
public class MelterCounter : BaseCounter, IHasProgress {

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public static event EventHandler OnAnyMeltHit;

    [SerializeField] private MeltingRecipeSO[] meltingRecipeSOArray;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private new ParticleSystem particleSystem;

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

            OnAnyMeltHit?.Invoke(this, EventArgs.Empty);

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

    public override string GetInteractHint(Player player) {
        if (!HasToyObject()) {
            if (player.HasToyObject() && HasRecipeWithInput(player.GetToyObject().GetToyObjectSO())) {
                return "Put " + ToyObjectSO.GetDisplayName(player.GetToyObject().GetToyObjectSO()) + " in melter";
            }
            return null;
        }
        if (player.HasToyObject()) return null;
        if (isMelted) {
            MeltingRecipeSO recipe = GetMeltingRecipeSOWithOutput(GetToyObject().GetToyObjectSO());
            if (recipe != null && recipe.holderOutput != null) {
                return "Scoop into " + ToyObjectSO.GetDisplayName(recipe.holderOutput);
            }
        }
        return "Pick up " + ToyObjectSO.GetDisplayName(GetToyObject().GetToyObjectSO());
    }

    public override string GetInteractAlternateHint(Player player) {
        if (!HasToyObject() || isMelted) return null;
        MeltingRecipeSO recipe = GetMeltingRecipeSOWithInput(GetToyObject().GetToyObjectSO());
        if (recipe == null) return null;
        return "Melt (" + meltingProgress + "/" + recipe.meltingProgressMax + ")";
    }

    public override string GetStationName() {
        return "Melter";
    }

    public MeltingRecipeSO[] GetMeltingRecipeSOArray() {
        return meltingRecipeSOArray;
    }

    public bool IsMelted() {
        return isMelted;
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
        if (meltingRecipeSOArray == null) return null;
        foreach (MeltingRecipeSO meltingRecipeSO in meltingRecipeSOArray) {
            if (meltingRecipeSO != null && meltingRecipeSO.input == inputToyObjectSO) {
                return meltingRecipeSO;
            }
        }
        return null;
    }

    private MeltingRecipeSO GetMeltingRecipeSOWithOutput(ToyObjectSO outputToyObjectSO) {
        if (meltingRecipeSOArray == null) return null;
        foreach (MeltingRecipeSO meltingRecipeSO in meltingRecipeSOArray) {
            if (meltingRecipeSO != null && meltingRecipeSO.output == outputToyObjectSO) {
                return meltingRecipeSO;
            }
        }
        return null;
    }

    public static void ResetStaticData() {
        OnAnyMeltHit = null;
    }
}
