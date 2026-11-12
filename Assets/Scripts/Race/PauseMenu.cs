using UnityEngine;
using UnityEngine.InputSystem;

// Esc pauses the race and shows the pause panel, Esc again resumes, as in pygame.
// The panel's Menu button calls RaceManager.BackToMenu, which also unfreezes time.
// The whole game stops while paused: physics, timers and the countdown all run on Time.timeScale.
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private InputActionReference pauseAction; // Game/Pause (Esc)
    [SerializeField] private GameObject pausePanel;

    private bool paused;

    private void OnEnable()
    {
        pauseAction.action.Enable();
        pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (!pauseAction.action.WasPressedThisFrame()) return;

        if (paused) Resume();
        else Pause();
    }

    public void Pause()
    {
        paused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        paused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }
}
