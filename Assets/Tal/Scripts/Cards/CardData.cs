using UnityEngine;
using UnityEngine.EventSystems;

public class CardData : MonoBehaviour, IPointerClickHandler
{
    [Header("Spawns")]
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private GameObject traversalPrefab;

    [Header("Restrictions")]
    public bool isWeaponDisabled = false;
    public bool isTraversalDisabled = false;

    [Header("Card Info")]
    [SerializeField] AudioClip cardSelectionSound;

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
            if (isWeaponDisabled)
            {
                Debug.Log("Weapon activation is disabled for this card!");
                return;
            }

            SoundFXManager.instance.PlaySoundFXClip(cardSelectionSound, transform, 1f);

            handManager.PlayCard(this.gameObject, weaponPrefab);
        }
        // Right Click -> Traversal
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (isTraversalDisabled)
            {
                Debug.Log("Traversal activation is disabled for this card!");
                return;
            }

            SoundFXManager.instance.PlaySoundFXClip(cardSelectionSound, transform, 1f);

            handManager.PlayCard(this.gameObject, traversalPrefab);
        }
    }
}