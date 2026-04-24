using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Hiển thị bảng Highscore khi bấm nút Highscore từ Main Menu
/// </summary>
public class HighscoreUI : MonoBehaviour
{
    [Header("Stats Text")]
    [SerializeField] private TextMeshProUGUI enemyKillText;
    [SerializeField] private TextMeshProUGUI meteorKillText;
    [SerializeField] private TextMeshProUGUI waveText;

    [Header("Buttons")]
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        // Dùng Awake thay vì Start để listener được đăng ký
        // ngay cả khi object bắt đầu inactive
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    private void Start()
    {
        // Panel đã được set inactive bởi MenuSetupTool,
        // KHÔNG gọi SetActive(false) ở đây vì sẽ tắt ngay sau khi Show() bật lên!
    }

    /// <summary>
    /// Gọi khi mở bảng highscore để refresh dữ liệu mới nhất
    /// </summary>
    public void Show()
    {
        RefreshData();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void RefreshData()
    {
        if (GameManager.Instance == null) return;

        if (enemyKillText != null)
            enemyKillText.text = "x" + GameManager.Instance.HighscoreEnemyKills;

        if (meteorKillText != null)
            meteorKillText.text = "x" + GameManager.Instance.HighscoreMeteorKills;

        if (waveText != null)
            waveText.text = "Waves Survived: " + GameManager.Instance.HighscoreWave;
    }

    private void OnMainMenuClicked()
    {
        Hide();
    }
}
