using UnityEngine;
using UnityEngine.InputSystem;

public class DoubleJumpTraversal : MonoBehaviour
{
    [Header("Double Jump Settings")]
    [Tooltip("Uses standard forward jump force (X = horizontal speed, Y = vertical impulse)")]
    [SerializeField] private Vector2 jumpVelocity = new Vector2(4f, 6f);

    [SerializeField] private GameObject jumpEffectPrefab; // Optional visual particle effect

    private BeaverController beaver;
    private BeaverInputActions inputActions;

    private void Awake()
    {
        // Find the parent Beaver script
        beaver = GetComponentInParent<BeaverController>();

        inputActions = new BeaverInputActions();

        // Bind Space bar (Player.Action action)
        inputActions.Player.Action.started += _ => TryDoubleJump();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void TryDoubleJump()
    {
        if (beaver == null) return;

        // Double jump can ONLY happen while airborne
        if (!beaver.IsGrounded)
        {
            float horizontalInput = beaver.MoveInput;

            // Player MUST be holding either Left Arrow (-1) or Right Arrow (+1)
            if (horizontalInput != 0f)
            {
                // Determine absolute jump direction from arrow key
                float xDirection = Mathf.Sign(horizontalInput);
                Vector2 finalJumpVelocity = new Vector2(xDirection * jumpVelocity.x, jumpVelocity.y);

                // Override beaver linear velocity mid-air
                beaver.ExecuteMidAirJump(finalJumpVelocity);

                if (jumpEffectPrefab != null)
                {
                    Instantiate(jumpEffectPrefab, beaver.transform.position, Quaternion.identity);
                }

                // Consume and destroy the traversal object after use
                Destroy(gameObject);
            }
        }
    }
}