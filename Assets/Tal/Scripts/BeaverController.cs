using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class BeaverController : MonoBehaviour
{
    private enum JumpKey { None, Enter, Backspace }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Jump Velocities (X = Horizontal, Y = Vertical)")]
    [SerializeField] private Vector2 forwardJump = new Vector2(4f, 6f);
    [SerializeField] private Vector2 backwardJump = new Vector2(5f, 7f);
    [SerializeField] private Vector2 verticalJump = new Vector2(0f, 9f);
    [SerializeField] private Vector2 backflip = new Vector2(1.5f, 10f);
    [SerializeField] private Vector2 smallBackflip = new Vector2(1.5f, 5f);

    [Header("Input Buffer Settings")]
    [Tooltip("Time window (in seconds) to detect a second keypress before executing a single jump.")]
    [SerializeField] private float comboWindow = 0.15f;

    private Rigidbody2D rb;
    private BeaverInputActions inputActions;

    private float moveInput;
    private bool isLockingDirection;
    private bool isGrounded;
    private float facingDirection = 1f; // 1 = Right, -1 = Left
    private float groundCheckCooldown;

    // Buffer state variables
    private bool isBuffering = false;
    private JumpKey firstKey = JumpKey.None;
    private Coroutine bufferCoroutine;

    public bool IsGrounded => isGrounded;
    public float MoveInput => moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new BeaverInputActions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<float>();
        inputActions.Player.Move.canceled += _ => moveInput = 0f;

        inputActions.Player.LockDirection.performed += _ => isLockingDirection = true;
        inputActions.Player.LockDirection.canceled += _ => isLockingDirection = false;

        inputActions.Player.EnterJump.started += _ => OnEnterPressed();
        inputActions.Player.BackspaceJump.started += _ => OnBackspacePressed();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        // Prevent ground check instantly re-triggering during takeoff
        if (groundCheckCooldown > 0f)
        {
            groundCheckCooldown -= Time.deltaTime;
            isGrounded = false;
        }
        else
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Turning / direction locking is only evaluated on the ground
        if (isGrounded && moveInput != 0 && !isLockingDirection)
        {
            facingDirection = Mathf.Sign(moveInput);
            transform.localScale = new Vector3(facingDirection, 1, 1);
        }
    }

    private void FixedUpdate()
    {
        // Grounded horizontal walking only. Mid-air trajectory is strictly physical momentum.
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void OnEnterPressed()
    {
        // Ignore all jump inputs if in mid-air
        if (!isGrounded) return;

        if (!isBuffering)
        {
            // First input received: open the window to listen for a potential combo
            firstKey = JumpKey.Enter;
            bufferCoroutine = StartCoroutine(BufferRoutine());
        }
        else if (firstKey == JumpKey.Enter)
        {
            // Enter -> Enter combo (Backward Jump)
            StopCoroutine(bufferCoroutine);
            ExecuteJump(backwardJump, backward: true);
        }
        else if (firstKey == JumpKey.Backspace)
        {
            // Backspace -> Enter combo (Small Backflip)
            StopCoroutine(bufferCoroutine);
            ExecuteJump(smallBackflip, backward: true);
        }
    }

    private void OnBackspacePressed()
    {
        // Ignore all jump inputs if in mid-air
        if (!isGrounded) return;

        if (!isBuffering)
        {
            // First input received: open the window to listen for a potential combo
            firstKey = JumpKey.Backspace;
            bufferCoroutine = StartCoroutine(BufferRoutine());
        }
        else if (firstKey == JumpKey.Backspace)
        {
            // Backspace -> Backspace combo (Backflip)
            StopCoroutine(bufferCoroutine);
            ExecuteJump(backflip, backward: true);
        }
        else if (firstKey == JumpKey.Enter)
        {
            // Enter -> Backspace (Undefined combo, defaults to completing single Forward Jump immediately)
            StopCoroutine(bufferCoroutine);
            ExecuteJump(forwardJump, backward: false);
        }
    }

    private IEnumerator BufferRoutine()
    {
        isBuffering = true;
        yield return new WaitForSeconds(comboWindow);

        // Window expired with no second input: trigger corresponding single jump
        if (firstKey == JumpKey.Enter)
        {
            ExecuteJump(forwardJump, backward: false);
        }
        else if (firstKey == JumpKey.Backspace)
        {
            ExecuteJump(verticalJump, backward: false);
        }
    }

    private void ExecuteJump(Vector2 jumpVelocity, bool backward)
    {
        ResetBuffer();

        // Calculate jump direction according to current facing side
        float xDirection = backward ? -facingDirection : facingDirection;
        rb.linearVelocity = new Vector2(xDirection * jumpVelocity.x, jumpVelocity.y);

        // Lock out grounded checks and inputs while airborne
        isGrounded = false;
        groundCheckCooldown = 0.15f;
    }

    public void ExecuteMidAirJump(Vector2 newVelocity)
    {
        rb.linearVelocity = newVelocity;

        // Ensure ground check doesn't accidentally trigger immediately
        isGrounded = false;
        groundCheckCooldown = 0.15f;
    }

    private void ResetBuffer()
    {
        isBuffering = false;
        firstKey = JumpKey.None;
        bufferCoroutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}