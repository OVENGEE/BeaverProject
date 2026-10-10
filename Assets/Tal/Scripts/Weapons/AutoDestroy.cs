using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Tooltip("Time in seconds before the explosion effect destroys itself.")]
    [SerializeField] private float lifetime = 2f;

    private void Start()
    {
        // Destroys the GameObject this script is attached to after 'lifetime' seconds
        Destroy(gameObject, lifetime);
    }
}