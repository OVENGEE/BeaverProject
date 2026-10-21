using System.Collections;
using UnityEngine;
using TMPro;

public class BeaverHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private TextMeshPro healthText; // Use 3D TextMeshPro placed above the head
    [SerializeField] private TMP_Text[] additionalHealthTexts;
    [SerializeField] private GameObject logPrefab; // Spawned when beaver dies

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float deathDelay = 1.2f; // Time to allow the death animation to play

    private int currentHealth;
    private bool isDead = false;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

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

        if (additionalHealthTexts == null) return;

        float facing = Mathf.Sign(transform.localScale.x);
        foreach (TMP_Text additionalText in additionalHealthTexts)
        {
            if (additionalText != null)
            {
                Vector3 textScale = additionalText.transform.localScale;
                textScale.x = Mathf.Abs(textScale.x) * facing;
                additionalText.transform.localScale = textScale;
            }
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

    public void Heal(int healthIncrease)
    {
        // Don't heal if dead or already at/above max health
        if (isDead || currentHealth <= 0 || currentHealth >= maxHealth) return;

        // Increase health and clamp it so it never exceeds maxHealth
        currentHealth = Mathf.Min(currentHealth + healthIncrease, maxHealth);

        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString();
        }

        if (additionalHealthTexts == null) return;

        foreach (TMP_Text additionalText in additionalHealthTexts)
        {
            if (additionalText != null)
            {
                additionalText.text = currentHealth.ToString();
            }
        }
    }

    private void Die()
    {
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("Dead");
        }

        // Disable physics and colliders so the beaver doesn't move while dying
        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
        if (TryGetComponent<Rigidbody2D>(out var rb)) rb.simulated = false;
        if (healthText != null) healthText.gameObject.SetActive(false);
        if (additionalHealthTexts != null)
        {
            foreach (TMP_Text additionalText in additionalHealthTexts)
            {
                if (additionalText != null) additionalText.gameObject.SetActive(false);
            }
        }

        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        // Wait for the death animation to finish playing
        yield return new WaitForSeconds(deathDelay);

        // Spawn the log prefab at the beaver's final position
        if (logPrefab != null)
        {
            Instantiate(logPrefab, transform.position, Quaternion.identity);
        }

        // Destroy the beaver game object
        Destroy(gameObject);
    }
}