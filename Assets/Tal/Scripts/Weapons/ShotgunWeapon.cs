using UnityEngine;
using UnityEngine.InputSystem;

public class ShotgunWeapon : MonoBehaviour
{
    [Header("Aiming Settings")]
    [SerializeField] private Transform reticle;
    [SerializeField] private float orbitRadius = 1.5f;
    [SerializeField] private float aimSpeed = 90f;
    [SerializeField] private float minAngle = -75f;
    [SerializeField] private float maxAngle = 75f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 18f;
    [Tooltip("Total number of bullets fired in one shot")]
    [SerializeField] private int pelletCount = 5;
    [Tooltip("The total angle (in degrees) of the firing cone")]
    [SerializeField] private float spreadAngle = 30f;
    [Tooltip("Assign your Terrain/Ground layer here to prevent firePoint terrain clipping.")]
    [SerializeField] private LayerMask groundLayer;

    private BeaverInputActions inputActions;
    private Transform beaverTransform;

    private float currentAngle = 0f;
    private float aimInput;

    private void Awake()
    {
        inputActions = new BeaverInputActions();

        inputActions.Player.Aim.performed += ctx => aimInput = ctx.ReadValue<float>();
        inputActions.Player.Aim.canceled += _ => aimInput = 0f;

        inputActions.Player.Action.started += _ => Shoot();
    }

    private void Start()
    {
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
        if (bulletPrefab == null || firePoint == null) return;

        Vector2 beaverOrigin = beaverTransform != null ? (Vector2)beaverTransform.position : (Vector2)transform.position;
        Vector2 intendedSpawnPos = firePoint.position;

        // Perform linecast to check if firePoint has clipped inside or past terrain
        RaycastHit2D hit = Physics2D.Linecast(beaverOrigin, intendedSpawnPos, groundLayer);
        Vector2 safeSpawnPos = (hit.collider != null) ? hit.point : intendedSpawnPos;

        // Base direction towards the reticle relative to safe spawn point
        Vector2 centerDirection = ((Vector2)reticle.position - safeSpawnPos).normalized;

        // Convert center direction to angle in degrees
        float centerAngle = Mathf.Atan2(centerDirection.y, centerDirection.x) * Mathf.Rad2Deg;

        // Calculate starting angle
        float startAngle = centerAngle - (spreadAngle / 2f);

        // Angle step between each pellet
        float angleStep = pelletCount > 1 ? spreadAngle / (pelletCount - 1) : 0f;

        for (int i = 0; i < pelletCount; i++)
        {
            // Calculate exact angle for this pellet
            float pelletAngle = startAngle + (angleStep * i);

            // Convert angle back to directional vector
            Vector2 pelletDirection = new Vector2(
                Mathf.Cos(pelletAngle * Mathf.Deg2Rad),
                Mathf.Sin(pelletAngle * Mathf.Deg2Rad)
            );

            // Instantiate pellet at safe outer wall position
            GameObject bullet = Instantiate(bulletPrefab, safeSpawnPos, Quaternion.identity);

            if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.linearVelocity = pelletDirection * bulletSpeed;
            }

            // Rotate bullet sprite to match pellet trajectory
            bullet.transform.rotation = Quaternion.Euler(0, 0, pelletAngle);
        }

        DestroyShotgun();
    }

    private void DestroyShotgun()
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