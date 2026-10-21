using UnityEngine;
using UnityEngine.InputSystem;

public class RocketLauncherWeapon : MonoBehaviour
{
    [Header("Aiming Settings")]
    [SerializeField] private Transform reticle;
    [SerializeField] private float orbitRadius = 1.5f;
    [SerializeField] private float aimSpeed = 90f; // Degrees per second
    [SerializeField] private float minAngle = -75f; // Lowest downward angle
    [SerializeField] private float maxAngle = 75f;  // Highest upward angle

    [Header("Shooting Settings")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float rocketSpeed = 18f;
    [SerializeField] private int maxAmmo = 1;
    [Tooltip("Assign your Terrain/Ground layer here to prevent firePoint terrain clipping.")]
    [SerializeField] private LayerMask groundLayer;

    private BeaverInputActions inputActions;
    private Transform beaverTransform;

    private float currentAngle = 0f; // 0 = straight ahead
    private float aimInput;
    private int currentAmmo;

    private void Awake()
    {
        inputActions = new BeaverInputActions();

        inputActions.Player.Aim.performed += ctx => aimInput = ctx.ReadValue<float>();
        inputActions.Player.Aim.canceled += _ => aimInput = 0f;

        inputActions.Player.Action.started += _ => Shoot();
    }

    private void Start()
    {
        currentAmmo = maxAmmo;

        beaverTransform = GetComponentInParent<BeaverController>()?.transform ?? transform.parent;

        if (reticle != null)
        {
            reticle.gameObject.SetActive(true);
        }
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        UpdateAiming();
    }

    private void UpdateAiming()
    {
        currentAngle += aimInput * aimSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        transform.localRotation = Quaternion.Euler(0, 0, currentAngle);

        float facingDirection = beaverTransform != null ? Mathf.Sign(beaverTransform.localScale.x) : 1f;

        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad) * facingDirection, Mathf.Sin(rad));

        if (reticle != null)
        {
            reticle.position = (Vector2)transform.position + (dir * orbitRadius);
        }
    }

    private void Shoot()
    {
        if (currentAmmo <= 0 || rocketPrefab == null || firePoint == null) return;

        Vector2 beaverOrigin = beaverTransform != null ? (Vector2)beaverTransform.position : (Vector2)transform.position;
        Vector2 intendedSpawnPos = firePoint.position;

        // Perform linecast to detect if firePoint is clipping inside or past terrain
        RaycastHit2D hit = Physics2D.Linecast(beaverOrigin, intendedSpawnPos, groundLayer);
        Vector2 safeSpawnPos = (hit.collider != null) ? hit.point : intendedSpawnPos;

        // Calculate direct vector from safe spawn point to reticle
        Vector2 shootDirection = ((Vector2)reticle.position - safeSpawnPos).normalized;

        // Instantiate rocket at safe surface point
        GameObject rocket = Instantiate(rocketPrefab, safeSpawnPos, Quaternion.identity);

        if (rocket.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = shootDirection * rocketSpeed;
        }

        // Rotate rocket sprite to match trajectory angle
        float rocketAngle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        rocket.transform.rotation = Quaternion.Euler(0, 0, rocketAngle);

        currentAmmo--;

        if (currentAmmo <= 0)
        {
            DestroyRocket();
        }
    }

    private void DestroyRocket()
    {
        if (reticle != null)
        {
            Destroy(reticle.gameObject);
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (reticle != null)
        {
            Destroy(reticle.gameObject);
        }
    }
}