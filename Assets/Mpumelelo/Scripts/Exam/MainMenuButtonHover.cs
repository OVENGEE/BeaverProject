using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Min(1f)] private float hoverScale = 1.06f;
    [SerializeField, Min(0f)] private float transitionSpeed = 14f;

    private Vector3 normalScale;
    private bool isHovered;

    private void Awake()
    {
        normalScale = transform.localScale;
    }

    private void OnDisable()
    {
        isHovered = false;
        transform.localScale = normalScale;
    }

    private void Update()
    {
        Vector3 targetScale = isHovered ? normalScale * hoverScale : normalScale;
        float blend = 1f - Mathf.Exp(-transitionSpeed * Time.unscaledDeltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, blend);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}
