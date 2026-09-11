using System.Collections.Generic;
using UnityEngine;

public class CardHandManager : MonoBehaviour
{
    [Header("Card Setup")]
    // Replaced the single prefab with a List of prefabs
    [SerializeField] private List<GameObject> cardPrefabs;

    [Header("Curve Points (RectTransforms)")]
    [SerializeField] private RectTransform startPoint;
    [SerializeField] private RectTransform controlPoint; // The peak of the curve
    [SerializeField] private RectTransform endPoint;

    [Header("Fan Settings")]
    [SerializeField] private float maxRotationAngle = 15f; // How much the outer cards tilt

    private List<GameObject> cardsInHand = new List<GameObject>();

    // Called by your UI Button
    public void AddCard()
    {
        // Safety check to ensure the list isn't empty
        if (cardPrefabs == null || cardPrefabs.Count == 0)
        {
            Debug.LogWarning("No card prefabs assigned in the inspector!");
            return;
        }

        // Pick a random index between 0 and the number of prefabs
        int randomIndex = Random.Range(0, cardPrefabs.Count);
        GameObject selectedPrefab = cardPrefabs[randomIndex];

        // Instantiate the randomly selected card
        GameObject newCard = Instantiate(selectedPrefab, transform);
        cardsInHand.Add(newCard);
        ArrangeCards();
    }

    private void ArrangeCards()
    {
        int cardCount = cardsInHand.Count;
        if (cardCount == 0) return;

        for (int i = 0; i < cardCount; i++)
        {
            // Get a value from 0 to 1 representing position along the curve
            // If only 1 card, place it in the middle (0.5f)
            float t = (cardCount == 1) ? 0.5f : (float)i / (cardCount - 1);

            // Calculate Position
            Vector3 targetPosition = CalculateQuadraticBezierPoint(t, startPoint.position, controlPoint.position, endPoint.position);
            cardsInHand[i].transform.position = targetPosition;

            // Calculate Rotation
            // Map t (0 to 1) to an angle (-maxRotation to +maxRotation)
            float angle = Mathf.Lerp(maxRotationAngle, -maxRotationAngle, t);
            cardsInHand[i].transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // Mathematical formula for a curved spline
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