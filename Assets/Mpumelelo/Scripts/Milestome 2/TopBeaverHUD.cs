using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopBeaverHUD : MonoBehaviour
{
    [Serializable]
    private class BeaverSlot
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Image activeRing;
        [SerializeField] private GameObject deathX;

        private Image healthFill;
        private bool missingHealthWarningLogged;
        private bool missingFillWarningLogged;
        private int cachedMaxHealth = -1;
        private int cachedHealthTextValue = int.MinValue;

        public void Refresh(BeaverController beaver, BeaverController activeBeaver, Color green, Color yellow, Color red)
        {
            bool isDead = beaver == null;
            BeaverHealth health = beaver != null ? beaver.GetComponent<BeaverHealth>() : null;

            if (health != null)
            {
                isDead = health.IsDead;
                int maxHealth = Mathf.Max(1, health.MaxHealth);
                int currentHealth = Mathf.Clamp(health.CurrentHealth, 0, maxHealth);

                if (healthSlider != null)
                {
                    if (cachedMaxHealth != maxHealth)
                    {
                        healthSlider.minValue = 0f;
                        healthSlider.maxValue = maxHealth;
                        cachedMaxHealth = maxHealth;
                    }

                    healthSlider.SetValueWithoutNotify(currentHealth);
                    UpdateHealthColor(currentHealth / (float)maxHealth, green, yellow, red);
                }

                UpdateHealthText(currentHealth);
            }
            else
            {
                if (beaver != null && !missingHealthWarningLogged)
                {
                    Debug.LogWarning($"TopBeaverHUD: {beaver.name} is missing its BeaverHealth component.");
                    missingHealthWarningLogged = true;
                }

                if (healthSlider != null)
                {
                    healthSlider.SetValueWithoutNotify(0f);
                    UpdateHealthColor(0f, green, yellow, red);
                }

                UpdateHealthText(0);
            }

            if (activeRing != null)
            {
                activeRing.gameObject.SetActive(!isDead && beaver == activeBeaver);
            }

            if (deathX != null)
            {
                deathX.SetActive(isDead);
            }
        }

        private void UpdateHealthText(int currentHealth)
        {
            if (healthText == null || cachedHealthTextValue == currentHealth) return;

            healthText.text = currentHealth.ToString();
            cachedHealthTextValue = currentHealth;
        }

        private void UpdateHealthColor(float healthRatio, Color green, Color yellow, Color red)
        {
            if (healthSlider == null || healthSlider.fillRect == null) return;

            if (healthFill == null)
            {
                healthFill = healthSlider.fillRect.GetComponent<Image>();
                if (healthFill == null && !missingFillWarningLogged)
                {
                    Debug.LogWarning("TopBeaverHUD: A health Slider's Fill child needs an Image component.");
                    missingFillWarningLogged = true;
                }
            }

            if (healthFill == null) return;

            healthRatio = Mathf.Clamp01(healthRatio);
            healthFill.color = healthRatio <= 0.5f
                ? Color.Lerp(red, yellow, healthRatio * 2f)
                : Color.Lerp(yellow, green, (healthRatio - 0.5f) * 2f);
        }
    }

    [Header("Turn Manager")]
    [SerializeField] private TurnManager turnManager;

    [Header("Player 1 slots (same order as TurnManager's Player 1 list)")]
    [SerializeField] private BeaverSlot[] player1Slots = new BeaverSlot[3];

    [Header("Player 2 slots (same order as TurnManager's Player 2 list)")]
    [SerializeField] private BeaverSlot[] player2Slots = new BeaverSlot[3];

    [Header("Health colors")]
    [SerializeField] private Color healthyColor = new Color(0.18f, 0.67f, 0.28f, 1f);
    [SerializeField] private Color halfHealthColor = new Color(1f, 0.82f, 0.12f, 1f);
    [SerializeField] private Color lowHealthColor = new Color(0.9f, 0.18f, 0.14f, 1f);

    private void Start()
    {
        if (turnManager == null)
        {
            turnManager = TurnManager.Instance;
        }

        if (turnManager == null)
        {
            Debug.LogError("TopBeaverHUD requires a TurnManager in the scene.");
            enabled = false;
            return;
        }

        ValidateSlotCounts();
        RefreshHUD();
    }

    private void Update()
    {
        if (turnManager != null)
        {
            RefreshHUD();
        }
    }

    private void RefreshHUD()
    {
        RefreshTeam(player1Slots, turnManager.Player1Beavers, turnManager.ActiveBeaver);
        RefreshTeam(player2Slots, turnManager.Player2Beavers, turnManager.ActiveBeaver);
    }

    private void RefreshTeam(BeaverSlot[] slots, List<BeaverController> beavers, BeaverController activeBeaver)
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            BeaverController beaver = beavers != null && i < beavers.Count ? beavers[i] : null;
            if (slots[i] != null)
            {
                slots[i].Refresh(beaver, activeBeaver, healthyColor, halfHealthColor, lowHealthColor);
            }
        }
    }

    private void ValidateSlotCounts()
    {
        ValidateSlotCount("Player 1", player1Slots, turnManager.Player1Beavers);
        ValidateSlotCount("Player 2", player2Slots, turnManager.Player2Beavers);
    }

    private static void ValidateSlotCount(
        string playerName,
        BeaverSlot[] slots,
        List<BeaverController> beavers)
    {
        int slotCount = slots != null ? slots.Length : 0;
        int beaverCount = beavers != null ? beavers.Count : 0;
        if (slotCount != beaverCount)
        {
            Debug.LogWarning($"TopBeaverHUD: {playerName} has {beaverCount} beavers but {slotCount} HUD slots. Match the slot count and beaver list order.");
        }
    }
}
