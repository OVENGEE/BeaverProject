using UnityEngine;

public class DeathByWater : MonoBehaviour
{
    [Header("Water Hazard")]
    [SerializeField] private bool destroyPlayerOnTouch = true;
    [SerializeField] private bool destroyWaterAfterPlayerDies = false;

    // Called when a 3D collider enters the water trigger.
    private void OnTriggerEnter(Collider other)
    {
        HandlePlayerDeath(other.gameObject);
    }

    // Called when a 2D collider enters the water trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerDeath(other.gameObject);
    }

    // Called when a 3D object collides with the water volume.
    private void OnCollisionEnter(Collision collision)
    {
        HandlePlayerDeath(collision.gameObject);
    }

    // Called when a 2D object collides with the water volume.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerDeath(collision.gameObject);
    }

    // Destroys the player if it is tagged as Player, then optionally removes the water object.
    private void HandlePlayerDeath(GameObject other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (destroyPlayerOnTouch)
        {
            Destroy(other.gameObject);
        }
        else
        {
            // Add custom player death logic here if you do not want to destroy the object immediately.
            // Example:
            // other.GetComponent<PlayerHealth>()?.Die();
        }

        if (destroyWaterAfterPlayerDies)
        {
            Destroy(gameObject);
        }
    }
}

