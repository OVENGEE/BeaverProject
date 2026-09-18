using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GrenadeProjectile : MonoBehaviour
{
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private GameObject explosionEffectPrefab;

    private void Start()
    {
        // Start the fuse countdown as soon as it is spawned
        Invoke(nameof(Explode), fuseTime);
    }

    private void Explode()
    {
        // Spawn visual/damage hitbox
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Apply explosion physics to nearby beavers/objects here later

        Destroy(gameObject);
    }
}