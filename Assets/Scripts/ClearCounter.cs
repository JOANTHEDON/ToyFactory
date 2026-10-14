using UnityEngine;

public class ClearCounter : BaseCounter {

    [SerializeField] private ToyObjectSO toyObjectSO;
    public override void Interact(Player player) {
        if (!HasToyObject()) {
            //There is no ToyObject
            if (player.HasToyObject()) {
                //Player is carrying Toy Object
                player.GetToyObject().SetToyObjectParent(this);
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


}
