using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardGlossaryController : MonoBehaviour
{
    [Header("Featured Card")]
    [SerializeField] private Image featuredCardImage;
    [SerializeField] private TMP_Text featuredCardName;

    [Header("Left Click Details")]
    [SerializeField] private TMP_Text leftClickLabel;
    [SerializeField] private TMP_Text leftClickCategory;
    [SerializeField] private TMP_Text leftClickDescription;
    [SerializeField] private TMP_Text leftClickThen;

    [Header("Right Click Details")]
    [SerializeField] private TMP_Text rightClickLabel;
    [SerializeField] private TMP_Text rightClickCategory;
    [SerializeField] private TMP_Text rightClickDescription;
    [SerializeField] private TMP_Text rightClickThen;

    public void ShowCard(CardGlossaryEntry card)
    {
        if (card == null)
        {
            Debug.LogError("Cannot show a null card in the glossary.", this);
            return;
        }

        Sprite featuredSprite = card.HoverSprite != null ? card.HoverSprite : card.NormalSprite;
        if (featuredCardImage != null)
        {
            featuredCardImage.sprite = featuredSprite;
        }
        else
        {
            Debug.LogError("Assign the Featured Card Image on the Card Glossary Controller.", this);
        }

        SetText(featuredCardName, card.CardName);
        SetText(leftClickLabel, card.LeftClick.Label);
        SetText(leftClickCategory, card.LeftClick.Category);
        SetText(leftClickDescription, card.LeftClick.Description);
        SetText(leftClickThen, card.LeftClick.Then);
        SetText(rightClickLabel, card.RightClick.Label);
        SetText(rightClickCategory, card.RightClick.Category);
        SetText(rightClickDescription, card.RightClick.Description);
        SetText(rightClickThen, card.RightClick.Then);
    }

    private static void SetText(TMP_Text label, string value)
    {
        if (label != null)
        {
            label.text = value;
        }
    }
}
