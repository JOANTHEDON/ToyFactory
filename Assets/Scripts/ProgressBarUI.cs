using UnityEngine;
using UnityEngine.UI;
public class ProgressBarUI : MonoBehaviour {
    [SerializeField] private GameObject hasProgressGameObject;
    [SerializeField] private MelterCounter melterCounter;
    [SerializeField] private Image barImage;

    private IHasProgress hasProgress;

    private void Start() {
        if (hasProgressGameObject != null) {
            hasProgress = hasProgressGameObject.GetComponent<IHasProgress>();
        } else if (melterCounter != null) {
            hasProgress = melterCounter;
        }

        if (hasProgress == null) {
            Debug.LogError("Game Object " + gameObject + " does not have a component that implements IHasProgress!");
            return;
        }

        hasProgress.OnProgressChanged += HasProgress_OnProgressChanged;

        barImage.fillAmount = 0;
        Hide();
    }

    private void HasProgress_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e) {
        barImage.fillAmount = e.progressNormalized;
        if (e.progressNormalized == 0f || e.progressNormalized == 1f) {
            Hide();
        } else {
            Show();
        }
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
