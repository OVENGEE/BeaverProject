using UnityEngine;
using UnityEngine.InputSystem;

public class PistolWeapon : MonoBehaviour
{
    [Header("Aiming Settings")]
    [SerializeField] private Transform reticle;
    [SerializeField] private float orbitRadius = 1.5f;
    [SerializeField] private float aimSpeed = 90f; // Degrees per second
    [SerializeField] private float minAngle = -75f; // Lowest downward angle
    [SerializeField] private float maxAngle = 75f;  // Highest upward angle

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 18f;
    [SerializeField] private int maxAmmo = 3;

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

        inputActions.Player.Fire.started += _ => Shoot();
    }

    private void Start()
    {
        currentAmmo = maxAmmo;

        // Find the parent beaver (assuming pistol is attached to the beaver or its hand)
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
        // Adjust angle based on Up/Down input
        currentAngle += aimInput * aimSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        // Determine facing direction from beaver scale (1 = Right, -1 = Left)
        float facingDirection = beaverTransform != null ? Mathf.Sign(beaverTransform.localScale.x) : 1f;

        // Convert angle to horizontal/vertical offsets
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad) * facingDirection, Mathf.Sin(rad));

        // Position the reticle along the semi-circle orbit
        if (reticle != null)
        {
            reticle.position = (Vector2)transform.position + (dir * orbitRadius);
        }

        // Rotate pistol toward aiming direction
        float rotationAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
    }

    private void Shoot()
    {
        if (currentAmmo <= 0 || bulletPrefab == null || firePoint == null) return;

        // Calculate direct vector from Fire Point to Reticle
        Vector2 shootDirection = (reticle.position - firePoint.position).normalized;

        // Instantiate and propel the bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = shootDirection * bulletSpeed;
        }

        // Rotate bullet sprite to match trajectory angle
        float bulletAngle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, bulletAngle);

        currentAmmo--;

        if (currentAmmo <= 0)
        {
            DestroyPistol();
        }
    }

    private void DestroyPistol()
    {
        if (reticle != null)
        {
            Destroy(reticle.gameObject);
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Cleanup reticle if object is destroyed externally
        if (reticle != null)
        {
            Destroy(reticle.gameObject);
        }
    }
}