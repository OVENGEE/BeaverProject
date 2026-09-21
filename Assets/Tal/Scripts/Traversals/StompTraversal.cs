using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StompTraversal : MonoBehaviour
{
    [Header("Stomp Settings")]
    [SerializeField] private float stompSpeed = 25f;
    [SerializeField] private float hangTime = 0.1f;
    [SerializeField] private GameObject stompImpactPrefab;

    [Header("Damage Settings")]
    [SerializeField] private int stompDamage = 40;
    [SerializeField] private float hitRadius = 1.2f;
    [SerializeField] private float terrainDestructionRadius = 1.2f;

    private BeaverController beaver;
    private BeaverInputActions inputActions;
    private bool isStomping = false;
    private float aimInput;
    private HashSet<BeaverHealth> hitBeavers = new HashSet<BeaverHealth>();

    private void Awake()
    {
        beaver = GetComponentInParent<BeaverController>();

        inputActions = new BeaverInputActions();

        inputActions.Player.Aim.performed += ctx => aimInput = ctx.ReadValue<float>();
        inputActions.Player.Aim.canceled += _ => aimInput = 0f;

        inputActions.Player.Action.started += _ => TryStomp();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void TryStomp()
    {
        if (beaver == null || isStomping || beaver.IsGrounded) return;

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

            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;

            inputActions.Disable();

            yield return new WaitForSeconds(hangTime);

            rb.linearVelocity = new Vector2(0f, -stompSpeed);

            yield return new WaitForFixedUpdate();

            while (!beaver.IsGrounded)
            {
                rb.linearVelocity = new Vector2(0f, -stompSpeed);
                CheckForHits();
                yield return new WaitForFixedUpdate();
            }

            // Final hit check on ground impact
            CheckForHits();

            rb.gravityScale = originalGravity;

            if (stompImpactPrefab != null)
            {
                Instantiate(stompImpactPrefab, beaver.transform.position, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }

    private void CheckForHits()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(beaver.transform.position, hitRadius);
        foreach (var hit in hits)
        {
            BeaverHealth targetHealth = hit.GetComponentInParent<BeaverHealth>();

            if (targetHealth != null && targetHealth.gameObject != beaver.gameObject)
            {
                if (!hitBeavers.Contains(targetHealth))
                {
                    hitBeavers.Add(targetHealth);
                    targetHealth.TakeDamage(stompDamage);
                }
            }
            else
            {
                DestructibleTerrain terrain = hit.GetComponentInParent<DestructibleTerrain>();
                if (terrain != null)
                {
                    // Carve a square as the beaver plunges downward (True = Rectangle[cite: 2])
                    terrain.RemoveTerrainAt(beaver.transform.position, terrainDestructionRadius, true); //[cite: 2]
                }
            }
        }
    }
}