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
    [SerializeField] private float terrainDestructionRadius = 1f;

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
            inputActions.Disable();

            if (dashEffectPrefab != null)
            {
                Instantiate(dashEffectPrefab, beaver.transform.position, Quaternion.identity);
            }

            float elapsed = 0f;

            // Switch to fixed time since we are manipulating physics
            while (elapsed < dashDuration)
            {
                // Constantly enforce velocity so hitting another beaver doesn't stop the dash dead in its tracks
                rb.linearVelocity = new Vector2(direction * dashSpeed, 0f);

                CheckForHits();

                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
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
            BeaverHealth targetHealth = hit.GetComponentInParent<BeaverHealth>();

            // If we hit an enemy beaver
            if (targetHealth != null && targetHealth.gameObject != beaver.gameObject)
            {
                if (!hitBeavers.Contains(targetHealth))
                {
                    hitBeavers.Add(targetHealth);
                    targetHealth.TakeDamage(dashDamage);
                }
            }
            // If we hit terrain
            else
            {
                DestructibleTerrain terrain = hit.GetComponentInParent<DestructibleTerrain>();
                if (terrain != null)
                {
                    // Carve a square at the beaver's position as it dashes (True = Rectangle[cite: 2])
                    terrain.RemoveTerrainAt(beaver.transform.position, terrainDestructionRadius, true); //[cite: 2]
                }
            }
        }
    }
}