using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DashTraversal : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private GameObject dashEffectPrefab;

    private BeaverController beaver;
    private BeaverInputActions inputActions;
    private bool isDashing = false;

    private void Awake()
    {
        beaver = GetComponentInParent<BeaverController>();

        inputActions = new BeaverInputActions();
        inputActions.Player.Action.started += _ => TryDash();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void TryDash()
    {
        if (beaver == null || isDashing) return;

        float horizontalInput = beaver.MoveInput;

        // Player MUST be holding either Left Arrow (-1) or Right Arrow (+1)
        if (horizontalInput != 0f)
        {
            float direction = Mathf.Sign(horizontalInput);
            StartCoroutine(DashRoutine(direction));
        }
    }

    private IEnumerator DashRoutine(float direction)
    {
        isDashing = true;

        if (beaver.TryGetComponent<Rigidbody2D>(out var rb))
        {
            float originalGravity = rb.gravityScale;

            // Zero out gravity and force direct horizontal velocity
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(direction * dashSpeed, 0f);

            // Turn off input detection while dashing
            inputActions.Disable();

            if (dashEffectPrefab != null)
            {
                Instantiate(dashEffectPrefab, beaver.transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(dashDuration);

            // Restore normal gravity
            rb.gravityScale = originalGravity;
        }

        // Destroy the traversal object after the dash completes
        Destroy(gameObject);
    }
}