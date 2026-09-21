using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 15;

    [Header("Terrain Destruction")]
    [SerializeField] private float terrainDestructionRadius = 0.5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Deal damage if we hit a beaver
        if (collision.gameObject.TryGetComponent<BeaverHealth>(out var health))
        {
            health.TakeDamage(damage);
        }
        else
        {
            // Otherwise, check if we hit a terrain chunk and destroy a rectangle (True = Rectangle cut[cite: 2])
            DestructibleTerrain terrain = collision.gameObject.GetComponentInParent<DestructibleTerrain>();
            if (terrain != null)
            {
                terrain.RemoveTerrainAt(collision.contacts[0].point, terrainDestructionRadius, true); //[cite: 2]
            }
        }

        Destroy(gameObject);
    }
}