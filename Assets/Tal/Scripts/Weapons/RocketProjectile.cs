using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RocketProjectile : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private int damage = 45;
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("Terrain Destruction")]
    [SerializeField] private float terrainDestructionRadius = 2f;

    [Header("Impact Settings")]
    [SerializeField] private LayerMask groundLayer;

    private bool hasExploded = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckImpact(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        CheckImpact(collider.gameObject);
    }

    private void CheckImpact(GameObject hitObject)
    {
        if (hasExploded) return;

        // Check if the object is on the assigned ground layer
        bool isGround = ((1 << hitObject.layer) & groundLayer.value) != 0;

        // Check if the object is a beaver
        bool isBeaver = hitObject.CompareTag("BeaverP1") || hitObject.CompareTag("BeaverP2");

        if (isGround || isBeaver)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true; // Prevents double-explosions if multiple colliders are hit in the same frame

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 1. Deal Damage
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent<BeaverHealth>(out var beaverHealth))
            {
                beaverHealth.TakeDamage(damage);
            }
        }

        // 2. Destroy Terrain (False = Circle cut)
        DestructibleTerrain terrain = FindAnyObjectByType<DestructibleTerrain>();
        if (terrain != null)
        {
            terrain.RemoveTerrainAt(transform.position, terrainDestructionRadius, false);
        }

        Destroy(gameObject);
    }
}