using UnityEngine;

public class DeathByWater : MonoBehaviour
{
    [Header("Water Hazard")]
    [SerializeField] private bool destroyBeaverOnTouch = true;
    [SerializeField] private bool destroyWaterAfterBeaverDies = false;

    // Called when a 3D collider enters the water trigger.
    private void OnTriggerEnter(Collider other)
    {
        HandleBeaverDeath(other.gameObject);
    }

    // Called when a 2D collider enters the water trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleBeaverDeath(other.gameObject);
    }

    // Called when a 3D object collides with the water volume.
    private void OnCollisionEnter(Collision collision)
    {
        HandleBeaverDeath(collision.gameObject);
    }

    // Called when a 2D object collides with the water volume.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleBeaverDeath(collision.gameObject);
    }

    // Destroys the Beaver if it is tagged as Beaver, then optionally removes the water object.
    private void HandleBeaverDeath(GameObject other)
    {
        if (!other.CompareTag("Beaver"))
            return;

        if (destroyBeaverOnTouch)
        {
            Destroy(other.gameObject);
        }
        else
        {
            // Add custom Beaver death logic here if you do not want to destroy the object immediately.
            // Example:
            // other.GetComponent<BeaverHealth>()?.Die();
        }

        if (destroyWaterAfterBeaverDies)
        {
            Destroy(gameObject);
        }
    }
}

