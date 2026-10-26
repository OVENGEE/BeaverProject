using UnityEngine;

public class DeathByWater : MonoBehaviour
{
    private enum WaterRiseSchedule
    {
        EveryNRounds,
        EveryRoundFromSelectedRound
    }

    [Header("Water Hazard")]
    [SerializeField] private bool destroyBeaverOnTouch = true;
    [SerializeField] private bool destroyWaterAfterBeaverDies = false;

    [Header("Water Rising")]
    [SerializeField] private bool enableWaterRise;
    [SerializeField, Min(0f)] private float waterRiseDistance = 1f;
    [SerializeField] private WaterRiseSchedule waterRiseSchedule = WaterRiseSchedule.EveryNRounds;
    [SerializeField, Min(1)] private int roundsBetweenRises = 3;
    [SerializeField, Min(1)] private int firstConsecutiveRiseRound = 5;

    private void OnEnable()
    {
        TurnManager.RoundStarted += HandleRoundStarted;
    }

    private void OnDisable()
    {
        TurnManager.RoundStarted -= HandleRoundStarted;
    }

    private void HandleRoundStarted(int round)
    {
        if (!enableWaterRise || waterRiseDistance <= 0f)
            return;

        bool shouldRise = waterRiseSchedule switch
        {
            WaterRiseSchedule.EveryNRounds =>
                round % Mathf.Max(1, roundsBetweenRises) == 0,
            WaterRiseSchedule.EveryRoundFromSelectedRound =>
                round >= Mathf.Max(1, firstConsecutiveRiseRound),
            _ => false
        };

        if (shouldRise)
            transform.position += Vector3.up * waterRiseDistance;
    }

    // Called when a 3D collider enters the water trigger.
    private void OnTriggerEnter(Collider other)
    {
        HandleBeaverDeath(other.gameObject);
    }

    // Called when a 2D collider enters the water trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleBeaverDeath(other.gameObject);
    }

    // Called when a 3D object collides with the water volume.
    private void OnCollisionEnter(Collision collision)
    {
        HandleBeaverDeath(collision.gameObject);
    }

    // Called when a 2D object collides with the water volume.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleBeaverDeath(collision.gameObject);
    }

    // Destroys the Beaver if it is tagged as Beaver, then optionally removes the water object.
    private void HandleBeaverDeath(GameObject other)
    {
        if (!other.CompareTag("BeaverP1") && !other.CompareTag("BeaverP2"))
            return;

        if (destroyBeaverOnTouch)
        {
            Destroy(other.gameObject);
        }
        else
        {
            // Add custom Beaver death logic here if you do not want to destroy the object immediately.
            // Example:
            // other.GetComponent<BeaverHealth>()?.Die();
        }

        if (destroyWaterAfterBeaverDies)
        {
            Destroy(gameObject);
        }
    }
}
