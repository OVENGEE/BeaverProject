using System.Collections.Generic;
using UnityEngine;

public class CardGrantButton : MonoBehaviour
{
    public enum CardRestriction
    {
        None,
        DisableTraversal, // Disables Right-Click action
        DisableWeapon     // Disables Left-Click action
    }

    [Header("Card Grant Settings")]
    [SerializeField] private List<GameObject> cardPrefabs = new List<GameObject>();
    [SerializeField] private CardHandManager handManager; // Optional: auto-finds if unassigned
    [SerializeField] private CardRestriction restriction = CardRestriction.None;
    [SerializeField] private bool singleUse = true;
    [SerializeField] private float grantCooldown = 2f; // Cooldown between card grants

    [Header("Button Movement Settings")]
    [SerializeField] private float buttonLowerDistance = 0.2f;
    [SerializeField] private float buttonSpeed = 3f;

    private Vector3 buttonInitialPos;
    private Vector3 buttonPressedPos;
    private bool isPressed;
    private bool hasGrantedCard;
    private float lastGrantTime = -999f;
    private HashSet<Collider2D> occupyingColliders = new HashSet<Collider2D>();

    private void Start()
    {
        buttonInitialPos = transform.position;
        buttonPressedPos = buttonInitialPos + (Vector3.down * buttonLowerDistance);

        if (handManager == null)
        {
            handManager = FindObjectOfType<CardHandManager>(true);
        }
    }

    private void Update()
    {
        Vector3 targetPos = isPressed ? buttonPressedPos : buttonInitialPos;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, buttonSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BeaverController beaver = other.GetComponentInParent<BeaverController>();
        if (beaver != null)
        {
            occupyingColliders.Add(other);
            isPressed = true;

            // Ensure reference exists (including inactive objects) and activate GameObject
            if (handManager == null)
            {
                handManager = FindObjectOfType<CardHandManager>(true);
            }

            if (handManager != null)
            {
                handManager.gameObject.SetActive(true);
            }

            if (!hasGrantedCard || !singleUse)
            {
                // Check if cooldown duration has passed since the last card was granted
                if (Time.time - lastGrantTime >= grantCooldown)
                {
                    GrantCardWithRestriction();
                    hasGrantedCard = true;
                    lastGrantTime = Time.time;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (occupyingColliders.Contains(other))
        {
            occupyingColliders.Remove(other);

            if (occupyingColliders.Count == 0 && !singleUse)
            {
                isPressed = false;
            }
        }
    }

    private void GrantCardWithRestriction()
    {
        if (cardPrefabs == null || cardPrefabs.Count == 0)
        {
            Debug.LogWarning($"CardGrantButton on {gameObject.name} has no Card Prefabs assigned in the list!");
            return;
        }

        if (handManager == null)
        {
            handManager = FindObjectOfType<CardHandManager>(true);
        }

        if (handManager == null)
        {
            Debug.LogWarning("CardGrantButton: No CardHandManager found in scene!");
            return;
        }

        // Pick a random card prefab from the list
        int randomIndex = Random.Range(0, cardPrefabs.Count);
        GameObject chosenPrefab = cardPrefabs[randomIndex];

        if (chosenPrefab == null)
        {
            Debug.LogWarning($"CardGrantButton on {gameObject.name}: Prefab at index {randomIndex} is null!");
            return;
        }

        // Add selected card to hand
        handManager.AddCard(chosenPrefab);

        // Find the newly added card instance inside the hand
        CardData[] cardsInHand = handManager.GetComponentsInChildren<CardData>();
        if (cardsInHand.Length > 0)
        {
            CardData latestCard = cardsInHand[cardsInHand.Length - 1];

            // Apply chosen restriction to the card
            switch (restriction)
            {
                case CardRestriction.DisableTraversal:
                    latestCard.isTraversalDisabled = true;
                    latestCard.isWeaponDisabled = false;
                    break;

                case CardRestriction.DisableWeapon:
                    latestCard.isWeaponDisabled = true;
                    latestCard.isTraversalDisabled = false;
                    break;

                case CardRestriction.None:
                    latestCard.isWeaponDisabled = false;
                    latestCard.isTraversalDisabled = false;
                    break;
            }
        }
    }
}