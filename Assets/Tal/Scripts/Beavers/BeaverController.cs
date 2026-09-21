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

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;

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

    private bool isMyTurn = false;

    // Buffer state variables
    private bool isBuffering = false;
    private JumpKey firstKey = JumpKey.None;
    private Coroutine bufferCoroutine;

    public bool IsGrounded => isGrounded;
    public float MoveInput => moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

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
        if (!isMyTurn) return;

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

        UpdateAnimatorStates();
    }

    private void FixedUpdate()
    {
        // Grounded horizontal walking only. Mid-air trajectory is strictly physical momentum.
        if (isGrounded && isMyTurn)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void UpdateAnimatorStates()
    {
        if (animator == null) return;

        // IsWalking is true when grounded and moving horizontally
        bool isWalking = isGrounded && Mathf.Abs(moveInput) > 0.01f;
        animator.SetBool("IsWalking", isWalking);

        // IsFalling is true when airborne and moving downward
        bool isFalling = !isGrounded && rb.linearVelocity.y < -0.1f;
        animator.SetBool("IsFalling", isFalling);
    }

    public void SetAiming(bool isAiming)
    {
        if (animator != null)
        {
            animator.SetBool("IsAiming", isAiming);
        }
    }

    private void OnEnterPressed()
    {
        if (!isMyTurn || !isGrounded) return;

        if (!isBuffering)
        {
            firstKey = JumpKey.Enter;
            bufferCoroutine = StartCoroutine(BufferRoutine());
        }
        else if (firstKey == JumpKey.Enter)
        {
            // Enter -> Enter combo (Backward Jump)
            StopCoroutine(bufferCoroutine);
            ExecuteJump(backwardJump, backward: true, isBackflip: false);
        }
        else if (firstKey == JumpKey.Backspace)
        {
            // Backspace -> Enter combo (Small Backflip)
            StopCoroutine(bufferCoroutine);
            ExecuteJump(smallBackflip, backward: true, isBackflip: true);
        }
    }

    private void OnBackspacePressed()
    {
        if (!isMyTurn || !isGrounded) return;

        if (!isBuffering)
        {
            firstKey = JumpKey.Backspace;
            bufferCoroutine = StartCoroutine(BufferRoutine());
        }
        else if (firstKey == JumpKey.Backspace)
        {
            // Backspace -> Backspace combo (Backflip)
            StopCoroutine(bufferCoroutine);
            ExecuteJump(backflip, backward: true, isBackflip: true);
        }
        else if (firstKey == JumpKey.Enter)
        {
            // Enter -> Backspace (Forward Jump)
            StopCoroutine(bufferCoroutine);
            ExecuteJump(forwardJump, backward: false, isBackflip: false);
        }
    }

    private IEnumerator BufferRoutine()
    {
        isBuffering = true;
        yield return new WaitForSeconds(comboWindow);

        if (firstKey == JumpKey.Enter)
        {
            ExecuteJump(forwardJump, backward: false, isBackflip: false);
        }
        else if (firstKey == JumpKey.Backspace)
        {
            ExecuteJump(verticalJump, backward: false, isBackflip: false);
        }
    }

    private void ExecuteJump(Vector2 jumpVelocity, bool backward, bool isBackflip)
    {
        ResetBuffer();

        float xDirection = backward ? -facingDirection : facingDirection;
        rb.linearVelocity = new Vector2(xDirection * jumpVelocity.x, jumpVelocity.y);

        isGrounded = false;
        groundCheckCooldown = 0.15f;

        if (animator != null)
        {
            if (isBackflip)
            {
                animator.SetTrigger("Backflip");
            }
            else
            {
                animator.SetTrigger("Jump");
            }
        }
    }

    public void ExecuteMidAirJump(Vector2 newVelocity)
    {
        rb.linearVelocity = newVelocity;

        isGrounded = false;
        groundCheckCooldown = 0.15f;
    }

    public void SetTurnActive(bool active)
    {
        isMyTurn = active;

        if (!active)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsAiming", false);
            }
        }
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