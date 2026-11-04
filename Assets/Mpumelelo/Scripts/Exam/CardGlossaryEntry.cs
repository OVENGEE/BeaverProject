using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public sealed class CardGlossaryUse
{
    [SerializeField] private string label;
    [SerializeField] private string category;
    [SerializeField] private string description;
    [SerializeField] private string then;

    public string Label => label;
    public string Category => category;
    public string Description => description;
    public string Then => then;
}

[RequireComponent(typeof(Image))]
public class CardGlossaryEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Appearance")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField, Min(1f)] private float hoverScale = 1.08f;
    [SerializeField, Min(0f)] private float transitionSpeed = 14f;

    [Header("Glossary Content")]
    [SerializeField] private CardGlossaryController glossary;
    [SerializeField] private string cardName;
    [SerializeField] private CardGlossaryUse leftClick = new CardGlossaryUse();
    [SerializeField] private CardGlossaryUse rightClick = new CardGlossaryUse();

    private Vector3 originalScale;
    private int originalSiblingIndex;
    private Coroutine scaleCoroutine;
    private bool isHovered;

    public Sprite NormalSprite => normalSprite;
    public Sprite HoverSprite => hoverSprite;
    public string CardName => cardName;
    public CardGlossaryUse LeftClick => leftClick;
    public CardGlossaryUse RightClick => rightClick;

    private void Awake()
    {
        if (cardImage == null)
        {
            cardImage = GetComponent<Image>();
        }

        cardImage.raycastTarget = true;
        originalScale = transform.localScale;
        originalSiblingIndex = transform.GetSiblingIndex();

        if (normalSprite != null)
        {
            cardImage.sprite = normalSprite;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovered)
        {
            return;
        }

        isHovered = true;
        originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();

        if (hoverSprite != null)
        {
            cardImage.sprite = hoverSprite;
        }

        AnimateTo(originalScale * hoverScale);

        if (glossary != null)
        {
            glossary.ShowCard(this);
        }
        else
        {
            Debug.LogError($"Assign a Card Glossary Controller to {gameObject.name}.", this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetHover();
    }

    private void OnDisable()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }

        if (isHovered)
        {
            transform.SetSiblingIndex(originalSiblingIndex);
            isHovered = false;
        }

        if (cardImage != null && normalSprite != null)
        {
            cardImage.sprite = normalSprite;
        }

        transform.localScale = originalScale;
    }

    private void ResetHover()
    {
        if (cardImage == null)
        {
            return;
        }

        if (isHovered)
        {
            transform.SetSiblingIndex(originalSiblingIndex);
            isHovered = false;
        }

        if (normalSprite != null)
        {
            cardImage.sprite = normalSprite;
        }

        AnimateTo(originalScale);
    }

    private void AnimateTo(Vector3 targetScale)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }

        scaleCoroutine = StartCoroutine(ScaleTo(targetScale));
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        while ((transform.localScale - targetScale).sqrMagnitude > 0.0001f)
        {
            float blend = 1f - Mathf.Exp(-transitionSpeed * Time.unscaledDeltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, blend);
            yield return null;
        }

        transform.localScale = targetScale;
        scaleCoroutine = null;
    }
}
