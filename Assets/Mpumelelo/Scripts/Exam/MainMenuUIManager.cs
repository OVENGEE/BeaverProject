using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Canvas mainMenuCanvas;
    [SerializeField] private GameSceneManager sceneManager;
    [SerializeField] private GameObject mapSelectionPanel;
    [SerializeField] private GameObject soundsPanel;

    [Header("Map Selection")]
    [SerializeField] private Button[] mapButtons = new Button[3];
    [SerializeField] private TMP_Text[] mapNameLabels = new TMP_Text[3];
    [SerializeField] private TMP_Text[] mapSelectionLabels = new TMP_Text[3];
    [SerializeField] private Button startMatchButton;

    [Header("Navigation")]
    [SerializeField] private Button mapSelectionBackButton;
    [SerializeField] private Button soundsBackButton;

    [Header("Selected Map Appearance")]
    [SerializeField] private Color selectedMapColor = new Color(1f, 0.78f, 0.2f, 1f);
    [SerializeField] private Color unselectedMapColor = new Color(0.82f, 0.8f, 0.75f, 1f);
    [SerializeField] private string selectedMapStatusText = "Selected";
    [SerializeField] private string unselectedMapStatusText = "Click to select";

    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSound;

    private readonly List<GameObject> visibleMainMenuObjects = new List<GameObject>();
    private int selectedMapIndex;
    private bool isSubmenuOpen;

    private void Start()
    {
        if (mainMenuCanvas == null || mapSelectionPanel == null || soundsPanel == null)
        {
            Debug.LogError("Main menu panels are not assigned. Use Tools > Beaver Project > Create Editable Main Menu Panels.", this);
            return;
        }

        if (sceneManager == null)
        {
            sceneManager = FindAnyObjectByType<GameSceneManager>();
        }

        if (sceneManager == null)
        {
            Debug.LogError("Could not find a GameSceneManager in the Main Menu scene.", this);
            return;
        }

        mapSelectionPanel.SetActive(false);
        soundsPanel.SetActive(false);
        if (!SetupMenuButtons())
        {
            return;
        }

        SelectMap(0);
    }

    private void Update()
    {
        if (!isSubmenuOpen || Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return;
        }

        CloseSubmenu();
    }

    private bool SetupMenuButtons()
    {
        Button[] buttons = mainMenuCanvas.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            // Automatically add the click sound to every button on the menu
            button.onClick.AddListener(PlayButtonClick);
            switch (button.gameObject.name)
            {
                case "StartBTN":
                    button.onClick.RemoveListener(OpenMapSelection);
                    button.onClick.AddListener(OpenMapSelection);
                    break;
                case "Sounds":
                    button.onClick.RemoveListener(OpenSounds);
                    button.onClick.AddListener(OpenSounds);
                    break;
            }
        }

        if (mapButtons == null || mapButtons.Length != 3 ||
            mapNameLabels == null || mapNameLabels.Length != 3 ||
            mapSelectionLabels == null || mapSelectionLabels.Length != 3 ||
            startMatchButton == null || mapSelectionBackButton == null || soundsBackButton == null)
        {
            Debug.LogError("Map selection panel references are incomplete. Re-run the editable main menu setup tool.", this);
            return false;
        }

        for (int i = 0; i < mapButtons.Length; i++)
        {
            int mapIndex = i;
            mapButtons[i].onClick.RemoveAllListeners();
            mapButtons[i].onClick.AddListener(() => SelectMap(mapIndex));
        }

        startMatchButton.onClick.RemoveListener(StartSelectedMatch);
        startMatchButton.onClick.AddListener(StartSelectedMatch);
        mapSelectionBackButton.onClick.RemoveListener(CloseSubmenu);
        mapSelectionBackButton.onClick.AddListener(CloseSubmenu);
        soundsBackButton.onClick.RemoveListener(CloseSubmenu);
        soundsBackButton.onClick.AddListener(CloseSubmenu);
        return true;
    }

    private void OpenMapSelection()
    {
        OpenSubmenu(mapSelectionPanel);
    }

    private void PlayButtonClick()
    {
        if (buttonClickSound != null && SoundFXManager.instance != null)
        {
            SoundFXManager.instance.PlaySoundFXClip(buttonClickSound, transform, 1f);
        }
    }

    private void OpenSounds()
    {
        OpenSubmenu(soundsPanel);
    }

    private void OpenSubmenu(GameObject panel)
    {
        visibleMainMenuObjects.Clear();
        Transform canvasTransform = mainMenuCanvas.transform;
        for (int i = 0; i < canvasTransform.childCount; i++)
        {
            GameObject child = canvasTransform.GetChild(i).gameObject;
            if (child == mapSelectionPanel || child == soundsPanel)
            {
                continue;
            }

            if (child.activeSelf)
            {
                visibleMainMenuObjects.Add(child);
            }

            child.SetActive(false);
        }

        mapSelectionPanel.SetActive(panel == mapSelectionPanel);
        soundsPanel.SetActive(panel == soundsPanel);
        panel.transform.SetAsLastSibling();
        isSubmenuOpen = true;
    }

    private void CloseSubmenu()
    {
        mapSelectionPanel.SetActive(false);
        soundsPanel.SetActive(false);

        foreach (GameObject menuObject in visibleMainMenuObjects)
        {
            if (menuObject != null)
            {
                menuObject.SetActive(true);
            }
        }

        visibleMainMenuObjects.Clear();
        isSubmenuOpen = false;
    }

    private void SelectMap(int mapIndex)
    {
        if (mapIndex < 0 || mapIndex >= mapButtons.Length)
        {
            Debug.LogError($"Map selection index {mapIndex} is outside the configured map list.", this);
            return;
        }

        selectedMapIndex = mapIndex;
        for (int i = 0; i < mapButtons.Length; i++)
        {
            bool isSelected = i == selectedMapIndex;
            Image cardImage = mapButtons[i].targetGraphic as Image;
            if (cardImage != null)
            {
                cardImage.color = isSelected ? selectedMapColor : unselectedMapColor;
            }

            mapNameLabels[i].color = Color.black;
            mapSelectionLabels[i].text = isSelected ? selectedMapStatusText : unselectedMapStatusText;
            mapSelectionLabels[i].color = Color.black;
        }
    }

    private void StartSelectedMatch()
    {
        switch (selectedMapIndex)
        {
            case 0:
                sceneManager.LoadGameplayMap1();
                break;
            case 1:
                sceneManager.LoadGameplayMap2();
                break;
            case 2:
                sceneManager.LoadGameplayMap3();
                break;
            default:
                Debug.LogError($"Cannot load map for selection index {selectedMapIndex}.", this);
                break;
        }
    }
}
