using UnityEngine;
using UnityEngine.EventSystems;

public class CardData : MonoBehaviour, IPointerClickHandler
{
    [Header("Spawns")]
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private GameObject traversalPrefab;

    private CardHandManager handManager;

    // Called by the Hand Manager when instantiated
    public void Initialize(CardHandManager manager)
    {
        handManager = manager;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Left Click -> Weapon
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            handManager.PlayCard(this.gameObject, weaponPrefab);
        }
        // Right Click -> Traversal
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            handManager.PlayCard(this.gameObject, traversalPrefab);
        }
    }
}