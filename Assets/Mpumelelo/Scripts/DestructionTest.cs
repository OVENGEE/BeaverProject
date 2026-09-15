using UnityEngine;
using UnityEngine.InputSystem;

public class DestructionTest : MonoBehaviour
{
    [SerializeField]
    private InputActionReference m_pointerInput;

    [SerializeField]
    private InputActionReference m_clickInput;

    [SerializeField]
    private DestructibleTerrain m_destructibleTerrain;

    [SerializeField]
    private GameObject m_explosionEffect;

    [SerializeField, Min(0.1f)]
    private float m_radius = 1f;

    private void Reset()
    {
        m_destructibleTerrain = GetComponent<DestructibleTerrain>();

        if (m_destructibleTerrain == null)
            m_destructibleTerrain = GetComponentInChildren<DestructibleTerrain>();
    }

    private void Awake()
    {
        if (m_destructibleTerrain == null)
            m_destructibleTerrain = GetComponent<DestructibleTerrain>();

        if (m_destructibleTerrain == null)
            m_destructibleTerrain = GetComponentInChildren<DestructibleTerrain>();
    }

    private void OnEnable()
    {
        if (m_clickInput != null)
            m_clickInput.action.performed += HandleClick;
    }

    private void Update()
    {
        if (m_clickInput == null && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TryDestroyAtMouse();
    }

    private void HandleClick(InputAction.CallbackContext obj)
    {
        TryDestroyAtMouse();
    }

    private void TryDestroyAtMouse()
    {
        if (m_destructibleTerrain == null)
        {
            Debug.LogWarning("No DestructibleTerrain assigned or found.");
            return;
        }

        Vector2 mousePosition = m_pointerInput != null
            ? m_pointerInput.action.ReadValue<Vector2>()
            : Mouse.current.position.ReadValue();

        if (Camera.main == null)
        {
            Debug.LogWarning("No main camera found for terrain destruction.");
            return;
        }

        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        m_destructibleTerrain.RemoveTerrainAt(worldPosition, m_radius);

        if (m_explosionEffect != null)
            Instantiate(m_explosionEffect, worldPosition, Quaternion.identity);
    }

    private void OnDisable()
    {
        if (m_clickInput != null)
            m_clickInput.action.performed -= HandleClick;
    }
}
