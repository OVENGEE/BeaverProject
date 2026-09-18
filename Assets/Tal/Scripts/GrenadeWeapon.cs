using UnityEngine;
using UnityEngine.InputSystem;

public class GrenadeWeapon : MonoBehaviour
{
    [Header("Aiming Settings")]
    [SerializeField] private Transform reticle;
    [SerializeField] private float orbitRadius = 1.5f;
    [SerializeField] private float aimSpeed = 90f;
    [SerializeField] private float minAngle = -75f;
    [SerializeField] private float maxAngle = 75f;

    [Header("Throw Settings")]
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float minThrowForce = 5f;
    [SerializeField] private float maxThrowForce = 25f;
    [SerializeField] private float maxChargeTime = 1.5f;
    [Tooltip("Optional: A sprite or UI element that scales horizontally as you charge")]
    [SerializeField] private Transform chargeMeterTransform;

    private BeaverInputActions inputActions;
    private Transform beaverTransform;

    private float currentAngle = 0f;
    private float aimInput;

    private bool isCharging = false;
    private float currentChargeTime = 0f;

    private void Awake()
    {
        inputActions = new BeaverInputActions();

        inputActions.Player.Aim.performed += ctx => aimInput = ctx.ReadValue<float>();
        inputActions.Player.Aim.canceled += _ => aimInput = 0f;

        // Press Space to start charging
        inputActions.Player.Action.started += _ => StartCharging();

        // Release Space to throw
        inputActions.Player.Action.canceled += _ => ThrowGrenade();
    }

    private void Start()
    {
        beaverTransform = GetComponentInParent<BeaverController>()?.transform ?? transform.parent;

        if (reticle != null) reticle.gameObject.SetActive(true);
        if (chargeMeterTransform != null) chargeMeterTransform.localScale = new Vector3(0, 1, 1);
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        UpdateAiming();
        UpdateCharging();
    }

    private void UpdateAiming()
    {
        currentAngle += aimInput * aimSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        // Local rotation handles flipping perfectly based on beaver scale
        transform.localRotation = Quaternion.Euler(0, 0, currentAngle);

        float facingDirection = beaverTransform != null ? Mathf.Sign(beaverTransform.localScale.x) : 1f;

        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad) * facingDirection, Mathf.Sin(rad));

        if (reticle != null)
        {
            reticle.position = (Vector2)transform.position + (dir * orbitRadius);
        }
    }

    private void UpdateCharging()
    {
        if (!isCharging) return;

        currentChargeTime += Time.deltaTime;
        currentChargeTime = Mathf.Clamp(currentChargeTime, 0f, maxChargeTime);

        // Update visual meter if assigned
        if (chargeMeterTransform != null)
        {
            float chargeRatio = currentChargeTime / maxChargeTime;
            // Scales the meter from 0 to 1 on the X axis
            chargeMeterTransform.localScale = new Vector3(chargeRatio, 1, 1);
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        currentChargeTime = 0f;
    }

    private void ThrowGrenade()
    {
        if (!isCharging || grenadePrefab == null || firePoint == null) return;

        isCharging = false;

        // Calculate force based on how long Space was held
        float chargeRatio = currentChargeTime / maxChargeTime;
        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeRatio);

        // Calculate world direction from firePoint to reticle
        Vector2 throwDirection = (reticle.position - firePoint.position).normalized;

        // Spawn grenade
        GameObject grenade = Instantiate(grenadePrefab, firePoint.position, Quaternion.identity);

        if (grenade.TryGetComponent<Rigidbody2D>(out var rb))
        {
            // Apply velocity
            rb.linearVelocity = throwDirection * throwForce;

            // Add some random spin to make it look like a thrown grenade
            rb.angularVelocity = Random.Range(-360f, 360f);
        }

        DestroyWeapon();
    }

    private void DestroyWeapon()
    {
        if (reticle != null) Destroy(reticle.gameObject);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (reticle != null) Destroy(reticle.gameObject);
    }
}