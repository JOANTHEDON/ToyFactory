using System;
using UnityEngine;

public class ColorCounter : BaseCounter, IHasProgress {

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    private enum State {
        Idle,
        Coloring,
        Colored,
    }

    [SerializeField] private float coloringTimerMax = 4f;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private new ParticleSystem particleSystem;

    private State state;
    private float coloringTimer;

    public override Transform GetToyObjectFollowTransform() {
        if (state == State.Colored && endPoint != null) {
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

                case State.Coloring:
                    coloringTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = coloringTimer / coloringTimerMax
                    });

                    if (coloringTimer >= coloringTimerMax) {
                        // Coloring finished
                        coloringTimer = 0f;

                        // Attach original material to the toy object
                        GetToyObject().ApplyOriginalMaterial();

                        state = State.Colored;

                        // Reparent to endPoint if available
                        if (endPoint != null) {
                            GetToyObject().transform.parent = endPoint;
                            GetToyObject().transform.localPosition = Vector3.zero;
                        }

                        if (particleSystem != null) {
                            particleSystem.Stop();
                        }

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                            progressNormalized = 0f
                        });
                    }
                    break;

                case State.Colored:
                    break;
            }
        }
    }

    public override void Interact(Player player) {
        if (!HasToyObject()) {
            // There is no ToyObject on counter
            if (player.HasToyObject()) {
                ToyObject playerToyObject = player.GetToyObject();
                // Check if the toy object can be colored
                if (CanColor(playerToyObject)) {
                    playerToyObject.SetToyObjectParent(this);

                    state = State.Coloring;
                    coloringTimer = 0f;

                    if (particleSystem != null) {
                        particleSystem.Play();
                    }

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = 0f
                    });
                }
            }
        } else {
            // There is a ToyObject on counter
            if (player.HasToyObject()) {
                // Player is carrying something: cannot pick up
            } else {
                if (state == State.Coloring) {
                    // Wait for the paint to dry
                    return;
                }
                // Player picks up the ToyObject
                GetToyObject().SetToyObjectParent(player);

                state = State.Idle;
                coloringTimer = 0f;

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
            if (player.HasToyObject() && CanColor(player.GetToyObject())) {
                return "Paint " + ToyObjectSO.GetDisplayName(player.GetToyObject().GetToyObjectSO());
            }
            return null;
        }
        if (player.HasToyObject()) return null;
        if (state == State.Coloring) return null;
        return "Pick up " + ToyObjectSO.GetDisplayName(GetToyObject().GetToyObjectSO());
    }

    public override string GetBusyHint() {
        if (HasToyObject() && state == State.Coloring) return "Painting...";
        return null;
    }

    public override string GetStationName() {
        return "Paint Station";
    }

    public float GetColoringTimerMax() {
        return coloringTimerMax;
    }

    public bool CanColor(ToyObject toyObject) {
        return toyObject != null && toyObject.HasOriginalMaterial() && !toyObject.IsColored();
    }
}
