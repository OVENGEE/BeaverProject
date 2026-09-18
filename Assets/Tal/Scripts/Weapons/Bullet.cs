using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;

    private void Start()
    {
        // Destroy automatically if it hits nothing
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Add impact effects/damage logic here later
        Destroy(gameObject);
    }
}