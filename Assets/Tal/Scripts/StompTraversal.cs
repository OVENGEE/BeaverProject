using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class StompTraversal : MonoBehaviour
{
    [Header("Stomp Settings")]
    [SerializeField] private float stompSpeed = 25f;
    [Tooltip("A brief pause in the air before shooting downward adds a lot of weight to the impact.")]
    [SerializeField] private float hangTime = 0.1f;
    [SerializeField] private GameObject stompImpactPrefab;

    private BeaverController beaver;
    private BeaverInputActions inputActions;
    private bool isStomping = false;
    private float aimInput;

    private void Awake()
    {
        beaver = GetComponentInParent<BeaverController>();

        inputActions = new BeaverInputActions();

        // Reusing the Aim action (Up/Down) we made for the weapons
        inputActions.Player.Aim.performed += ctx => aimInput = ctx.ReadValue<float>();
        inputActions.Player.Aim.canceled += _ => aimInput = 0f;

        // Bind Space bar
        inputActions.Player.Action.started += _ => TryStomp();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void TryStomp()
    {
        // Stomp can only happen in the air
        if (beaver == null || isStomping || beaver.IsGrounded) return;

        // Player MUST be holding Down Arrow (Negative Aim Input)
        if (aimInput < 0f)
        {
            StartCoroutine(StompRoutine());
        }
    }

    private IEnumerator StompRoutine()
    {
        isStomping = true;

        if (beaver.TryGetComponent<Rigidbody2D>(out var rb))
        {
            float originalGravity = rb.gravityScale;

            // 1. Hangtime (Stall in the air briefly)
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;

            // Turn off input detection while stomping
            inputActions.Disable();

            yield return new WaitForSeconds(hangTime);

            // 2. The Plunge
            rb.linearVelocity = new Vector2(0f, -stompSpeed);

            // Wait one physics frame to ensure we don't instantly detect the ground if we just jumped
            yield return new WaitForFixedUpdate();

            // 3. Wait until the BeaverController detects the ground
            while (!beaver.IsGrounded)
            {
                // Constantly re-apply the velocity so other physics forces don't slow it down
                rb.linearVelocity = new Vector2(0f, -stompSpeed);
                yield return new WaitForFixedUpdate();
            }

            // 4. Impact Recovery
            rb.gravityScale = originalGravity;

            if (stompImpactPrefab != null)
            {
                Instantiate(stompImpactPrefab, beaver.transform.position, Quaternion.identity);
            }
        }

        // Consume and destroy the traversal object
        Destroy(gameObject);
    }
}