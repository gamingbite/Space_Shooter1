using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tool tạo Highscore Panel và gắn vào MainMenuCanvas có sẵn trong Scene.
/// Chạy sau khi đã chạy "Tools/Setup Main Menu".
/// </summary>
public class HighscoreSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Highscore Panel")]
    public static void CreateHighscorePanel()
    {
        // Tìm MainMenuCanvas trong Scene
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[HighscoreSetupTool] Không tìm thấy Canvas! Hãy chạy 'Tools/Setup Main Menu' trước.");
            return;
        }
        GameObject canvasObj = canvas.gameObject;

        // Kiểm tra nếu HighscorePanel đã tồn tại thì xóa đi để tạo mới
        Transform existing = canvasObj.transform.Find("HighscorePanel");
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
            Debug.Log("[HighscoreSetupTool] Đã xóa HighscorePanel cũ.");
        }

        // Tìm MainMenuUI để wire references
        MainMenuUI menuUI = canvasObj.GetComponent<MainMenuUI>();
        if (menuUI == null)
        {
            Debug.LogError("[HighscoreSetupTool] Không tìm thấy MainMenuUI trên Canvas! Hãy chạy 'Tools/Setup Main Menu' trước.");
            return;
        }

        // Tìm nút Highscore trong scene
        Button highscoreBtn = null;
        Transform btnGroup = canvasObj.transform.Find("MainMenuPanel/ButtonGroup/HighscoreButton");
        if (btnGroup != null) highscoreBtn = btnGroup.GetComponent<Button>();

        // ============================================================
        // Tạo HighscorePanel
        // ============================================================
        GameObject hsPanel = new GameObject("HighscorePanel");
        hsPanel.transform.SetParent(canvasObj.transform, false);
        Image hsBg = hsPanel.AddComponent<Image>();
        hsBg.color = new Color(0.02f, 0.02f, 0.05f, 1f);
        RectTransform hsPanelRect = hsPanel.GetComponent<RectTransform>();
        hsPanelRect.anchorMin = Vector2.zero;
        hsPanelRect.anchorMax = Vector2.one;
        hsPanelRect.sizeDelta = Vector2.zero;

        // Content (Vertical Layout)
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(hsPanel.transform, false);
        VerticalLayoutGroup contentVlg = contentObj.AddComponent<VerticalLayoutGroup>();
        contentVlg.childAlignment = TextAnchor.MiddleCenter;
        contentVlg.spacing = 40;
        contentVlg.childControlHeight = false;
        contentVlg.childControlWidth = false;
        contentVlg.padding = new RectOffset(0, 0, 0, 0);
        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(700, 600);

        // Row: Enemy icon + count
        TextMeshProUGUI enemyText = CreateIconRow("EnemyRow", "EnemyIcon", "x0", contentObj.transform);

        // Row: Meteor icon + count
        TextMeshProUGUI meteorText = CreateIconRow("MeteorRow", "MeteorIcon", "x0", contentObj.transform);

        // Waves Survived text
        GameObject waveObj = new GameObject("WaveText");
        waveObj.transform.SetParent(contentObj.transform, false);
        TextMeshProUGUI waveText = waveObj.AddComponent<TextMeshProUGUI>();
        waveText.text = "Waves Survived: 0";
        waveText.fontSize = 55;
        waveText.fontStyle = FontStyles.Bold;
        waveText.alignment = TextAlignmentOptions.Center;
        waveText.color = Color.white;
        waveObj.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 80);

        // Spacer
        GameObject spacer = new GameObject("Spacer");
        spacer.transform.SetParent(contentObj.transform, false);
        spacer.AddComponent<LayoutElement>().minHeight = 20;

        // Main Menu button
        Button mainMenuBtn = CreateMenuButton("MainMenuButton", "Main Menu", contentObj.transform);
        mainMenuBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 70);

        // Panel starts hidden
        hsPanel.SetActive(false);

        // ============================================================
        // Wire up HighscoreUI component
        // ============================================================
        HighscoreUI hsUI = hsPanel.AddComponent<HighscoreUI>();
        SerializedObject hsSo = new SerializedObject(hsUI);
        hsSo.FindProperty("enemyKillText").objectReferenceValue = enemyText;
        hsSo.FindProperty("meteorKillText").objectReferenceValue = meteorText;
        hsSo.FindProperty("waveText").objectReferenceValue = waveText;
        hsSo.FindProperty("mainMenuButton").objectReferenceValue = mainMenuBtn;
        hsSo.ApplyModifiedProperties();

        // Wire highscorePanel vào MainMenuUI
        SerializedObject menuSo = new SerializedObject(menuUI);
        menuSo.FindProperty("highscorePanel").objectReferenceValue = hsUI;
        menuSo.ApplyModifiedProperties();

        // Wire nút "Main Menu" -> MainMenuUI.ShowMainPanel
        UnityEventTools.AddPersistentListener(mainMenuBtn.onClick, menuUI.ShowMainPanel);

        // Wire nút "Highscore" -> MainMenuUI.OnHighscoreClicked (nếu chưa có)
        if (highscoreBtn != null)
        {
            // Xóa listener cũ nếu có và thêm lại
            highscoreBtn.onClick.RemoveAllListeners();
            UnityEventTools.AddPersistentListener(highscoreBtn.onClick, menuUI.OnHighscoreClicked);
        }

        Selection.activeGameObject = hsPanel;
        Debug.Log("[HighscoreSetupTool] Highscore Panel tạo thành công và đã gắn vào Canvas!");
    }

    // Tạo một hàng: icon placeholder + số đếm
    private static TextMeshProUGUI CreateIconRow(string rowName, string iconName, string defaultText, Transform parent)
    {
        GameObject rowObj = new GameObject(rowName);
        rowObj.transform.SetParent(parent, false);
        HorizontalLayoutGroup hlg = rowObj.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 30;
        hlg.childControlHeight = false;
        hlg.childControlWidth = false;
        rowObj.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 130);

        // Icon placeholder
        GameObject iconObj = new GameObject(iconName);
        iconObj.transform.SetParent(rowObj.transform, false);
        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        iconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(110, 110);

        // Text
        GameObject textObj = new GameObject("CountText");
        textObj.transform.SetParent(rowObj.transform, false);
        TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
        txt.text = defaultText;
        txt.fontSize = 80;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Left;
        txt.color = Color.white;
        textObj.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 110);

        return txt;
    }

    // Button tím đậm chuẩn style
    private static Button CreateMenuButton(string name, string textStr, Transform parent)
    {
        Color buttonColor = new Color(0.24f, 0.13f, 0.44f, 1f);

        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = buttonColor;

        Button btn = buttonObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = buttonColor;
        cb.highlightedColor = new Color(0.35f, 0.22f, 0.58f, 1f);
        cb.pressedColor = new Color(0.15f, 0.08f, 0.30f, 1f);
        cb.selectedColor = buttonColor;
        btn.colors = cb;

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(250, 60);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = textStr;
        btnText.fontSize = 35;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return btn;
    }
}
