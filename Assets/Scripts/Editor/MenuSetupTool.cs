using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Tool tạo Main Menu Canvas với logo, tiêu đề, nút Play + Highscore, và vật thể trôi nổi.
/// Sau khi chạy, dùng "Tools/Setup Highscore Panel" để thêm bảng điểm cao.
/// </summary>
public class MenuSetupTool : EditorWindow
{
    [MenuItem("Tools/Setup Main Menu")]
    public static void CreateMainMenu()
    {
        // 1. Create Canvas
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Create Event System
        if (Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // ============================================================
        // 3. Create MAIN MENU PANEL (logo + title + buttons)
        // ============================================================
        GameObject mainPanel = new GameObject("MainMenuPanel");
        mainPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform mainPanelRect = mainPanel.AddComponent<RectTransform>();
        mainPanelRect.anchorMin = Vector2.zero;
        mainPanelRect.anchorMax = Vector2.one;
        mainPanelRect.sizeDelta = Vector2.zero;

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(mainPanel.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.02f, 0.02f, 0.05f, 1f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Logo Placeholder
        GameObject logoObj = new GameObject("LogoImage");
        logoObj.transform.SetParent(mainPanel.transform, false);
        Image logoImage = logoObj.AddComponent<Image>();
        logoImage.color = new Color(1f, 1f, 1f, 0.2f);
        RectTransform logoRect = logoObj.GetComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.5f, 0.8f);
        logoRect.anchorMax = new Vector2(0.5f, 0.8f);
        logoRect.sizeDelta = new Vector2(250, 200);

        // Title
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(mainPanel.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Space Adventure";
        titleText.fontSize = 110;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.6f);
        titleRect.anchorMax = new Vector2(0.5f, 0.6f);
        titleRect.sizeDelta = new Vector2(1200, 200);

        // Button Group
        GameObject buttonGroupObj = new GameObject("ButtonGroup");
        buttonGroupObj.transform.SetParent(mainPanel.transform, false);
        VerticalLayoutGroup vlg = buttonGroupObj.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 20;
        vlg.childControlHeight = false;
        vlg.childControlWidth = false;
        RectTransform groupRect = buttonGroupObj.GetComponent<RectTransform>();
        groupRect.anchorMin = new Vector2(0.5f, 0.35f);
        groupRect.anchorMax = new Vector2(0.5f, 0.35f);
        groupRect.sizeDelta = new Vector2(400, 300);

        Button playBtn = CreateMenuButton("PlayButton", "Play", buttonGroupObj.transform);
        Button highscoreBtn = CreateMenuButton("HighscoreButton", "Highscore", buttonGroupObj.transform);

        // ============================================================
        // 4. Wire up MainMenuUI component
        // ============================================================
        MainMenuUI menuUI = canvasObj.AddComponent<MainMenuUI>();
        SerializedObject menuSo = new SerializedObject(menuUI);
        menuSo.FindProperty("playButton").objectReferenceValue = playBtn;
        menuSo.FindProperty("highscoreButton").objectReferenceValue = highscoreBtn;
        menuSo.FindProperty("mainMenuPanel").objectReferenceValue = mainPanel;
        menuSo.ApplyModifiedProperties();

        // ============================================================
        // 5. Add MenuDrifterSpawner
        // ============================================================
        MenuDrifterSpawner spawner = canvasObj.AddComponent<MenuDrifterSpawner>();
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        System.Collections.Generic.List<GameObject> foundPrefabs = new System.Collections.Generic.List<GameObject>();
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string lowerPath = path.ToLower();
            if (lowerPath.Contains("meteor") || lowerPath.Contains("asteroid") || lowerPath.Contains("enemy"))
            {
                GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (p != null) foundPrefabs.Add(p);
            }
        }
        if (foundPrefabs.Count > 0)
        {
            SerializedObject spawnerSo = new SerializedObject(spawner);
            SerializedProperty prefabsProp = spawnerSo.FindProperty("prefabsToSpawn");
            prefabsProp.arraySize = foundPrefabs.Count;
            for (int i = 0; i < foundPrefabs.Count; i++)
                prefabsProp.GetArrayElementAtIndex(i).objectReferenceValue = foundPrefabs[i];
            spawnerSo.ApplyModifiedProperties();
            Debug.Log($"[MenuSetupTool] Tìm thấy {foundPrefabs.Count} prefab trôi nổi.");
        }

        Selection.activeGameObject = canvasObj;
        Debug.Log("[MenuSetupTool] Main Menu tạo xong! Tiếp theo chạy 'Tools/Setup Highscore Panel'.");
    }

    // Button tím đậm chuẩn style
    public static Button CreateMenuButton(string name, string textStr, Transform parent)
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
