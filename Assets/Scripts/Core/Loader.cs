using UnityEngine.SceneManagement;

public static class Loader {

    public enum Scene {
        StartMenu,
        GameScene,
    }

    public static void Load(Scene scene) {
        GamePause.ResetAll();
        SceneManager.LoadScene(scene.ToString());
    }

    public static void ReloadCurrent() {
        GamePause.ResetAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
