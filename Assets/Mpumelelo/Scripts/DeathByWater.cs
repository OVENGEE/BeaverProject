using UnityEngine;

public class DeathByWater : MonoBehaviour
{
    [Header("Water Hazard")]
    [SerializeField] private bool destroyPlayerOnTouch = true;
    [SerializeField] private bool destroyWaterAfterPlayerDies = false;

    private void OnTriggerEnter(Collider other)
    {
        HandlePlayerDeath(other.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerDeath(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandlePlayerDeath(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerDeath(collision.gameObject);
    }

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
            // Replace this with your own player death logic if you have a health script.
            // Example:
            // other.GetComponent<PlayerHealth>()?.Die();
        }

        if (destroyWaterAfterPlayerDies)
        {
            Destroy(gameObject);
        }
    }
}

