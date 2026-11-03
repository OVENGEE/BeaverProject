using UnityEngine;

public class CardDropItem : MonoBehaviour
{
    [SerializeField] private GameObject handCardPrefab;
    private bool collected;
    private CardDrops dropSpawner;

    public void Initialize(CardDrops spawner)
    {
        dropSpawner = spawner;
    }

    public void Collect(BeaverController beaver)
    {
        if (collected || beaver == null || handCardPrefab == null) return;
        if (TurnManager.Instance == null) return;

        collected = true;
        TurnManager.Instance.AwardCardToBeaver(beaver, handCardPrefab);
        dropSpawner?.ShowPickupMessage(beaver);
        Destroy(gameObject);
    }
}