using UnityEngine;
using UnityEngine.UI;
public class ProgressBarUI : MonoBehaviour {
    [SerializeField] private MelterCounter melterCounter;
    [SerializeField] private Image barImage;

    private void Start() {
        melterCounter.OnProgressChanged += MelterCounter_OnProgressChanged;

        barImage.fillAmount = 0;
        Hide();
    }

    private void MelterCounter_OnProgressChanged(object sender, MelterCounter.OnProgressChangedEventArgs e) {
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
