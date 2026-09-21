using UnityEngine;
using TMPro;

public class BeaverHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private TextMeshPro healthText; // Use 3D TextMeshPro placed above the head
    [SerializeField] private GameObject logPrefab; // Spawned when beaver dies

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float deathDelay = 1.2f; // Time to allow the death animation to play

    private int currentHealth;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        if (animator == null) animator = GetComponentInChildren<Animator>();
        UpdateHealthUI();
    }

    private void LateUpdate()
    {
        if (healthText != null)
        {
            float parentFacing = Mathf.Sign(transform.localScale.x);
            healthText.transform.localScale = new Vector3(parentFacing, 1f, 1f);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString();
        }
    }

    private void Die()
    {
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("Dead");
        }

        if (logPrefab != null)
        {
            Instantiate(logPrefab, transform.position, Quaternion.identity);
        }

        // Disable physics and colliders so the beaver doesn't move while dying
        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
        if (TryGetComponent<Rigidbody2D>(out var rb)) rb.simulated = false;
        if (healthText != null) healthText.gameObject.SetActive(false);

        Destroy(gameObject, deathDelay);
    }
}