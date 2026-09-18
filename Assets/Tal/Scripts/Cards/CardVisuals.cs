using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardVisuals : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private float transitionSpeed = 10f;
    [SerializeField] private GameObject backglowObject; // Assign a UI image child object here

    private Vector3 originalScale;
    private int originalSiblingIndex;
    private Coroutine scaleCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
        if (backglowObject != null) backglowObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Bring card to the front of the UI so it doesn't clip behind other cards
        originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();

        if (backglowObject != null) backglowObject.SetActive(true);

        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleCard(originalScale * hoverScale));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Put card back in its original order
        transform.SetSiblingIndex(originalSiblingIndex);

        if (backglowObject != null) backglowObject.SetActive(false);

        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleCard(originalScale));
    }

    private IEnumerator ScaleCard(Vector3 targetScale)
    {
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}