using UnityEngine;

public class TeamSpriteSwapper : MonoBehaviour
{
    [Header("Team Sprites")]
    [Tooltip("Leave this blank if you already assigned the P1 sprite in the SpriteRenderer")]
    [SerializeField] private Sprite p1Sprite;
    [SerializeField] private Sprite p2Sprite;

    public void ApplyTeamSprite(bool isPlayerTwo)
    {
        // Find the child named "Sprite"
        Transform spriteChild = transform.Find("Sprite");

        if (spriteChild != null && spriteChild.TryGetComponent<SpriteRenderer>(out var sr))
        {
            if (isPlayerTwo && p2Sprite != null)
            {
                sr.sprite = p2Sprite;
            }
            else if (!isPlayerTwo && p1Sprite != null)
            {
                sr.sprite = p1Sprite;
            }
        }
        else
        {
            Debug.LogWarning($"No child named 'Sprite' with a SpriteRenderer found on {gameObject.name}.");
        }
    }
}