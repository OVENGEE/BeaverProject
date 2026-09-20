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

    [SerializeField, Min(0.1f)]
    private float m_squareRadius = 1f;

    // Looks for a terrain component when this object is reset in the inspector.
    private void Reset()
    {
        m_destructibleTerrain = GetComponent<DestructibleTerrain>();

        if (m_destructibleTerrain == null)
            m_destructibleTerrain = GetComponentInChildren<DestructibleTerrain>();
    }

    // Finds the terrain reference early so it is ready before gameplay begins.
    private void Awake()
    {
        if (m_destructibleTerrain == null)
            m_destructibleTerrain = GetComponent<DestructibleTerrain>();

        if (m_destructibleTerrain == null)
            m_destructibleTerrain = GetComponentInChildren<DestructibleTerrain>();
    }

    // Subscribes to the input action when the script is enabled.
    private void OnEnable()
    {
        if (m_clickInput != null)
            m_clickInput.action.performed += HandleClick;
    }

    // Checks for mouse clicks and destroys terrain at the pointer position.
    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (m_clickInput == null && Mouse.current.leftButton.wasPressedThisFrame)
            TryDestroyAtMouse(false);

        if (Mouse.current.rightButton.wasPressedThisFrame)
            TryDestroyAtMouse(true);
    }

    // Handles the input action callback for left-click destruction.
    private void HandleClick(InputAction.CallbackContext obj)
    {
        TryDestroyAtMouse(false);
    }

    // Converts the pointer position to world space and removes terrain there.
    private void TryDestroyAtMouse(bool useRectangle)
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
        float destructionRadius = useRectangle ? m_squareRadius : m_radius;
        m_destructibleTerrain.RemoveTerrainAt(worldPosition, destructionRadius, useRectangle);

        if (m_explosionEffect != null)
            Instantiate(m_explosionEffect, worldPosition, Quaternion.identity);
    }

    // Unsubscribes from the click action when the script is disabled.
    private void OnDisable()
    {
        if (m_clickInput != null)
            m_clickInput.action.performed -= HandleClick;
    }
}
