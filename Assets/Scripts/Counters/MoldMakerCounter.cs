using System;
using UnityEngine;

public class MoldMakerCounter : BaseCounter, IHasProgress {

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler<OnMoldingStateChangedEventArgs> OnMoldingStateChanged;
    public class OnMoldingStateChangedEventArgs : EventArgs {
        public State state;
    }

    public enum State {
        Idle,
        ChoosingRecipe,
        Molding,
        Molded,
    }

    [SerializeField] private MoldingRecipeSO[] moldingRecipeSOArray;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private ParticleSystem particleSystem;

    private State state;
    private float moldingTimer;
    private MoldingRecipeSO activeRecipeSO;
    private ToyObjectSO selectedOutputSO;

    public override Transform GetToyObjectFollowTransform() {
        if (state == State.Molded && endPoint != null) {
            return endPoint;
        }
        if (startPoint != null) {
            return startPoint;
        }
        return base.GetToyObjectFollowTransform();
    }

    private void Start() {
        SetState(State.Idle);
        if (particleSystem != null) {
            particleSystem.Stop();
        }
    }

    private void Update() {
        if (HasToyObject()) {
            switch (state) {
                case State.Idle:
                    break;

                case State.ChoosingRecipe:
                    // Waiting for player to select an option from UI menu
                    break;

                case State.Molding:
                    moldingTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = moldingTimer / activeRecipeSO.moldingTimerMax
                    });

                    if (moldingTimer >= activeRecipeSO.moldingTimerMax) {
                        // Molding finished
                        moldingTimer = 0f;
                        GetToyObject().DestroySelf();

                        SetState(State.Molded);
                        ToyObject.SpawnToyObject(selectedOutputSO, this);

                        if (particleSystem != null) {
                            particleSystem.Stop();
                        }

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                            progressNormalized = 0f
                        });
                    }
                    break;

                case State.Molded:
                    break;
            }
        }
    }

    public override void Interact(Player player) {
        if (!HasToyObject()) {
            // There is no ToyObject on counter
            if (player.HasToyObject()) {
                ToyObjectSO carriedToyObjectSO = player.GetToyObject().GetToyObjectSO();
                if (HasRecipeWithInput(carriedToyObjectSO)) {
                    // Player is carrying a valid moldable item (e.g. PlasticHolder or MetalHolder)
                    player.GetToyObject().SetToyObjectParent(this);

                    activeRecipeSO = GetMoldingRecipeSOWithInput(carriedToyObjectSO);
                    moldingTimer = 0f;

                    // Automatically open menu for player to choose toy
                    SetState(State.ChoosingRecipe);

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = 0f
                    });
                }
            }
        } else {
            // Counter already has a ToyObject
            if (player.HasToyObject()) {
                // Player is carrying something: cannot drop or combine
            } else {
                // If it is in ChoosingRecipe state, pressing interact again can reopen menu if closed
                if (state == State.ChoosingRecipe) {
                    // Notify UI to re-open/show menu if player interacted
                    OnMoldingStateChanged?.Invoke(this, new OnMoldingStateChangedEventArgs {
                        state = state
                    });
                    return;
                }

                if (state == State.Molding) {
                    // While molding is in progress, cannot take it out
                    return;
                }

                // Player can pick up the molded toy or raw input
                GetToyObject().SetToyObjectParent(player);
                ResetCounter();
            }
        }
    }

    public void SelectOutput(ToyObjectSO chosenOutputSO) {
        if (state != State.ChoosingRecipe) {
            return;
        }

        selectedOutputSO = chosenOutputSO;
        moldingTimer = 0f;
        SetState(State.Molding);

        if (particleSystem != null) {
            particleSystem.Play();
        }

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
            progressNormalized = 0f
        });
    }

    public void CancelSelection() {
        if (state == State.ChoosingRecipe) {
            // Player closed menu without selecting
            // Remains in ChoosingRecipe or can be interacted with
        }
    }

    private void SetState(State newState) {
        state = newState;
        OnMoldingStateChanged?.Invoke(this, new OnMoldingStateChangedEventArgs {
            state = state
        });
    }

    public void ResetCounter() {
        SetState(State.Idle);
        moldingTimer = 0f;
        activeRecipeSO = null;
        selectedOutputSO = null;

        if (particleSystem != null) {
            particleSystem.Stop();
        }

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
            progressNormalized = 0f
        });
    }

    public State GetState() {
        return state;
    }

    public MoldingRecipeSO GetActiveRecipeSO() {
        return activeRecipeSO;
    }

    public bool HasRecipeWithInput(ToyObjectSO inputToyObjectSO) {
        return GetMoldingRecipeSOWithInput(inputToyObjectSO) != null;
    }

    public MoldingRecipeSO GetMoldingRecipeSOWithInput(ToyObjectSO inputToyObjectSO) {
        if (moldingRecipeSOArray == null || inputToyObjectSO == null) return null;

        foreach (MoldingRecipeSO recipeSO in moldingRecipeSOArray) {
            if (recipeSO != null && recipeSO.input == inputToyObjectSO) {
                return recipeSO;
            }
        }
        return null;
    }
}
