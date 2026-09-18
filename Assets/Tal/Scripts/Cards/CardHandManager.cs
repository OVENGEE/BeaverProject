using System.Collections.Generic;
using UnityEngine;

public class CardHandManager : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private Transform beaverTransform; // Drag the Beaver GameObject here

    [Header("Card Setup")]
    [SerializeField] private List<GameObject> cardPrefabs;

    [Header("Curve Points (RectTransforms)")]
    [SerializeField] private RectTransform startPoint;
    [SerializeField] private RectTransform controlPoint;
    [SerializeField] private RectTransform endPoint;

    [Header("Fan Settings")]
    [SerializeField] private float maxRotationAngle = 15f;

    private List<GameObject> cardsInHand = new List<GameObject>();

    public void AddCard()
    {
        if (cardPrefabs == null || cardPrefabs.Count == 0)
        {
            Debug.LogWarning("No card prefabs assigned in the inspector!");
            return;
        }

        int randomIndex = Random.Range(0, cardPrefabs.Count);
        GameObject selectedPrefab = cardPrefabs[randomIndex];

        GameObject newCard = Instantiate(selectedPrefab, transform);

        // Pass a reference of this manager to the card so it can call PlayCard()
        CardData cardData = newCard.GetComponent<CardData>();
        if (cardData != null)
        {
            cardData.Initialize(this);
        }
        else
        {
            Debug.LogWarning("Card prefab is missing the CardData script!");
        }

        cardsInHand.Add(newCard);
        ArrangeCards();
    }

    // Called by the CardData script when clicked
    public void PlayCard(GameObject playedCard, GameObject prefabToSpawn)
    {
        if (prefabToSpawn != null && beaverTransform != null)
        {
            // Spawn the weapon or traversal as a child of the beaver
            Instantiate(prefabToSpawn, beaverTransform);
        }
        else
        {
            Debug.LogWarning("Missing prefab to spawn or Beaver transform is not assigned!");
        }

        // Remove from list, destroy the UI card, and re-center the hand
        cardsInHand.Remove(playedCard);
        Destroy(playedCard);
        ArrangeCards();
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