using UnityEngine;

public class CardDropItem : MonoBehaviour
{
    [SerializeField] private GameObject handCardPrefab;
    private bool collected;

    public void Collect(BeaverController beaver)
    {
        if (collected || beaver == null || handCardPrefab == null) return;
        if (TurnManager.Instance == null) return;

        collected = true;
        TurnManager.Instance.AwardCardToBeaver(beaver, handCardPrefab);
        Destroy(gameObject);
    }
}