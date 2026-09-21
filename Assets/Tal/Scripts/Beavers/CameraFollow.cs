using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float panSpeed = 15f;

    private bool isPanning = false;
    private BeaverInputActions inputActions;

    private void Awake()
    {
        inputActions = new BeaverInputActions();

        // Snap back when the action (Space) or aim keys are pressed
        inputActions.Player.Action.started += _ => SnapToActiveBeaver();
        inputActions.Player.Aim.started += _ => SnapToActiveBeaver();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void LateUpdate()
    {
        HandlePanning();
        CheckForOtherActions();

        // Only use your existing follow logic if the player isn't manually panning
        if (!isPanning)
        {
            FollowActiveBeaver();
        }
    }

    private void HandlePanning()
    {
        Vector2 panInput = Vector2.zero;

        // Reading raw WASD directly so it doesn't conflict with your Beaver's movement bindings (Arrow Keys)
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
            transform.position += new Vector3(panInput.x, panInput.y, 0f) * (panSpeed * Time.deltaTime);
        }
    }

    private void CheckForOtherActions()
    {
        // Snap back if the player clicks on a UI card
        if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
        {
            SnapToActiveBeaver();
        }

        // Snap back if the player uses the spacebar for anything else
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SnapToActiveBeaver();
        }

        // Snap back if the player starts walking the beaver
        if (TurnManager.Instance != null && TurnManager.Instance.ActiveBeaver != null)
        {
            if (Mathf.Abs(TurnManager.Instance.ActiveBeaver.MoveInput) > 0.1f)
            {
                SnapToActiveBeaver();
            }
        }
    }

    private void FollowActiveBeaver()
    {
        if (TurnManager.Instance == null || TurnManager.Instance.ActiveBeaver == null) return;

        Transform target = TurnManager.Instance.ActiveBeaver.transform;
        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
    }

    private void SnapToActiveBeaver()
    {
        isPanning = false;
    }
}