using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject pauseMenuCanvas;

    [Header("Editable Submenu Panels")]
    [SerializeField] private GameObject cardGlossaryPanel;
    [SerializeField] private GameObject soundsPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private Button submenuBackButton;

    private readonly List<GameObject> controlsPanelContent = new List<GameObject>();
    private readonly List<GameObject> activeMenuObjectsBeforeSubmenu = new List<GameObject>();
    private GameObject submenuPanel;
    private GameObject activeSubmenuPanel;
    private TMP_Text submenuTitle;
    private bool isSubmenuOpen;
    private bool useEditablePanels;

    public static bool IsPaused { get; private set; } = false;

    private void Start()
    {
        SetupSubmenuNavigation();

        // Ensure the pause canvas starts hidden and game time is running
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }
        Time.timeScale = 1f;
        IsPaused = false;
    }

    private void Update()
    {
        // Toggle pause whenever Escape is pressed
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused)
            {
                if (isSubmenuOpen)
                {
                    ReturnToPauseMenu();
                }
                else
                {
                    Resume();
                }
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        if (submenuPanel != null)
        {
            submenuPanel.SetActive(false);
        }
        if (activeSubmenuPanel != null)
        {
            activeSubmenuPanel.SetActive(false);
            activeSubmenuPanel = null;
        }

        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(true);
        }

        isSubmenuOpen = false;
        Time.timeScale = 0f; // Freeze game physics and turn timers
        IsPaused = true;
    }

    public void Resume()
    {
        if (isSubmenuOpen)
        {
            ReturnToPauseMenu();
        }

        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }

        Time.timeScale = 1f; // Unfreeze game
        IsPaused = false;
    }

    public void ShowCardGlossary()
    {
        ShowSubmenu("Card Glossary", false);
    }

    public void ShowSounds()
    {
        ShowSubmenu("Sounds", false);
    }

    public void ShowControls()
    {
        ShowSubmenu("Controls", true);
    }

    private void SetupSubmenuNavigation()
    {
        if (pauseMenuCanvas == null)
        {
            return;
        }

        Button[] buttons = pauseMenuCanvas.GetComponentsInChildren<Button>(true);
        Button cardGlossaryButton = null;
        Button soundsButton = null;
        Button controlsButton = null;

        foreach (Button button in buttons)
        {
            switch (button.gameObject.name)
            {
                case "CardGlossaryButton":
                    cardGlossaryButton = button;
                    break;
                case "SoundsButton":
                    soundsButton = button;
                    break;
                case "ControlsButton":
                    controlsButton = button;
                    break;
            }
        }

        if (cardGlossaryButton == null && soundsButton == null && controlsButton == null)
        {
            return;
        }

        if (cardGlossaryButton != null)
        {
            cardGlossaryButton.onClick.AddListener(ShowCardGlossary);
        }

        if (soundsButton != null)
        {
            soundsButton.onClick.AddListener(ShowSounds);
        }

        if (controlsButton != null)
        {
            controlsButton.onClick.AddListener(ShowControls);
        }

        Transform[] children = pauseMenuCanvas.GetComponentsInChildren<Transform>(true);
        TMP_Text existingText = pauseMenuCanvas.GetComponentInChildren<TMP_Text>(true);
        foreach (Transform child in children)
        {
            if (child.gameObject.name.EndsWith("ControlsTXT", StringComparison.OrdinalIgnoreCase))
            {
                controlsPanelContent.Add(child.gameObject);
            }
        }

        useEditablePanels = cardGlossaryPanel != null
            && soundsPanel != null
            && controlsPanel != null
            && submenuBackButton != null;

        if (useEditablePanels)
        {
            cardGlossaryPanel.SetActive(false);
            soundsPanel.SetActive(false);
            controlsPanel.SetActive(false);
            submenuBackButton.onClick.AddListener(ReturnToPauseMenu);
        }
        else
        {
            CreateSubmenuPanel(existingText);
        }
    }

    private void CreateSubmenuPanel(TMP_Text existingText)
    {
        submenuPanel = new GameObject("PauseSubmenuPanel", typeof(RectTransform), typeof(Image));
        submenuPanel.transform.SetParent(pauseMenuCanvas.transform, false);
        submenuPanel.transform.SetAsFirstSibling();

        RectTransform panelTransform = (RectTransform)submenuPanel.transform;
        panelTransform.anchorMin = new Vector2(0.5f, 0.5f);
        panelTransform.anchorMax = new Vector2(0.5f, 0.5f);
        panelTransform.anchoredPosition = Vector2.zero;
        panelTransform.sizeDelta = new Vector2(1700f, 900f);

        Image panelImage = submenuPanel.GetComponent<Image>();
        panelImage.color = new Color(0.08f, 0.1f, 0.15f, 0.97f);
        panelImage.raycastTarget = true;

        GameObject titleObject = new GameObject("SubmenuTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(submenuPanel.transform, false);
        RectTransform titleTransform = (RectTransform)titleObject.transform;
        titleTransform.anchorMin = new Vector2(0.1f, 0.68f);
        titleTransform.anchorMax = new Vector2(0.9f, 0.9f);
        titleTransform.offsetMin = Vector2.zero;
        titleTransform.offsetMax = Vector2.zero;

        submenuTitle = titleObject.GetComponent<TextMeshProUGUI>();
        CopyTextStyle(existingText, submenuTitle);
        submenuTitle.fontSize = 56f;
        submenuTitle.alignment = TextAlignmentOptions.Center;
        submenuTitle.color = Color.white;
        submenuTitle.raycastTarget = false;

        GameObject backButtonObject = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
        backButtonObject.transform.SetParent(submenuPanel.transform, false);
        RectTransform backButtonTransform = (RectTransform)backButtonObject.transform;
        backButtonTransform.anchorMin = new Vector2(0.5f, 0.18f);
        backButtonTransform.anchorMax = new Vector2(0.5f, 0.18f);
        backButtonTransform.anchoredPosition = Vector2.zero;
        backButtonTransform.sizeDelta = new Vector2(280f, 90f);

        Image backButtonImage = backButtonObject.GetComponent<Image>();
        backButtonImage.color = new Color(0.25f, 0.42f, 0.58f, 1f);

        Button backButton = backButtonObject.GetComponent<Button>();
        backButton.targetGraphic = backButtonImage;
        backButton.onClick.AddListener(ReturnToPauseMenu);

        GameObject backLabelObject = new GameObject("BackLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        backLabelObject.transform.SetParent(backButtonObject.transform, false);
        RectTransform backLabelTransform = (RectTransform)backLabelObject.transform;
        backLabelTransform.anchorMin = Vector2.zero;
        backLabelTransform.anchorMax = Vector2.one;
        backLabelTransform.offsetMin = Vector2.zero;
        backLabelTransform.offsetMax = Vector2.zero;

        TMP_Text backLabel = backLabelObject.GetComponent<TextMeshProUGUI>();
        CopyTextStyle(existingText, backLabel);
        backLabel.text = "Back";
        backLabel.fontSize = 36f;
        backLabel.alignment = TextAlignmentOptions.Center;
        backLabel.color = Color.white;
        backLabel.raycastTarget = false;

        submenuPanel.SetActive(false);
    }

    private static void CopyTextStyle(TMP_Text source, TMP_Text target)
    {
        if (source != null)
        {
            target.font = source.font;
            target.fontSharedMaterial = source.fontSharedMaterial;
        }
    }

    private void ShowSubmenu(string title, bool showControls)
    {
        if (!IsPaused || (!useEditablePanels && submenuPanel == null))
        {
            return;
        }

        activeMenuObjectsBeforeSubmenu.Clear();
        Transform canvasTransform = pauseMenuCanvas.transform;
        for (int i = 0; i < canvasTransform.childCount; i++)
        {
            GameObject child = canvasTransform.GetChild(i).gameObject;
            if (child == submenuPanel)
            {
                continue;
            }

            if (child.activeSelf)
            {
                activeMenuObjectsBeforeSubmenu.Add(child);
            }

            child.SetActive(false);
        }

        if (useEditablePanels)
        {
            GameObject panel = title == "Card Glossary"
                ? cardGlossaryPanel
                : title == "Sounds"
                    ? soundsPanel
                    : controlsPanel;

            activeSubmenuPanel = panel;
            panel.SetActive(true);
            submenuBackButton.gameObject.SetActive(true);
            submenuBackButton.transform.SetAsLastSibling();
            if (showControls)
            {
                foreach (GameObject content in controlsPanelContent)
                {
                    content.SetActive(true);
                }
            }

            isSubmenuOpen = true;
            return;
        }

        submenuTitle.text = title;
        submenuPanel.SetActive(true);
        submenuPanel.transform.SetAsFirstSibling();

        if (showControls)
        {
            foreach (GameObject content in controlsPanelContent)
            {
                content.SetActive(true);
            }
        }

        isSubmenuOpen = true;
    }

    private void ReturnToPauseMenu()
    {
        if (!isSubmenuOpen)
        {
            return;
        }

        if (useEditablePanels)
        {
            if (activeSubmenuPanel != null)
            {
                activeSubmenuPanel.SetActive(false);
                activeSubmenuPanel = null;
            }

            submenuBackButton.gameObject.SetActive(false);
        }
        else
        {
            submenuPanel.SetActive(false);
        }
        foreach (GameObject content in controlsPanelContent)
        {
            content.SetActive(false);
        }

        foreach (GameObject menuObject in activeMenuObjectsBeforeSubmenu)
        {
            menuObject.SetActive(true);
        }

        activeMenuObjectsBeforeSubmenu.Clear();
        isSubmenuOpen = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f; // ALWAYS unfreeze before reloading!
        IsPaused = false;

        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadGameplay();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void MainMenu()
    {
        Time.timeScale = 1f; // ALWAYS unfreeze before scene swap!
        IsPaused = false;

        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadMainMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}