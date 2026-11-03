using System.Collections.Generic;
using UnityEngine;

public class CardHandManager : MonoBehaviour
{
    [Header("Hand Settings")]
    [SerializeField] private int maxHandSize = 5;

    [Header("Card Rarity Pools")]
    [SerializeField] private List<GameObject> commonCardPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> rareCardPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> epicCardPrefabs = new List<GameObject>();

    [Header("Curve Points (RectTransforms)")]
    [SerializeField] private RectTransform startPoint;
    [SerializeField] private RectTransform controlPoint;
    [SerializeField] private RectTransform endPoint;

    [Header("Fan Settings")]
    [SerializeField] private float maxRotationAngle = 15f;

    private List<GameObject> cardsInHand = new List<GameObject>();

    public void AddCard()
    {
        // Check if hand capacity is reached before rolling
        if (cardsInHand.Count >= maxHandSize)
        {
            Debug.LogWarning($"Hand is full! Maximum hand size is {maxHandSize}.");
            return;
        }

        // Roll a value between 0 and 100
        float roll = Random.Range(0f, 100f);
        List<GameObject> selectedPool = null;

        if (roll < 60f) // 60% Chance: Common
        {
            selectedPool = GetValidPool(commonCardPrefabs, rareCardPrefabs, epicCardPrefabs);
        }
        else if (roll < 90f) // 30% Chance: Rare (60 to 90)
        {
            selectedPool = GetValidPool(rareCardPrefabs, commonCardPrefabs, epicCardPrefabs);
        }
        else // 10% Chance: Epic (90 to 100)
        {
            selectedPool = GetValidPool(epicCardPrefabs, rareCardPrefabs, commonCardPrefabs);
        }

        if (selectedPool == null || selectedPool.Count == 0)
        {
            Debug.LogWarning("No card prefabs assigned in any rarity pool!");
            return;
        }

        int randomIndex = Random.Range(0, selectedPool.Count);
        GameObject selectedPrefab = selectedPool[randomIndex];

        AddCard(selectedPrefab);
    }

    // Helper method to fall back to other pools if the preferred rarity pool is empty
    private List<GameObject> GetValidPool(List<GameObject> primary, List<GameObject> fallback1, List<GameObject> fallback2)
    {
        if (primary != null && primary.Count > 0) return primary;
        if (fallback1 != null && fallback1.Count > 0) return fallback1;
        if (fallback2 != null && fallback2.Count > 0) return fallback2;
        return null;
    }

    public void AddCard(GameObject selectedPrefab)
    {
        if (selectedPrefab == null) return;

        // Prevent adding cards if the hand is full
        if (cardsInHand.Count >= maxHandSize)
        {
            Debug.LogWarning($"Hand is full! Cannot add card. Maximum hand size is {maxHandSize}.");
            return;
        }

        GameObject newCard = Instantiate(selectedPrefab, transform);
        CardData cardData = newCard.GetComponent<CardData>();

        if (cardData == null)
        {
            Debug.LogWarning("The hand card prefab is missing CardData.");
            Destroy(newCard);
            return;
        }

        cardData.Initialize(this);
        cardsInHand.Add(newCard);
        ArrangeCards();
    }

    // Called by the CardData script when clicked
    public void PlayCard(GameObject playedCard, GameObject prefabToSpawn)
    {
        // 1. Fetch active beaver from TurnManager, or fallback to TutorialManager
        BeaverController activeBeaver = null;
        if (TurnManager.Instance != null && TurnManager.Instance.ActiveBeaver != null)
        {
            activeBeaver = TurnManager.Instance.ActiveBeaver;
        }
        else if (TutorialManager.Instance != null && TutorialManager.Instance.TutorialBeaver != null)
        {
            activeBeaver = TutorialManager.Instance.TutorialBeaver;
        }

        if (prefabToSpawn != null && activeBeaver != null)
        {
            // 2. Spawn the weapon or traversal as a child of the active beaver
            GameObject spawnedEquipment = Instantiate(prefabToSpawn, activeBeaver.transform);

            // 3. Check for the team swapper and apply the correct sprite
            if (spawnedEquipment.TryGetComponent<TeamSpriteSwapper>(out var swapper))
            {
                bool isPlayerTwo = activeBeaver.gameObject.CompareTag("BeaverP2");
                swapper.ApplyTeamSprite(isPlayerTwo);
            }

            // 4. Tell TurnManager to auto-end turn when this item is destroyed (only if TurnManager exists)
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.RegisterEquipment(spawnedEquipment);
            }
        }
        else
        {
            Debug.LogWarning("Missing prefab to spawn or Beaver transform is not assigned!");
        }

        // Remove from list and destroy the UI card
        cardsInHand.Remove(playedCard);
        Destroy(playedCard);
        ArrangeCards();

        // HIDE THE HAND so the player cannot click any more cards this turn
        gameObject.SetActive(false);
    }

    private void ArrangeCards()
    {
        int cardCount = cardsInHand.Count;
        if (cardCount == 0) return;

        for (int i = 0; i < cardCount; i++)
        {
            float t = (cardCount == 1) ? 0.5f : (float)i / (cardCount - 1);

            Vector3 targetPosition = CalculateQuadraticBezierPoint(t, startPoint.position, controlPoint.position, endPoint.position);
            cardsInHand[i].transform.position = targetPosition;

            float angle = Mathf.Lerp(maxRotationAngle, -maxRotationAngle, t);
            cardsInHand[i].transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void ClearHand()
    {
        foreach (GameObject card in new List<GameObject>(cardsInHand))
        {
            Destroy(card);
        }
        cardsInHand.Clear();
    }

    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;

        return p;
    }
}