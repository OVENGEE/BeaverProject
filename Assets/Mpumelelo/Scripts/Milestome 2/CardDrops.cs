using System.Collections;
using TMPro;
using UnityEngine;

public class CardDrops : MonoBehaviour
{
    [Header("Drop Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject[] cardPrefab;
    [SerializeField] private float dropHeight = 6f;
    [SerializeField] private float dropDelay = 5f;

    [Header("Drop Message")]
    [SerializeField] private TextMeshProUGUI dropMessage;
    [SerializeField] private string dropMessageText = "A card has dropped!";
    [SerializeField] private float messageVisibleDuration = 1f;
    [SerializeField] private float messageFadeDuration = 1f;

    private GameObject currentDrop;
    private Coroutine messageCoroutine;
    private Coroutine dropCoroutine;

    private void Awake()
    {
        if (dropMessage == null) return;

        Color color = dropMessage.color;
        color.a = 0f;
        dropMessage.color = color;
        dropMessage.gameObject.SetActive(false);
    }

    public void SpawnDrop()
    {
        if (cardPrefab == null || cardPrefab.Length == 0)
        {
            Debug.LogWarning("No card prefabs assigned to CardDrops.");
            return;
        }

        if (dropCoroutine != null)
        {
            StopCoroutine(dropCoroutine);
        }

        if (currentDrop != null)
        {
            Destroy(currentDrop);
            currentDrop = null;
        }

        dropCoroutine = StartCoroutine(SpawnAfterDelay());
    }

    private IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSeconds(dropDelay);
        dropCoroutine = null;
        SpawnDropNow();
    }

    private void SpawnDropNow()
    {
        float minX = -25f;
        float maxX = 25f;

        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(
            randomX,
            transform.position.y,
            transform.position.z
        );

        spawnPosition += Vector3.up * dropHeight;

        GameObject selectedPrefab = cardPrefab[Random.Range(0, cardPrefab.Length)];
        currentDrop = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        currentDrop.name = "DroppedCard";
        currentDrop.tag = "Card";

        CardDropItem dropItem = currentDrop.GetComponentInChildren<CardDropItem>();
        if (dropItem != null)
        {
            dropItem.Initialize(this);
        }

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

        BeaverController[] beavers = FindObjectsByType<BeaverController>(FindObjectsInactive.Include);
        foreach (BeaverController beaver in beavers)
        {
            Collider2D[] beaverColliders = beaver.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D beaverCollider in beaverColliders)
            {
                Physics2D.IgnoreCollision(mainCollider, beaverCollider);
            }
        }

        GameObject pickupTrigger = new GameObject("CardPickupTrigger");
        pickupTrigger.transform.SetParent(currentDrop.transform, false);
        pickupTrigger.transform.localPosition = Vector3.zero;
        pickupTrigger.tag = "Card";

        BoxCollider2D triggerCollider = pickupTrigger.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector2(0.8f, 0.8f);

        ShowDropMessage();
        Debug.Log("Card dropped from sky.");
    }

    public void ShowPickupMessage(BeaverController beaver)
    {
        if (beaver == null) return;

        string playerName = beaver.CompareTag("BeaverP1") ? "Player 1" :
            beaver.CompareTag("BeaverP2") ? "Player 2" : beaver.name;
        ShowMessage($"{playerName} picked up a card!");
    }

    private void ShowDropMessage()
    {
        ShowMessage(dropMessageText);
    }

    private void ShowMessage(string message)
    {
        if (dropMessage == null) return;

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        dropMessage.text = message;
        Color color = dropMessage.color;
        color.a = 1f;
        dropMessage.color = color;
        dropMessage.gameObject.SetActive(true);
        messageCoroutine = StartCoroutine(FadeDropMessage());
    }

    private IEnumerator FadeDropMessage()
    {
        yield return new WaitForSecondsRealtime(messageVisibleDuration);

        float elapsed = 0f;
        Color color = dropMessage.color;
        while (elapsed < messageFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / messageFadeDuration);
            dropMessage.color = color;
            yield return null;
        }

        color.a = 0f;
        dropMessage.color = color;
        dropMessage.gameObject.SetActive(false);
        messageCoroutine = null;
    }
}
