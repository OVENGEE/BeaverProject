using System.Collections.Generic;
using UnityEngine;

public class ButtonDoorMechanism : MonoBehaviour
{
    [Header("Door Reference")]
    [SerializeField] private Transform doorTransform;

    [Header("Button Movement Settings")]
    [Tooltip("How far down the button depresses when stood on.")]
    [SerializeField] private float buttonLowerDistance = 0.3f;
    [SerializeField] private float buttonSpeed = 2f;

    [Header("Door Movement Settings")]
    [Tooltip("How far up the door rises when the button is activated.")]
    [SerializeField] private float doorRiseDistance = 3f;
    [SerializeField] private float doorSpeed = 2f;

    [Header("Behavior Settings")]
    [Tooltip("If checked, the door stays open permanently once pressed.")]
    [SerializeField] private bool stayPressed = false;

    private Vector3 buttonInitialPos;
    private Vector3 buttonPressedPos;
    private Vector3 doorInitialPos;
    private Vector3 doorOpenPos;

    private HashSet<Collider2D> occupyingColliders = new HashSet<Collider2D>();
    private bool isPressed;

    private void Start()
    {
        // Cache starting positions and calculate targets
        buttonInitialPos = transform.position;
        buttonPressedPos = buttonInitialPos + (Vector3.down * buttonLowerDistance);

        if (doorTransform != null)
        {
            doorInitialPos = doorTransform.position;
            doorOpenPos = doorInitialPos + (Vector3.up * doorRiseDistance);
        }
        else
        {
            Debug.LogWarning($"ButtonDoorMechanism on {gameObject.name} is missing a Door Transform reference!");
        }
    }

    private void Update()
    {
        // Determine active targets based on pressed state
        Vector3 targetButtonPos = isPressed ? buttonPressedPos : buttonInitialPos;
        Vector3 targetDoorPos = isPressed ? doorOpenPos : doorInitialPos;

        // Smoothly move button towards target
        transform.position = Vector3.MoveTowards(transform.position, targetButtonPos, buttonSpeed * Time.deltaTime);

        // Smoothly move door towards target
        if (doorTransform != null)
        {
            doorTransform.position = Vector3.MoveTowards(doorTransform.position, targetDoorPos, doorSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the entering object is a beaver
        BeaverController beaver = other.GetComponentInParent<BeaverController>();
        if (beaver != null)
        {
            occupyingColliders.Add(other);
            isPressed = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (occupyingColliders.Contains(other))
        {
            occupyingColliders.Remove(other);

            // Reset button/door if no colliders remain on the button and stayPressed is false
            if (occupyingColliders.Count == 0 && !stayPressed)
            {
                isPressed = false;
            }
        }
    }
}