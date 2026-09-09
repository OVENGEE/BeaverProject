using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class BeaverController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Jump Velocities (X = Horizontal, Y = Vertical)")]
    [SerializeField] private Vector2 forwardJump = new Vector2(3f, 6f);
    [SerializeField] private Vector2 backwardJump = new Vector2(5f, 7f);
    [SerializeField] private Vector2 verticalJump = new Vector2(0f, 9f);
    [SerializeField] private Vector2 backflip = new Vector2(4f, 10f);
    [SerializeField] private Vector2 smallBackflip = new Vector2(2f, 4f);

    [Header("Input Combo Window")]
    [SerializeField] private float comboWindow = 0.25f;

    private Rigidbody2D rb;
    private BeaverInputActions inputActions;

    private float moveInput;
    private bool isLockingDirection;
    private bool isGrounded;
    private float facingDirection = 1f; // 1 = Right, -1 = Left

    private float lastEnterTime = -10f;
    private float lastBackspaceTime = -10f;
    private Coroutine comboCoroutine;

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
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && moveInput != 0)
        {
            if (!isLockingDirection)
            {
                facingDirection = Mathf.Sign(moveInput);
                transform.localScale = new Vector3(facingDirection, 1, 1);
            }
        }
    }

    private void FixedUpdate()
    {
        // Grounded movement only; midair velocity is fully physics-driven
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void OnEnterPressed()
    {
        if (!isGrounded) return;

        float currentTime = Time.time;

        // Check Backspace -> Enter combo (Small Backflip)
        if (currentTime - lastBackspaceTime <= comboWindow)
        {
            if (comboCoroutine != null) StopCoroutine(comboCoroutine);
            ExecuteJump(smallBackflip, backward: true);
            ResetInputTimers();
            return;
        }

        // Check Enter -> Enter combo (Backward Jump)
        if (currentTime - lastEnterTime <= comboWindow)
        {
            if (comboCoroutine != null) StopCoroutine(comboCoroutine);
            ExecuteJump(backwardJump, backward: true);
            ResetInputTimers();
            return;
        }

        lastEnterTime = currentTime;
        comboCoroutine = StartCoroutine(WaitAndExecuteSingleJump(ForwardJumpExecution));
    }

    private void OnBackspacePressed()
    {
        if (!isGrounded) return;

        float currentTime = Time.time;

        // Check Backspace -> Backspace combo (Backflip)
        if (currentTime - lastBackspaceTime <= comboWindow)
        {
            if (comboCoroutine != null) StopCoroutine(comboCoroutine);
            ExecuteJump(backflip, backward: true);
            ResetInputTimers();
            return;
        }

        lastBackspaceTime = currentTime;
        comboCoroutine = StartCoroutine(WaitAndExecuteSingleJump(VerticalJumpExecution));
    }

    private IEnumerator WaitAndExecuteSingleJump(System.Action executeAction)
    {
        yield return new WaitForSeconds(comboWindow);
        executeAction.Invoke();
    }

    private void ForwardJumpExecution()
    {
        ExecuteJump(forwardJump, backward: false);
        ResetInputTimers();
    }

    private void VerticalJumpExecution()
    {
        ExecuteJump(verticalJump, backward: false);
        ResetInputTimers();
    }

    private void ExecuteJump(Vector2 jumpImpulse, bool backward)
    {
        float xDirection = backward ? -facingDirection : facingDirection;
        rb.linearVelocity = new Vector2(xDirection * jumpImpulse.x, jumpImpulse.y);
    }

    private void ResetInputTimers()
    {
        lastEnterTime = -10f;
        lastBackspaceTime = -10f;
        comboCoroutine = null;
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