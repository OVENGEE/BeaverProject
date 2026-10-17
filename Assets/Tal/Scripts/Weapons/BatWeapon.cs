using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BatWeapon : MonoBehaviour
{
    [Header("Melee Hit Settings")]
    [SerializeField] private Transform hitPoint;
    [SerializeField] private float hitRadius = 1.2f;
    [SerializeField] private int damage = 30;
    [SerializeField] private LayerMask targetLayers;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 18f;
    [Tooltip("Upward angle added to the knockback trajectory (in degrees)")]
    [SerializeField] private float launchUpwardAngle = 30f;

    [Header("Animation Timings")]
    [Tooltip("Time in seconds into the animation when the bat physically hits the target")]
    [SerializeField] private float hitImpactDelay = 0.25f;
    [Tooltip("Total duration of the swing animation before removing the bat")]
    [SerializeField] private float totalSwingDuration = 0.8f;

    private BeaverInputActions inputActions;
    private BeaverController beaver;
    private Rigidbody2D beaverRb;
    private Animator beaverAnimator;
    private bool hasSwung = false;

    private void Awake()
    {
        inputActions = new BeaverInputActions();
        inputActions.Player.Action.started += _ => ExecuteSwing();
    }

    private void Start()
    {
        beaver = GetComponentInParent<BeaverController>();

        if (beaver != null)
        {
            beaverRb = beaver.GetComponent<Rigidbody2D>();
            beaverAnimator = beaver.GetComponentInChildren<Animator>();
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.RegisterEquipment(gameObject);
        }

        if (beaverAnimator != null)
        {
            beaverAnimator.SetBool("BatEquipped", true);
        }
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        UpdateWalkingAnimation();
    }

    private void UpdateWalkingAnimation()
    {
        if (beaverAnimator == null) return;

        // Check if beaver is moving horizontally
        bool isMoving = beaverRb != null && Mathf.Abs(beaverRb.linearVelocity.x) > 0.1f;

        beaverAnimator.SetBool("IsWalkingWithBat", isMoving);
    }

    private void ExecuteSwing()
    {
        if (hasSwung) return;

        hasSwung = true;

        if (beaverAnimator != null)
        {
            beaverAnimator.SetTrigger("BatSwing");
        }

        StartCoroutine(SwingRoutine());
    }

    private IEnumerator SwingRoutine()
    {
        yield return new WaitForSeconds(hitImpactDelay);

        PerformHitCheck();

        float remainingTime = Mathf.Max(0f, totalSwingDuration - hitImpactDelay);
        yield return new WaitForSeconds(remainingTime);

        DestroyBat();
    }

    private void PerformHitCheck()
    {
        Vector3 origin = hitPoint != null ? hitPoint.position : transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, hitRadius, targetLayers);

        float facingDir = beaver != null ? Mathf.Sign(beaver.transform.localScale.x) : 1f;

        float rad = launchUpwardAngle * Mathf.Deg2Rad;
        Vector2 knockbackDir = new Vector2(Mathf.Cos(rad) * facingDir, Mathf.Sin(rad)).normalized;

        HashSet<GameObject> hitTargets = new HashSet<GameObject>();

        foreach (var col in hits)
        {
            if (beaver != null && col.transform.IsChildOf(beaver.transform)) continue;
            if (hitTargets.Contains(col.gameObject)) continue;

            hitTargets.Add(col.gameObject);

            if (col.TryGetComponent<BeaverHealth>(out var health))
            {
                health.TakeDamage(damage);
            }
            else if (col.GetComponentInParent<BeaverHealth>() != null)
            {
                col.GetComponentInParent<BeaverHealth>().TakeDamage(damage);
            }

            if (col.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
            else if (col.attachedRigidbody != null)
            {
                col.attachedRigidbody.linearVelocity = Vector2.zero;
                col.attachedRigidbody.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    private void DestroyBat()
    {
        ResetAnimationState();
        Destroy(gameObject);
    }

    private void ResetAnimationState()
    {
        if (beaverAnimator != null)
        {
            beaverAnimator.SetBool("IsWalkingWithBat", false);
            beaverAnimator.SetBool("BatEquipped", false);
        }
    }

    private void OnDestroy()
    {
        ResetAnimationState();
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = hitPoint != null ? hitPoint.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, hitRadius);
    }
}