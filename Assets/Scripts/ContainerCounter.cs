using System;
using UnityEngine;

public class ContainerCounter : BaseCounter {

    public event EventHandler OnPlayerGrabbedObject;

    [SerializeField]
    private ToyObjectSO toyObjectSO;
    public override void Interact(Player player) {
        if (!player.HasToyObject()) {
            //Player is not carrying anything
            Transform toyObjectTransform = Instantiate(toyObjectSO.prefab);
            toyObjectTransform.GetComponent<ToyObject>().SetToyObjectParent(player);

            OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
        }
    }

}



