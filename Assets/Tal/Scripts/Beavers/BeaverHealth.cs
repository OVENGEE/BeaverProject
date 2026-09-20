using UnityEngine;
using TMPro;

public class BeaverHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private TextMeshPro healthText; // Use 3D TextMeshPro placed above the head
    [SerializeField] private GameObject logPrefab; // Spawned when beaver dies

    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void LateUpdate()
    {
        // Counteract beaver scale flips so the text never renders backward
        if (healthText != null)
        {
            float parentFacing = Mathf.Sign(transform.localScale.x);
            healthText.transform.localScale = new Vector3(parentFacing, 1f, 1f);
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

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
        if (logPrefab != null)
        {
            Instantiate(logPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}