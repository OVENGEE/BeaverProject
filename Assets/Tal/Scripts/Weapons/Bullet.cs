using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 15; // Set damage per pellet/bullet

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Deal damage if collision target has health
        if (collision.gameObject.TryGetComponent<BeaverHealth>(out var health))
        {
            health.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}