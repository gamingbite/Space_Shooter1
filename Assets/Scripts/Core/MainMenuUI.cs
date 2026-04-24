using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button highscoreButton;
    [SerializeField] private HighscoreUI highscorePanel;

    // Panel chứa logo/title/buttons của menu chính
    [SerializeField] private GameObject mainMenuPanel;

    private void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (highscoreButton != null)
            highscoreButton.onClick.AddListener(OnHighscoreClicked);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            HandleStateChanged(GameManager.Instance.CurrentState);
        }
    }

    private void OnPlayClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    public void OnHighscoreClicked()
    {
        Debug.Log($"[MainMenuUI] OnHighscoreClicked - mainMenuPanel={mainMenuPanel}, highscorePanel={highscorePanel}");

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        else
            Debug.LogWarning("[MainMenuUI] mainMenuPanel is NULL!");

        if (highscorePanel != null)
            highscorePanel.Show();
        else
            Debug.LogWarning("[MainMenuUI] highscorePanel is NULL! Hãy chạy lại 'Tools/Setup Highscore Panel'.");
    }

    // Được gọi bởi HighscoreUI khi bấm "Main Menu"
    public void ShowMainPanel()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (highscorePanel != null)
            highscorePanel.Hide();
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        bool isMenu = state == GameManager.GameState.Menu;
        gameObject.SetActive(isMenu);

        // Khi quay về Menu state, luôn hiện main panel, ẩn highscore
        if (isMenu)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (highscorePanel != null) highscorePanel.Hide();
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }
}
