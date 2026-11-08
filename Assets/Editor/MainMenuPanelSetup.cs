using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuPanelSetup
{
    private static readonly Color BackgroundColor = new Color(0.13f, 0.09f, 0.07f, 1f);
    private static readonly Color PreviewColor = new Color(0.58f, 0.71f, 0.8f, 1f);
    private static readonly Color TerrainColor = new Color(0.25f, 0.15f, 0.1f, 1f);
    private static readonly Color AccentColor = new Color(1f, 0.78f, 0.2f, 1f);

    [MenuItem("Tools/Beaver Project/Create Editable Main Menu Panels")]
    private static void CreateEditablePanels()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Stop Play Mode", "Stop Play Mode before creating the editable menu panels.", "OK");
            return;
        }

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            EditorUtility.DisplayDialog("Open MainMenu", "Open the MainMenu scene, then run this setup command again.", "OK");
            return;
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        GameSceneManager sceneManager = Object.FindAnyObjectByType<GameSceneManager>();
        MainMenuUIManager menuManager = Object.FindAnyObjectByType<MainMenuUIManager>();
        if (canvas == null || sceneManager == null || menuManager == null)
        {
            EditorUtility.DisplayDialog(
                "Main menu setup incomplete",
                "The MainMenu scene needs a Canvas, GameSceneManager, and MainMenuUIManager.",
                "OK");
            return;
        }

        TMP_Text sampleText = canvas.GetComponentInChildren<TMP_Text>(true);
        GameObject mapPanel = FindOrCreatePanel(canvas.transform, "MapSelectionPanel");
        GameObject soundsPanel = FindOrCreatePanel(canvas.transform, "SoundsPanel");

        if (mapPanel.transform.childCount == 0)
        {
            BuildMapSelectionPanel(mapPanel.transform, sampleText);
        }

        if (soundsPanel.transform.childCount == 0)
        {
            BuildSoundsPanel(soundsPanel.transform, sampleText);
        }

        WireMenuManager(menuManager, canvas, sceneManager, mapPanel, soundsPanel);
        mapPanel.SetActive(false);
        soundsPanel.SetActive(false);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = mapPanel;
        EditorUtility.DisplayDialog(
            "Editable panels created",
            "MapSelectionPanel and SoundsPanel are now scene objects under Canvas. Save the scene to keep the setup.",
            "OK");
    }

    private static GameObject FindOrCreatePanel(Transform canvas, string name)
    {
        Transform existing = canvas.Find(name);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(panel, $"Create {name}");
        panel.transform.SetParent(canvas, false);

        RectTransform rect = (RectTransform)panel.transform;
        Stretch(rect);

        Image background = panel.GetComponent<Image>();
        background.color = BackgroundColor;
        background.raycastTarget = true;
        panel.layer = canvas.gameObject.layer;
        panel.SetActive(false);
        return panel;
    }

    private static void BuildMapSelectionPanel(Transform panel, TMP_Text sampleText)
    {
        CreateText(panel, "Title", "Map Selection", new Vector2(0.25f, 0.85f),
            new Vector2(0.75f, 0.96f), 44f, TextAlignmentOptions.Center, sampleText);
        CreateText(panel, "Subtitle", "Choose where the match will be played", new Vector2(0.2f, 0.8f),
            new Vector2(0.8f, 0.86f), 20f, TextAlignmentOptions.Center, sampleText);

        CreateBackButton(panel, "BackButton", sampleText);

        Button[] cards = new Button[3];
        TMP_Text[] names = new TMP_Text[3];
        TMP_Text[] states = new TMP_Text[3];
        string[] labels = { "Map Name 1", "Map Name 2", "Map Name 3" };
        float[] xMin = { 0.035f, 0.365f, 0.695f };
        float[] xMax = { 0.315f, 0.645f, 0.965f };

        for (int i = 0; i < cards.Length; i++)
        {
            CreateMapCard(panel, i, labels[i], xMin[i], xMax[i], sampleText,
                out cards[i], out names[i], out states[i]);
        }

        CreateButton(panel, "StartMatchButton", "Start Match", new Vector2(0.38f, 0.055f),
            new Vector2(0.62f, 0.15f), AccentColor, sampleText, 28f);

        WireMapControls(panel, cards, names, states);
    }

    private static void CreateMapCard(Transform parent, int index, string mapName, float minX, float maxX,
        TMP_Text sampleText, out Button button, out TMP_Text nameLabel, out TMP_Text selectionLabel)
    {
        string cardName = $"MapCard{index + 1}";
        GameObject card = new GameObject(cardName, typeof(RectTransform), typeof(Image), typeof(Button),
            typeof(MainMenuButtonHover));
        Undo.RegisterCreatedObjectUndo(card, $"Create {cardName}");
        card.transform.SetParent(parent, false);

        RectTransform cardRect = (RectTransform)card.transform;
        cardRect.anchorMin = new Vector2(minX, 0.30f);
        cardRect.anchorMax = new Vector2(maxX, 0.70f);
        cardRect.offsetMin = Vector2.zero;
        cardRect.offsetMax = Vector2.zero;
        card.layer = parent.gameObject.layer;

        Image cardImage = card.GetComponent<Image>();
        cardImage.color = new Color(0.82f, 0.8f, 0.75f, 1f);
        Button cardButton = card.GetComponent<Button>();
        cardButton.targetGraphic = cardImage;
        cardButton.transition = Selectable.Transition.None;

        GameObject preview = new GameObject("PreviewImage", typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(preview, "Create map preview");
        preview.transform.SetParent(card.transform, false);
        RectTransform previewRect = (RectTransform)preview.transform;
        previewRect.anchorMin = new Vector2(0.04f, 0.32f);
        previewRect.anchorMax = new Vector2(0.96f, 0.94f);
        previewRect.offsetMin = Vector2.zero;
        previewRect.offsetMax = Vector2.zero;
        Image previewImage = preview.GetComponent<Image>();
        previewImage.color = PreviewColor;
        previewImage.raycastTarget = false;

        GameObject terrain = new GameObject("TerrainPlaceholder", typeof(RectTransform), typeof(Image));
        Undo.RegisterCreatedObjectUndo(terrain, "Create terrain preview placeholder");
        terrain.transform.SetParent(preview.transform, false);
        RectTransform terrainRect = (RectTransform)terrain.transform;
        terrainRect.anchorMin = new Vector2(0f, 0f);
        terrainRect.anchorMax = new Vector2(1f, 0.34f);
        terrainRect.offsetMin = Vector2.zero;
        terrainRect.offsetMax = Vector2.zero;
        Image terrainImage = terrain.GetComponent<Image>();
        terrainImage.color = TerrainColor;
        terrainImage.raycastTarget = false;

        CreateText(card.transform, "MapName", mapName, new Vector2(0.04f, 0.16f),
            new Vector2(0.96f, 0.28f), 23f, TextAlignmentOptions.Center, sampleText);
        CreateText(card.transform, "SelectionStatus", "Click to select", new Vector2(0.04f, 0.04f),
            new Vector2(0.96f, 0.14f), 15f, TextAlignmentOptions.Center, sampleText);

        button = cardButton;
        nameLabel = card.transform.Find("MapName").GetComponent<TMP_Text>();
        selectionLabel = card.transform.Find("SelectionStatus").GetComponent<TMP_Text>();
    }

    private static void BuildSoundsPanel(Transform panel, TMP_Text sampleText)
    {
        CreateText(panel, "Title", "Sounds", new Vector2(0.25f, 0.82f),
            new Vector2(0.75f, 0.94f), 44f, TextAlignmentOptions.Center, sampleText);
        CreateText(panel, "Placeholder", "Sound settings will be added here.", new Vector2(0.15f, 0.4f),
            new Vector2(0.85f, 0.6f), 24f, TextAlignmentOptions.Center, sampleText);
        CreateBackButton(panel, "BackButton", sampleText);
    }

    private static void CreateBackButton(Transform parent, string name, TMP_Text sampleText)
    {
        Button button = CreateButton(parent, name, "< Back", new Vector2(0.03f, 0.88f),
            new Vector2(0.14f, 0.95f), new Color(0.22f, 0.16f, 0.13f, 1f), sampleText, 20f);
        button.transition = Selectable.Transition.ColorTint;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin,
        Vector2 anchorMax, Color color, TMP_Text sampleText, float fontSize)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button),
            typeof(MainMenuButtonHover));
        Undo.RegisterCreatedObjectUndo(buttonObject, $"Create {name}");
        buttonObject.transform.SetParent(parent, false);
        buttonObject.layer = parent.gameObject.layer;

        RectTransform rect = (RectTransform)buttonObject.transform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = buttonObject.GetComponent<Image>();
        image.color = color;
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;

        CreateText(buttonObject.transform, "Label", label, Vector2.zero, Vector2.one,
            fontSize, TextAlignmentOptions.Center, sampleText);
        return button;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, Vector2 anchorMin,
        Vector2 anchorMax, float fontSize, TextAlignmentOptions alignment, TMP_Text sampleText)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(textObject, $"Create {name}");
        textObject.transform.SetParent(parent, false);
        textObject.layer = parent.gameObject.layer;

        RectTransform rect = (RectTransform)textObject.transform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TMP_Text text = textObject.GetComponent<TextMeshProUGUI>();
        if (sampleText != null)
        {
            text.font = sampleText.font;
            text.fontSharedMaterial = sampleText.fontSharedMaterial;
        }

        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void WireMapControls(Transform panel, Button[] mapButtons, TMP_Text[] mapNames, TMP_Text[] mapStates)
    {
        SerializedObject serializedManager = new SerializedObject(Object.FindAnyObjectByType<MainMenuUIManager>());
        SetObject(serializedManager, "mapSelectionPanel", panel.gameObject);

        SerializedProperty buttons = serializedManager.FindProperty("mapButtons");
        SerializedProperty names = serializedManager.FindProperty("mapNameLabels");
        SerializedProperty states = serializedManager.FindProperty("mapSelectionLabels");
        buttons.arraySize = mapButtons.Length;
        names.arraySize = mapNames.Length;
        states.arraySize = mapStates.Length;

        for (int i = 0; i < mapButtons.Length; i++)
        {
            buttons.GetArrayElementAtIndex(i).objectReferenceValue = mapButtons[i];
            names.GetArrayElementAtIndex(i).objectReferenceValue = mapNames[i];
            states.GetArrayElementAtIndex(i).objectReferenceValue = mapStates[i];
        }

        SetObject(serializedManager, "startMatchButton", panel.Find("StartMatchButton").GetComponent<Button>());
        SetObject(serializedManager, "mapSelectionBackButton", panel.Find("BackButton").GetComponent<Button>());
        serializedManager.ApplyModifiedProperties();
    }

    private static void WireMenuManager(MainMenuUIManager manager, Canvas canvas, GameSceneManager sceneManager,
        GameObject mapPanel, GameObject soundsPanel)
    {
        SerializedObject serializedManager = new SerializedObject(manager);
        SetObject(serializedManager, "mainMenuCanvas", canvas);
        SetObject(serializedManager, "sceneManager", sceneManager);
        SetObject(serializedManager, "mapSelectionPanel", mapPanel);
        SetObject(serializedManager, "soundsPanel", soundsPanel);
        SetObject(serializedManager, "soundsBackButton", soundsPanel.transform.Find("BackButton").GetComponent<Button>());
        serializedManager.ApplyModifiedProperties();
        EditorUtility.SetDirty(manager);
    }

    private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogError($"Could not find serialized field '{propertyName}'.");
            return;
        }

        property.objectReferenceValue = value;
    }
}
