using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GrenadeProjectile : MonoBehaviour
{
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private int damage = 45;
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("Terrain Destruction")]
    [SerializeField] private float terrainDestructionRadius = 2f;

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

        // 1. Deal Damage
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent<BeaverHealth>(out var beaverHealth))
            {
                beaverHealth.TakeDamage(damage);
            }
        }

        // 2. Destroy Terrain (False = Circle cut[cite: 2])
        DestructibleTerrain terrain = FindAnyObjectByType<DestructibleTerrain>();
        if (terrain != null)
        {
            terrain.RemoveTerrainAt(transform.position, terrainDestructionRadius, false); //[cite: 2]
        }

        Destroy(gameObject);
    }
}