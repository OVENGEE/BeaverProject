using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DrillProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 2.5f;

    [Header("Terrain Destruction")]
    [SerializeField] private float drillRadius = 1.2f;
    [Tooltip("How frequently in seconds terrain is cut away (lower = smoother tunnel).")]
    [SerializeField] private float destructionInterval = 0.04f;

    [Header("Damage Settings")]
    [SerializeField] private int damagePerTick = 10;
    [Tooltip("Cooldown between damage ticks so HP isn't instantly depleted in 1 frame.")]
    [SerializeField] private float damageInterval = 0.15f;

    private Rigidbody2D rb;
    private DestructibleTerrain terrain;
    private float destructionTimer;
    private float damageTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Direct the drill forward based on prefab orientation
        rb.linearVelocity = transform.right * speed;

        // Cache terrain instance
        terrain = FindAnyObjectByType<DestructibleTerrain>();

        // Destroy drill prefab after lifetime expires
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        HandleContinuousDestruction();
        HandleBeaverDamage();
    }

    private void HandleContinuousDestruction()
    {
        destructionTimer += Time.deltaTime;
        if (destructionTimer >= destructionInterval)
        {
            destructionTimer = 0f;

            if (terrain != null)
            {
                // Carve a circular hole at the current drill position (false = circular cut)
                terrain.RemoveTerrainAt(transform.position, drillRadius, false);
            }
        }
    }

    private void HandleBeaverDamage()
    {
        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;

            // Check for beavers inside the drill radius as it bores through
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, drillRadius);
            foreach (var hit in hitColliders)
            {
                if (hit.TryGetComponent<BeaverHealth>(out var health))
                {
                    health.TakeDamage(damagePerTick);
                }
            }
        }
    }
}