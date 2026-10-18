
using UnityEngine;

public class SelectCounterVisual : MonoBehaviour {
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjectArray;

    private void Awake() {
        if (baseCounter == null) {
            baseCounter = GetComponentInParent<BaseCounter>();
        }
    }

    private void Start() {
        if (baseCounter == null) {
            baseCounter = GetComponentInParent<BaseCounter>();
        }

        Player.Instance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
    }

    //private void OnDestroy() {
    //    if (Player.Instance != null) {
    //        Player.Instance.OnSelectedCounterChanged -=
    //            Player_OnSelectedCounterChanged;
    //    }
    //}

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e) {
        if (e.selectedCounter == baseCounter) {
            Show();
        } else {
            Hide();
        }
    }
    private void Show() {
        foreach (GameObject visualGame in visualGameObjectArray) {
            visualGame.SetActive(true);
        }
    }

    private void Hide() {
        foreach (GameObject visualGame in visualGameObjectArray) {
            visualGame.SetActive(false);
        }
    }
}
