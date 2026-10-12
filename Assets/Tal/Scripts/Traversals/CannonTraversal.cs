using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CannonTraversal : MonoBehaviour
{
    [Header("Cannon Parts Setup")]
    [Tooltip("Assign the 'Sprite' child GameObject (the Cannon Barrel)")]
    [SerializeField] private Transform barrelTransform;
    [Tooltip("Assign the 'Sprite2' child GameObject (the Cannon Wheels/Base)")]
    [SerializeField] private Transform wheelBaseTransform;

    [Header("Aiming Settings")]
    [SerializeField] private Transform reticle;
    [SerializeField] private float orbitRadius = 1.5f;
    [SerializeField] private float aimSpeed = 90f;
    [SerializeField] private float minAngle = -75f;
    [SerializeField] private float maxAngle = 75f;

    [Header("Launch Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float minLaunchForce = 10f;
    [SerializeField] private float maxLaunchForce = 35f;
    [SerializeField] private float maxChargeTime = 1.5f;

    [Header("Charge Meter Settings")]
    [SerializeField] private Transform chargeMeterTransform;

    private BeaverInputActions inputActions;
    private BeaverController beaver;
    private Rigidbody2D beaverRb;

    // Track hidden beaver components
    private List<Renderer> hiddenBeaverRenderers = new List<Renderer>();
    private List<Collider2D> disabledBeaverColliders = new List<Collider2D>();

    // Pivot & Scale caching
    private Vector3 initialBarrelOffset;
    private Vector3 originalChargeMeterScale = Vector3.one;

    private float currentAngle = 0f;
    private float aimInput;

    private bool isInsideCannon = false;
    private bool isCharging = false;
    private float chargeTimer = 0f;
    private float currentChargeRatio = 0f;

    private void Awake()
    {
        inputActions = new BeaverInputActions();

        inputActions.Player.Aim.performed += ctx => aimInput = ctx.ReadValue<float>();
        inputActions.Player.Aim.canceled += _ => aimInput = 0f;

        inputActions.Player.Action.started += _ => OnActionStarted();
        inputActions.Player.Action.canceled += _ => OnActionCanceled();
    }

    private void Start()
    {
        beaver = GetComponentInParent<BeaverController>();
        if (beaver != null)
        {
            beaverRb = beaver.GetComponent<Rigidbody2D>();
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.RegisterEquipment(gameObject);
        }

        // 1. Cache pivot offset between barrel (Sprite) and wheel base (Sprite2)
        if (barrelTransform != null && wheelBaseTransform != null)
        {
            initialBarrelOffset = barrelTransform.localPosition - wheelBaseTransform.localPosition;
        }

        // 2. Cache initial Inspector scale for charge meter so it doesn't appear stretched/ginormous
        if (chargeMeterTransform != null)
        {
            originalChargeMeterScale = chargeMeterTransform.localScale;
            chargeMeterTransform.localScale = new Vector3(0f, originalChargeMeterScale.y, originalChargeMeterScale.z);
        }

        if (reticle != null) reticle.gameObject.SetActive(false);
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        if (!isInsideCannon) return;

        UpdateAiming();
        UpdateCharging();
    }

    private void OnActionStarted()
    {
        if (!isInsideCannon)
        {
            EnterCannon();
        }
        else if (!isCharging)
        {
            StartCharging();
        }
    }

    private void OnActionCanceled()
    {
        if (isInsideCannon && isCharging)
        {
            LaunchBeaver();
        }
    }

    private void EnterCannon()
    {
        if (beaver == null) return;

        isInsideCannon = true;

        hiddenBeaverRenderers.Clear();
        disabledBeaverColliders.Clear();

        // Disable beaver graphics (excluding cannon parts)
        foreach (var r in beaver.GetComponentsInChildren<Renderer>())
        {
            if (r.transform.IsChildOf(transform)) continue;

            if (r.enabled)
            {
                r.enabled = false;
                hiddenBeaverRenderers.Add(r);
            }
        }

        // Disable beaver physics colliders (excluding cannon parts)
        foreach (var c in beaver.GetComponentsInChildren<Collider2D>())
        {
            if (c.transform.IsChildOf(transform)) continue;

            if (c.enabled)
            {
                c.enabled = false;
                disabledBeaverColliders.Add(c);
            }
        }

        if (beaverRb != null)
        {
            beaverRb.linearVelocity = Vector2.zero;
            beaverRb.simulated = false; // Freeze beaver physics while inside
        }

        if (reticle != null) reticle.gameObject.SetActive(true);
    }

    private void UpdateAiming()
    {
        currentAngle += aimInput * aimSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        // Pivot barrel (Sprite) around the wheel base (Sprite2) center
        if (barrelTransform != null && wheelBaseTransform != null)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            barrelTransform.localPosition = wheelBaseTransform.localPosition + (rotation * initialBarrelOffset);
            barrelTransform.localRotation = rotation;
        }
        else if (barrelTransform != null)
        {
            barrelTransform.localRotation = Quaternion.Euler(0, 0, currentAngle);
        }

        // Calculate reticle orbit position based on facing direction
        float facingDirection = beaver != null ? Mathf.Sign(beaver.transform.localScale.x) : 1f;
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 aimDirection = new Vector2(Mathf.Cos(rad) * facingDirection, Mathf.Sin(rad));

        if (reticle != null)
        {
            Vector3 origin = firePoint != null ? firePoint.position : transform.position;
            reticle.position = origin + (Vector3)(aimDirection * orbitRadius);
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        chargeTimer = 0f;
        currentChargeRatio = 0f;
    }

    private void UpdateCharging()
    {
        if (!isCharging) return;

        chargeTimer += Time.deltaTime;

        float pingPongTime = Mathf.PingPong(chargeTimer, maxChargeTime);
        currentChargeRatio = pingPongTime / maxChargeTime;

        // Scale X relative to original Inspector size (preserving original Y height and Z depth)
        if (chargeMeterTransform != null)
        {
            chargeMeterTransform.localScale = new Vector3(
                originalChargeMeterScale.x * currentChargeRatio,
                originalChargeMeterScale.y,
                originalChargeMeterScale.z
            );
        }
    }

    private void LaunchBeaver()
    {
        if (beaver == null) return;

        isCharging = false;

        Vector3 launchPosition = firePoint != null ? firePoint.position : transform.position;
        Vector2 launchDirection = reticle != null ? ((Vector2)reticle.position - (Vector2)launchPosition).normalized : transform.right;
        float launchForce = Mathf.Lerp(minLaunchForce, maxLaunchForce, currentChargeRatio);

        // Move beaver to fire point
        beaver.transform.position = launchPosition;

        // Restore beaver graphics & colliders
        RestoreBeaver();

        // Re-enable physics and launch
        if (beaverRb != null)
        {
            beaverRb.simulated = true;
            beaverRb.linearVelocity = launchDirection * launchForce;
        }

        CleanupAndDestroy();
    }

    private void RestoreBeaver()
    {
        foreach (var r in hiddenBeaverRenderers)
        {
            if (r != null) r.enabled = true;
        }
        hiddenBeaverRenderers.Clear();

        foreach (var c in disabledBeaverColliders)
        {
            if (c != null) c.enabled = true;
        }
        disabledBeaverColliders.Clear();
    }

    private void CleanupAndDestroy()
    {
        if (reticle != null) Destroy(reticle.gameObject);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (beaver != null && isInsideCannon)
        {
            RestoreBeaver();
            if (beaverRb != null) beaverRb.simulated = true;
        }

        if (reticle != null) Destroy(reticle.gameObject);
    }
}