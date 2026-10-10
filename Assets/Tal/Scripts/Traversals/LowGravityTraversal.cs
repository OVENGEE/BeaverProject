using System.Collections;
using UnityEngine;

public class LowGravityTraversal : MonoBehaviour
{
    [Header("Gravity Settings")]
    [Tooltip("Target gravity scale while active (default standard gravity is 1.0).")]
    [SerializeField] private float lowGravityScale = 0.35f;

    [Tooltip("Duration in seconds that low gravity remains active.")]
    [SerializeField] private float duration = 8f;

    [Header("Visual Effects (Optional)")]
    [SerializeField] private GameObject activationEffectPrefab;

    private Rigidbody2D parentRb;
    private float originalGravity;
    private bool isEffectActive = false;

    private void Start()
    {
        // Get the Rigidbody2D from the parent Beaver
        parentRb = GetComponentInParent<Rigidbody2D>();

        if (parentRb != null)
        {
            StartCoroutine(ApplyLowGravityRoutine());
        }
        else
        {
            Debug.LogWarning("LowGravityTraversal prefab was spawned without a parent Rigidbody2D!");
            Destroy(gameObject);
        }
    }

    private IEnumerator ApplyLowGravityRoutine()
    {
        isEffectActive = true;

        // Cache original gravity scale and reduce it
        originalGravity = parentRb.gravityScale;
        parentRb.gravityScale = lowGravityScale;

        // Instantiate visual effect as child of this object if assigned
        if (activationEffectPrefab != null)
        {
            Instantiate(activationEffectPrefab, parentRb.transform.position, Quaternion.identity, transform);
        }

        // Keep low gravity active for the specified duration
        yield return new WaitForSeconds(duration);

        // Restore original gravity
        RestoreGravity();

        // Destroy the traversal prefab object
        Destroy(gameObject);
    }

    private void RestoreGravity()
    {
        if (isEffectActive && parentRb != null)
        {
            parentRb.gravityScale = originalGravity;
            isEffectActive = false;
        }
    }

    private void OnDestroy()
    {
        // Safety guard: Restores normal gravity if the timer runs out or the turn switches mid-effect
        RestoreGravity();
    }
}