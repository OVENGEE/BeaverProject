using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Tutorial Beaver Setup")]
    [SerializeField] private BeaverController tutorialBeaver;

    [Header("Tutorial UI Buttons")]
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject menuButton;

    [Header("Camera Control Settings")]
    [Tooltip("Main camera transform. Automatically finds Camera.main if left empty.")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float panSpeed = 15f;

    private bool isPanning = false;

    public BeaverController TutorialBeaver => tutorialBeaver;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Ensure time is running normally at start
        Time.timeScale = 1f;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Start()
    {
        // Hide UI buttons on start
        if (restartButton != null) restartButton.SetActive(false);
        if (menuButton != null) menuButton.SetActive(false);

        if (tutorialBeaver != null)
        {
            tutorialBeaver.SetTurnActive(true);
            SnapToBeaver();
        }
    }

    private void Update()
    {
        // Enforce active control state
        if (tutorialBeaver != null)
        {
            tutorialBeaver.SetTurnActive(true);
        }
    }

    private void LateUpdate()
    {
        if (tutorialBeaver == null || cameraTransform == null) return;

        HandlePanning();
        CheckForOtherActions();

        if (!isPanning)
        {
            FollowBeaver();
        }
    }

    // Called when the beaver touches the physical goal object
    public void CompleteTutorial()
    {
        // 1. Reveal UI buttons
        if (restartButton != null) restartButton.SetActive(true);
        if (menuButton != null) menuButton.SetActive(true);

        // 2. Freeze game physics & input time
        Time.timeScale = 0f;
    }

    private void HandlePanning()
    {
        Vector2 panInput = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) panInput.y += 1;
            if (Keyboard.current.sKey.isPressed) panInput.y -= 1;
            if (Keyboard.current.aKey.isPressed) panInput.x -= 1;
            if (Keyboard.current.dKey.isPressed) panInput.x += 1;
        }

        if (panInput != Vector2.zero)
        {
            isPanning = true;
            cameraTransform.position += new Vector3(panInput.x, panInput.y, 0f) * (panSpeed * Time.unscaledDeltaTime);
        }
    }

    private void CheckForOtherActions()
    {
        if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
        {
            isPanning = false;
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            isPanning = false;
        }

        if (tutorialBeaver != null && Mathf.Abs(tutorialBeaver.MoveInput) > 0.1f)
        {
            isPanning = false;
        }
    }

    private void FollowBeaver()
    {
        Vector3 targetPosition = tutorialBeaver.transform.position + offset;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, Time.unscaledDeltaTime * smoothSpeed);
    }

    public void SnapToBeaver()
    {
        isPanning = false;
        if (tutorialBeaver != null && cameraTransform != null)
        {
            cameraTransform.position = tutorialBeaver.transform.position + offset;
        }
    }
}