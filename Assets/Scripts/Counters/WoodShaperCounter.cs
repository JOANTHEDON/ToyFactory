using System;
using UnityEngine;

public class WoodShaperCounter : BaseCounter, IHasProgress {

    public static WoodShaperCounter Instance { get; private set; }

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler<OnShapingStateChangedEventArgs> OnShapingStateChanged;
    public class OnShapingStateChangedEventArgs : EventArgs {
        public State state;
    }

    public enum State {
        Idle,
        ChoosingRecipe,
        Shaping,
        Shaped,
    }

    [SerializeField] private MoldingRecipeSO[] shapingRecipeSOArray;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private new ParticleSystem particleSystem;

    private State state;
    private float shapingTimer;
    private MoldingRecipeSO activeRecipeSO;
    private ToyObjectSO selectedOutputSO;

    public override Transform GetToyObjectFollowTransform() {
        if (state == State.Shaped && endPoint != null) {
            return endPoint;
        }
        if (startPoint != null) {
            return startPoint;
        }
        return base.GetToyObjectFollowTransform();
    }

    private void Awake() {
        Instance = this;
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

                case State.Shaping:
                    shapingTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = shapingTimer / activeRecipeSO.moldingTimerMax
                    });

                    if (shapingTimer >= activeRecipeSO.moldingTimerMax) {
                        // Shaping finished
                        shapingTimer = 0f;
                        GetToyObject().DestroySelf();

                        SetState(State.Shaped);
                        ToyObject.SpawnToyObject(selectedOutputSO, this);

                        if (particleSystem != null) {
                            particleSystem.Stop();
                        }

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                            progressNormalized = 0f
                        });
                    }
                    break;

                case State.Shaped:
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
                    // Player is carrying a valid item to shape (CuttedWood)
                    player.GetToyObject().SetToyObjectParent(this);

                    activeRecipeSO = GetShapingRecipeSOWithInput(carriedToyObjectSO);
                    shapingTimer = 0f;

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
                    OnShapingStateChanged?.Invoke(this, new OnShapingStateChangedEventArgs {
                        state = state
                    });
                    return;
                }

                if (state == State.Shaping) {
                    // While shaping is in progress, cannot take it out
                    return;
                }

                // Player can pick up the shaped toy or raw input
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
        shapingTimer = 0f;
        SetState(State.Shaping);

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
        }
    }

    private void SetState(State newState) {
        state = newState;
        OnShapingStateChanged?.Invoke(this, new OnShapingStateChangedEventArgs {
            state = state
        });
    }

    public void ResetCounter() {
        SetState(State.Idle);
        shapingTimer = 0f;
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

    public override string GetInteractHint(Player player) {
        if (!HasToyObject()) {
            if (player.HasToyObject() && HasRecipeWithInput(player.GetToyObject().GetToyObjectSO())) {
                return "Load " + ToyObjectSO.GetDisplayName(player.GetToyObject().GetToyObjectSO());
            }
            return null;
        }
        if (player.HasToyObject()) return null;
        switch (state) {
            case State.ChoosingRecipe: return "Choose a toy";
            case State.Shaping: return null;
            default: return "Pick up " + ToyObjectSO.GetDisplayName(GetToyObject().GetToyObjectSO());
        }
    }

    public override string GetBusyHint() {
        if (HasToyObject() && state == State.Shaping) return "Shaping...";
        return null;
    }

    public override string GetStationName() {
        return "Wood Shaper";
    }

    public MoldingRecipeSO[] GetShapingRecipeSOArray() {
        return shapingRecipeSOArray;
    }

    public MoldingRecipeSO GetActiveRecipeSO() {
        return activeRecipeSO;
    }

    public bool HasRecipeWithInput(ToyObjectSO inputToyObjectSO) {
        return GetShapingRecipeSOWithInput(inputToyObjectSO) != null;
    }

    public MoldingRecipeSO GetShapingRecipeSOWithInput(ToyObjectSO inputToyObjectSO) {
        if (shapingRecipeSOArray == null || inputToyObjectSO == null) return null;

        foreach (MoldingRecipeSO recipeSO in shapingRecipeSOArray) {
            if (recipeSO != null && recipeSO.input == inputToyObjectSO) {
                return recipeSO;
            }
        }
        return null;
    }
}
