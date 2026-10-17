public class TrashCounter : BaseCounter {


    public override void Interact(Player player) {
        if (player.HasToyObject()) {
            player.GetToyObject().DestroySelf();
        }
    }

}
