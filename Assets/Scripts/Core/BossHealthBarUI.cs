using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Boss HP Bar - Thanh máu boss hiển thị ở top màn hình
/// Đổi màu theo phase: Đỏ → Cam → Vàng
/// Gắn vào BossHPBarCanvas
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image  hpBarFill;       // Image với FillMethod = Horizontal
    [SerializeField] private Image  hpBarBackground;
    [SerializeField] private Text   bossNameText;    // "⚔ C7 — Ưng Thép"
    [SerializeField] private Text   phaseText;       // "Phase I / II / III"
    [SerializeField] private Text   hpValueText;     // "1500 / 1500"

    [Header("Colors by Phase")]
    [SerializeField] private Color phase1Color = new Color(0.9f, 0.1f, 0.1f); // Đỏ
    [SerializeField] private Color phase2Color = new Color(1f,   0.5f, 0f);   // Cam
    [SerializeField] private Color phase3Color = new Color(1f,   0.9f, 0f);   // Vàng

    [Header("Animation")]
    [SerializeField] private float smoothSpeed = 5f;

    private CanvasGroup canvasGroup;
    private float targetFill = 1f;
    private float currentFill = 1f;
    private BossController trackedBoss;

    private static readonly string[] PhaseLabels = { "", "Phase I", "Phase II", "Phase III ⚠" };

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        SetVisible(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        // Smooth HP bar animation
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed);
        if (hpBarFill != null)
            hpBarFill.fillAmount = currentFill;
    }

    /// <summary>
    /// Gọi khi boss spawn - bắt đầu track boss
    /// </summary>
    public void TrackBoss(BossController boss, string bossDisplayName)
    {
        if (boss == null) return;

        // Unsubscribe từ boss cũ nếu có
        UntrackCurrent();

        trackedBoss = boss;
        trackedBoss.OnHealthChanged += HandleHealthChanged;
        trackedBoss.OnPhaseChanged  += HandlePhaseChanged;
        trackedBoss.OnDeath         += HandleBossDeath;

        if (bossNameText != null)
            bossNameText.text = "⚔  " + bossDisplayName;

        currentFill = 1f;
        targetFill  = 1f;
        HandlePhaseChanged(1);

        SetVisible(true);
        StartCoroutine(SlideInRoutine());
    }

    private void HandleHealthChanged(float current, float max)
    {
        targetFill = max > 0f ? current / max : 0f;

        if (hpValueText != null)
            hpValueText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    private void HandlePhaseChanged(int phase)
    {
        if (phase < 1 || phase > 3) return;

        Color targetColor = phase == 1 ? phase1Color
                          : phase == 2 ? phase2Color
                          : phase3Color;

        if (hpBarFill != null)
            hpBarFill.color = targetColor;

        if (phaseText != null)
            phaseText.text = phase < PhaseLabels.Length ? PhaseLabels[phase] : "";
    }

    private void HandleBossDeath()
    {
        StartCoroutine(SlideOutRoutine());
    }

    private void UntrackCurrent()
    {
        if (trackedBoss == null) return;
        trackedBoss.OnHealthChanged -= HandleHealthChanged;
        trackedBoss.OnPhaseChanged  -= HandlePhaseChanged;
        trackedBoss.OnDeath         -= HandleBossDeath;
        trackedBoss = null;
    }

    // ───────── Slide Animations ─────────
    private IEnumerator SlideInRoutine()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) yield break;

        Vector2 startPos = rt.anchoredPosition + Vector2.up * 80f;
        Vector2 endPos   = rt.anchoredPosition;
        rt.anchoredPosition = startPos;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        rt.anchoredPosition = endPos;
    }

    private IEnumerator SlideOutRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) { SetVisible(false); yield break; }

        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos   = startPos + Vector2.up * 80f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2.5f;
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, t));
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        SetVisible(false);
        UntrackCurrent();
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha          = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable   = false;
    }

    private void OnDestroy() => UntrackCurrent();
}
