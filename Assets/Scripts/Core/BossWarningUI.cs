using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Boss Warning Screen - Flash đỏ + Text "WARNING" khi boss xuất hiện
/// Gắn vào một Canvas riêng (BossWarningCanvas) với SortingOrder cao
/// </summary>
public class BossWarningUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image  flashOverlay;     // Full-screen red overlay
    [SerializeField] private Text   warningText;      // "⚠ WARNING ⚠" text
    [SerializeField] private Text   bossNameText;     // "BOSS: C7 Ưng Thép"

    [Header("Flash Settings")]
    [SerializeField] private float flashDuration  = 0.18f;
    [SerializeField] private float flashCount     = 5f;
    [SerializeField] private Color flashColor     = new Color(1f, 0f, 0f, 0.55f);
    [SerializeField] private float totalDuration  = 3f;   // Bao lâu warning hiển thị

    [Header("Text Animation")]
    [SerializeField] private float textBlinkSpeed = 4f;

    private CanvasGroup canvasGroup;
    private bool isShowing = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Ẩn ngay từ đầu
        SetVisible(false);
    }

    /// <summary>
    /// Gọi từ WaveSpawner khi boss sắp xuất hiện
    /// </summary>
    public void ShowWarning(string bossName, System.Action onComplete = null)
    {
        if (isShowing) return;
        StartCoroutine(WarningSequence(bossName, onComplete));
    }

    private IEnumerator WarningSequence(string bossName, System.Action onComplete)
    {
        isShowing = true;
        SetVisible(true);

        // Thiết lập text
        if (bossNameText != null)
            bossNameText.text = "BOSS: " + bossName;

        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            // Flash overlay
            if (flashOverlay != null)
            {
                float flashCycle = Mathf.Sin(elapsed * flashCount * Mathf.PI) * 0.5f + 0.5f;
                Color c = flashColor;
                c.a = flashColor.a * flashCycle;
                flashOverlay.color = c;
            }

            // Blink WARNING text
            if (warningText != null)
            {
                float blink = Mathf.Sin(elapsed * textBlinkSpeed * Mathf.PI);
                warningText.color = blink > 0
                    ? new Color(1f, 0.9f, 0f, 1f)     // Vàng rực
                    : new Color(1f, 0.2f, 0.2f, 1f);  // Đỏ
            }

            yield return null;
        }

        // Fade out
        float fadeTime = 0.4f;
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeTime)
        {
            fadeElapsed += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = 1f - (fadeElapsed / fadeTime);
            yield return null;
        }

        SetVisible(false);
        isShowing = false;

        onComplete?.Invoke();
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha          = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable   = visible;
    }
}
