using UnityEngine;
public class WoodCounter : BaseCounter {
    private enum State {
        Idle,
        Cutting,
        Cutted,
        Bushed,
    }

    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    private State state;
    private float cuttingTimer;
    private CuttingRecipeSO cuttingRecipeSO;

    private void Start() {
        state = State.Idle;
    }
    private void Update() {
        if (HasToyObject()) {
            switch (state) {
                case State.Idle:

                    break;

                case State.Cutting:
                    cuttingTimer += Time.deltaTime;

                    if (cuttingTimer > cuttingRecipeSO.cuttingTimerMax) {
                        //Fried 
                        cuttingTimer = 0f;
                        GetToyObject().DestroySelf();

                        ToyObject.SpawnToyObject(cuttingRecipeSO.output, this);
                        state = State.Cutted;
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
                    //Player carrying somthing that can be Melted
                    player.GetToyObject().SetToyObjectParent(this);

                    cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetToyObject().GetToyObjectSO());
                    state = State.Cutting;
                    cuttingTimer = 0f;

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
