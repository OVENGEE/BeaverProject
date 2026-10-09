using UnityEngine;

public class Medkit : MonoBehaviour
{
    [Header("Medkit Settings")]
    [SerializeField] private int healAmount = 30;

    [Header("Visual Effects")]
    [Tooltip("Prefab of the 'plus' object or floating text that appears when healed.")]
    [SerializeField] private GameObject plusEffectPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private float plusDisplayDuration = 1.5f;

    private BeaverInputActions inputActions;
    private bool hasBeenUsed = false;

    private void Awake()
    {
        inputActions = new BeaverInputActions();
        inputActions.Player.Action.started += _ => UseMedkit();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Start()
    {
        // Register equipment with TurnManager so the turn timer resolves properly
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.RegisterEquipment(gameObject);
        }
    }

    private void UseMedkit()
    {
        if (hasBeenUsed) return;

        // Ensure there is an active beaver on turn
        if (TurnManager.Instance == null || TurnManager.Instance.ActiveBeaver == null) return;

        BeaverController activeBeaver = TurnManager.Instance.ActiveBeaver;
        BeaverHealth health = activeBeaver.GetComponent<BeaverHealth>();

        if (health != null)
        {
            hasBeenUsed = true;

            // 1. Heal the active beaver
            health.Heal(healAmount);

            // 2. Instantiate the 'plus' visual feedback
            if (plusEffectPrefab != null)
            {
                Vector3 spawnPosition = activeBeaver.transform.position + spawnOffset;
                GameObject plusInstance = Instantiate(plusEffectPrefab, spawnPosition, Quaternion.identity);

                // Parent to the beaver so it follows them if they move
                plusInstance.transform.SetParent(activeBeaver.transform);

                // Destroy the plus icon after the specified duration
                Destroy(plusInstance, plusDisplayDuration);
            }

            // 3. Destroy the Medkit prefab
            Destroy(gameObject);
        }
    }
}