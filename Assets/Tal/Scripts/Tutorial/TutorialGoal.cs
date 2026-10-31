using UnityEngine;

public class TutorialGoal : MonoBehaviour
{
    private bool triggerActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerActivated) return;

        // Verify if the collider belongs to the beaver
        BeaverController beaver = other.GetComponentInParent<BeaverController>();

        if (beaver != null)
        {
            triggerActivated = true;

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.CompleteTutorial();
            }
        }
    }
}