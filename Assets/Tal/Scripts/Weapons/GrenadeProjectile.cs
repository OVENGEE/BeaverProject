using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GrenadeProjectile : MonoBehaviour
{
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private int damage = 45;
    [SerializeField] private GameObject explosionEffectPrefab;

    private void Start()
    {
        Invoke(nameof(Explode), fuseTime);
    }

    private void Explode()
    {
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Detect all colliders within explosion range
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent<BeaverHealth>(out var beaverHealth))
            {
                beaverHealth.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}