using System;
using UnityEngine;

public class WoodCounter : BaseCounter, IHasProgress {

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    private enum State {
        Idle,
        Cutting,
        Cutted,
        Bushed,
    }

    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private ParticleSystem particleSystem;

    private State state;
    private float cuttingTimer;
    private CuttingRecipeSO cuttingRecipeSO;

    public override Transform GetToyObjectFollowTransform() {
        if (state == State.Cutted && endPoint != null) {
            return endPoint;
        }
        if (startPoint != null) {
            return startPoint;
        }
        return base.GetToyObjectFollowTransform();
    }

    private void Start() {
        state = State.Idle;
        if (particleSystem != null) {
            particleSystem.Stop();
        }
    }
    private void Update() {
        if (HasToyObject()) {
            switch (state) {
                case State.Idle:

                    break;

                case State.Cutting:
                    cuttingTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = cuttingTimer / cuttingRecipeSO.cuttingTimerMax
                    });

                    if (cuttingTimer > cuttingRecipeSO.cuttingTimerMax) {
                        //Cutting finished
                        cuttingTimer = 0f;
                        GetToyObject().DestroySelf();

                        state = State.Cutted;
                        ToyObject.SpawnToyObject(cuttingRecipeSO.output, this);

                        if (particleSystem != null) {
                            particleSystem.Stop();
                        }

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                            progressNormalized = 0f
                        });
                    }
                    break;

                case State.Cutted:

                    break;

                case State.Bushed:

                    break;

            }
            Debug.Log(state);
        }


    }

    public override void Interact(Player player) {
        if (!HasToyObject()) {
            //There is no ToyObject
            if (player.HasToyObject()) {
                //Player is carrying Toy Object
                if (HasRecipeWithInput(player.GetToyObject().GetToyObjectSO())) {
                    //Player carrying something that can be cut
                    player.GetToyObject().SetToyObjectParent(this);

                    cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetToyObject().GetToyObjectSO());
                    state = State.Cutting;
                    cuttingTimer = 0f;

                    if (particleSystem != null) {
                        particleSystem.Play();
                    }

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = 0f
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

                state = State.Idle;

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
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputToyObjectSO);
        if (cuttingRecipeSO != null) {
            return cuttingRecipeSO.output;
        } else {
            return null;
        }
    }
    private bool HasRecipeWithInput(ToyObjectSO inputToyObjectSO) {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputToyObjectSO);
        return cuttingRecipeSO != null;
    }

    private CuttingRecipeSO GetCuttingRecipeSOWithInput(ToyObjectSO inputToyObjectSO) {
        foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOArray) {
            if (cuttingRecipeSO.input == inputToyObjectSO) {
                return cuttingRecipeSO;
            }
        }
        return null;
    }
}
