using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DashTraversal : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private GameObject dashEffectPrefab;

    [Header("Damage Settings")]
    [SerializeField] private int dashDamage = 25;
    [SerializeField] private float hitRadius = 0.8f;

    private BeaverController beaver;
    private BeaverInputActions inputActions;
    private bool isDashing = false;
    private HashSet<BeaverHealth> hitBeavers = new HashSet<BeaverHealth>();

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

            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(direction * dashSpeed, 0f);

            inputActions.Disable();

            if (dashEffectPrefab != null)
            {
                Instantiate(dashEffectPrefab, beaver.transform.position, Quaternion.identity);
            }

            float elapsed = 0f;
            while (elapsed < dashDuration)
            {
                elapsed += Time.deltaTime;
                CheckForHits();
                yield return null;
            }

            rb.gravityScale = originalGravity;
        }

        Destroy(gameObject);
    }

    private void CheckForHits()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(beaver.transform.position, hitRadius);
        foreach (var hit in hits)
        {
            // Ignore the beaver performing the dash
            if (hit.gameObject != beaver.gameObject && hit.TryGetComponent<BeaverHealth>(out var targetHealth))
            {
                if (!hitBeavers.Contains(targetHealth))
                {
                    hitBeavers.Add(targetHealth);
                    targetHealth.TakeDamage(dashDamage);
                }
            }
        }
    }
}