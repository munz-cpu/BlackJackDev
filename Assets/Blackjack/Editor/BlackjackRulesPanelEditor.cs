using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Adds the rules without rebuilding the existing blackjack scene.</summary>
public static class BlackjackRulesPanelEditor
{
    [MenuItem("Tools/Blackjack/Add Rules Panel")]
    public static void AddRulesPanel()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Stop Play mode before adding the rules panel.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/main.unity")
        {
            Debug.LogWarning("Open Assets/Scenes/main.unity before adding the rules panel.");
            return;
        }

        GameObject canvasObject = GameObject.Find("Blackjack Canvas");
        if (canvasObject == null)
        {
            Debug.LogWarning("Blackjack Canvas was not found.");
            return;
        }

        RectTransform canvas = canvasObject.GetComponent<RectTransform>();
        if (canvas.Find("Rules Panel") != null)
        {
            Debug.Log("Rules Panel already exists.");
            return;
        }

        Text title = canvas.Find("Title")?.GetComponent<Text>();
        Font font = title != null ? title.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        Color ink = title != null ? title.color : new Color(0.12f, 0.16f, 0.19f);

        GameObject panelObject = new GameObject("Rules Panel", typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(panelObject, "Add Blackjack Rules Panel");
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.SetParent(canvas, false);
        panel.anchorMin = panel.anchorMax = new Vector2(1f, 0.5f);
        panel.pivot = new Vector2(1f, 0.5f);
        panel.anchoredPosition = new Vector2(-8f, 16f);
        panel.sizeDelta = new Vector2(206f, 312f);

        Image background = panelObject.GetComponent<Image>();
        background.color = new Color(0.96f, 0.98f, 0.985f, 0.96f);
        background.raycastTarget = false;

        Shadow shadow = panelObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.09f);
        shadow.effectDistance = new Vector2(3f, -3f);

        CreateText("Rules Heading", panel, font, ink, "블랙잭 규칙", 18,
            new Vector2(0f, 127f), new Vector2(184f, 30f), TextAnchor.MiddleLeft);

        GameObject ruleObject = new GameObject("Rules Text", typeof(RectTransform), typeof(Text));
        RectTransform ruleRect = ruleObject.GetComponent<RectTransform>();
        ruleRect.SetParent(panel, false);
        ruleRect.anchorMin = ruleRect.anchorMax = new Vector2(0.5f, 0.5f);
        ruleRect.anchoredPosition = new Vector2(0f, -19f);
        ruleRect.sizeDelta = new Vector2(184f, 252f);
        Text rules = ruleObject.GetComponent<Text>();
        rules.font = font;
        rules.fontSize = 14;
        rules.color = ink;
        rules.alignment = TextAnchor.UpperLeft;
        rules.horizontalOverflow = HorizontalWrapMode.Wrap;
        rules.verticalOverflow = VerticalWrapMode.Truncate;
        rules.lineSpacing = 1.1f;
        rules.raycastTarget = false;
        rules.text = "21에 가깝게 만드세요.\n21 초과 시 패배\n\n"
            + "A = 1 또는 11\n2~10 = 숫자 그대로\nJ / Q / K = 10\n\n"
            + "히트: 카드 받기\n스탠드: 턴 종료\n\n"
            + "딜러는 17 이상까지 뽑기\n높은 점수 승리\n동점은 무승부";

        Transform overlay = canvas.Find("Result Overlay");
        if (overlay != null)
            panel.SetSiblingIndex(overlay.GetSiblingIndex());

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Added Rules Panel to the right side of the blackjack canvas.");
    }

    private static void CreateText(string name, RectTransform parent, Font font, Color color,
        string content, int size, Vector2 position, Vector2 dimensions, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        Text label = textObject.GetComponent<Text>();
        label.font = font;
        label.fontSize = size;
        label.fontStyle = FontStyle.Bold;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        label.text = content;
    }
}
