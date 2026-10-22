using System;
using UnityEngine;

/// <summary>
/// Reference-counted pause. Any UI that wants the game frozen (pause menu, recipe modal) pushes a pause,
/// and pops it when closed. The game only resumes when every pause has been released.
/// </summary>
public static class GamePause {

    public static event Action<bool> OnPauseChanged;

    private static int pauseDepth;

    public static bool IsPaused => pauseDepth > 0;

    public static void Push() {
        pauseDepth++;
        if (pauseDepth == 1) {
            Time.timeScale = 0f;
            OnPauseChanged?.Invoke(true);
        }
    }

    public static void Pop() {
        if (pauseDepth <= 0) return;
        pauseDepth--;
        if (pauseDepth == 0) {
            Time.timeScale = 1f;
            OnPauseChanged?.Invoke(false);
        }
    }

    public static void ResetAll() {
        pauseDepth = 0;
        Time.timeScale = 1f;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        pauseDepth = 0;
        OnPauseChanged = null;
    }
}
