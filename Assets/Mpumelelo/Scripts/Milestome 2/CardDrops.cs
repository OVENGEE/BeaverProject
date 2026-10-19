using UnityEngine;

public class CardDrops : MonoBehaviour
{
    [Header("Drop Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject[] cardPrefab;
    [SerializeField] private float dropHeight = 6f;

    private GameObject currentDrop;

    private void Start()
    {
        SpawnDrop();
    }

    public void SpawnDrop()
    {
        if (cardPrefab == null || cardPrefab.Length == 0)
        {
            Debug.LogWarning("No card prefabs assigned to CardDrops.");
            return;
        }

        if (currentDrop != null)
        {
            Destroy(currentDrop);
        }

        Vector3 spawnPosition = transform.position;
        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position;
        }

        spawnPosition += Vector3.up * dropHeight;

        GameObject selectedPrefab = cardPrefab[Random.Range(0, cardPrefab.Length)];
        currentDrop = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        currentDrop.name = "DroppedCard";
        currentDrop.tag = "Card";

        Rigidbody2D rb = currentDrop.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = currentDrop.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 1f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Collider2D mainCollider = currentDrop.GetComponent<Collider2D>();
        if (mainCollider == null)
        {
            mainCollider = currentDrop.AddComponent<BoxCollider2D>();
        }

        if (mainCollider.isTrigger)
        {
            mainCollider.isTrigger = false;
        }

        GameObject pickupTrigger = new GameObject("CardPickupTrigger");
        pickupTrigger.transform.SetParent(currentDrop.transform, false);
        pickupTrigger.transform.localPosition = Vector3.zero;
        pickupTrigger.tag = "Card";

        BoxCollider2D triggerCollider = pickupTrigger.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector2(0.8f, 0.8f);

        Debug.Log("Card dropped from sky.");
    }
}
