using System;
using System.Linq;
using Blackjack;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class BlackjackSceneBuilder
{
    private static readonly Color Ink = new Color(0.12f, 0.16f, 0.19f);
    private static readonly Color Accent = new Color(0.25f, 0.72f, 0.79f);
    private static Font font;

    [MenuItem("Tools/Blackjack/Build Main Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/main.unity", OpenSceneMode.Single);
        GameObject old = GameObject.Find("Blackjack Canvas");
        if (old != null) UnityEngine.Object.DestroyImmediate(old);
        old = GameObject.Find("EventSystem");
        if (old != null) UnityEngine.Object.DestroyImmediate(old);
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject canvasObject = new GameObject("Blackjack Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(AudioSource));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        AudioSource cardAudio = canvasObject.GetComponent<AudioSource>();
        cardAudio.playOnAwake = false;
        cardAudio.loop = false;
        cardAudio.spatialBlend = 0;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960, 600);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        RectTransform root = canvasObject.GetComponent<RectTransform>();
        RectTransform background = Panel("White Background", root, Vector2.zero, new Vector2(960, 600), Color.white);
        background.anchorMin = Vector2.zero;
        background.anchorMax = Vector2.one;
        background.offsetMin = background.offsetMax = Vector2.zero;

        Sprite back = AssetDatabase.LoadAllAssetsAtPath("Assets/Image/카트겉면.png").OfType<Sprite>().First();
        DeckCard("Deck Card Bottom", root, back, new Vector2(-414, -6));
        DeckCard("Deck Card Middle", root, back, new Vector2(-408, 0));
        RectTransform deck = DeckCard("Deck Card Top", root, back, new Vector2(-402, 6));
        Label("Deck Label", root, "DECK", new Vector2(-402, -98), new Vector2(112, 28), 17, Ink);

        Label("Title", root, "BLACKJACK", new Vector2(0, 248), new Vector2(500, 52), 36, Ink);
        Text coins = Label("Coins", root, "COINS  10", new Vector2(365, 248), new Vector2(170, 42), 21, Ink);
        Text status = Label("Result", root, "Press START to play", new Vector2(0, 204), new Vector2(620, 40), 23, Accent);
        Text dealerScore = Label("Dealer Score", root, "DEALER", new Vector2(0, 165), new Vector2(400, 34), 22, Ink);
        RectTransform dealerHand = Hand("Dealer Cards", root, new Vector2(0, 70));
        Panel("Divider", root, new Vector2(0, -21), new Vector2(680, 2), new Color(0.84f, 0.88f, 0.89f));
        Text playerScore = Label("Player Score", root, "YOU", new Vector2(0, -49), new Vector2(400, 34), 22, Ink);
        RectTransform playerHand = Hand("Player Cards", root, new Vector2(0, -137));
        Button start = MakeButton("Start", root, "START", new Vector2(0, -253), new Vector2(168, 52));
        Button hit = MakeButton("Hit", root, "HIT", new Vector2(-96, -253), new Vector2(160, 52));
        Button stand = MakeButton("Stand", root, "STAND", new Vector2(96, -253), new Vector2(160, 52));
        hit.gameObject.SetActive(false);
        stand.gameObject.SetActive(false);

        RectTransform resultOverlay = Panel("Result Overlay", root, Vector2.zero, new Vector2(960, 600), new Color(0, 0, 0, 0.38f));
        resultOverlay.anchorMin = Vector2.zero;
        resultOverlay.anchorMax = Vector2.one;
        resultOverlay.offsetMin = resultOverlay.offsetMax = Vector2.zero;
        RectTransform resultWindow = Panel("Result Window", resultOverlay, Vector2.zero, new Vector2(430, 270), Color.white);
        Shadow windowShadow = resultWindow.gameObject.AddComponent<Shadow>();
        windowShadow.effectColor = new Color(0, 0, 0, 0.22f);
        windowShadow.effectDistance = new Vector2(10, -10);
        Text outcome = Label("Outcome", resultWindow, "VICTORY", new Vector2(0, 77), new Vector2(370, 52), 34, Ink);
        Text coinChange = Label("Coin Change", resultWindow, "+1 COIN", new Vector2(0, 24), new Vector2(340, 38), 24, Accent);
        Text popupBalance = Label("Popup Balance", resultWindow, "TOTAL COINS  10", new Vector2(0, -18), new Vector2(340, 34), 20, Ink);
        Button restart = MakeButton("Restart", resultWindow, "PLAY AGAIN", new Vector2(0, -82), new Vector2(188, 52));
        resultOverlay.gameObject.SetActive(false);

        BlackjackController controller = canvasObject.AddComponent<BlackjackController>();
        controller.Configure(status, dealerScore, playerScore, coins, resultOverlay.gameObject,
            outcome, coinChange, popupBalance, dealerHand, playerHand, deck,
            start, hit, stand, restart,
            Faces("ana카드.png"), Faces("Creal카드.png"), Faces("Para카드.png"), Faces("Tapie카드.png"),
            back, cardAudio, AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sound/카드효과음.mp3"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sound/성공.mp3"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/sound/미션실패.mp3"));
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/main.unity", true) };
        Selection.activeGameObject = canvasObject;
        Debug.Log("Simple Blackjack Canvas built with card sprites.");
    }

    private static Sprite[] Faces(string filename)
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Image/" + filename).OfType<Sprite>()
            .OrderBy(sprite => sprite.rect.x).ToArray();
        if (sprites.Length != 13) throw new InvalidOperationException(filename + " must have 13 card sprites.");
        return sprites;
    }

    private static RectTransform DeckCard(string name, Transform parent, Sprite sprite, Vector2 position)
    {
        RectTransform rect = Rect(name, parent, position, new Vector2(112, 153));
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        Shadow shadow = rect.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.2f);
        shadow.effectDistance = new Vector2(3, -4);
        shadow.useGraphicAlpha = true;
        return rect;
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var gameObject = new GameObject(name, typeof(RectTransform));
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static RectTransform Panel(string name, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        RectTransform rect = Rect(name, parent, position, size);
        rect.gameObject.AddComponent<Image>().color = color;
        return rect;
    }

    private static Text Label(string name, Transform parent, string value, Vector2 position, Vector2 size, int fontSize, Color color)
    {
        RectTransform rect = Rect(name, parent, position, size);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform Hand(string name, Transform parent, Vector2 position)
    {
        RectTransform rect = Rect(name, parent, position, new Vector2(760, 158));
        HorizontalLayoutGroup layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return rect;
    }

    private static Button MakeButton(string name, Transform parent, string value, Vector2 position, Vector2 size)
    {
        RectTransform rect = Panel(name, parent, position, size, Accent);
        Button button = rect.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.77f, 0.94f, 0.96f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        Label("Label", rect, value, Vector2.zero, size, 21, Ink);
        return button;
    }
}
