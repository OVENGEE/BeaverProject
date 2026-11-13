using System.Collections;
using TMPro;
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
    [SerializeField] private TextMeshProUGUI waterRiseInfoText;
    [SerializeField, Min(0f)] private float infoDisplayDuration = 2f;
    [SerializeField, Min(0.1f)] private float fadeDuration = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip beaverSplashSound;
    [SerializeField] private AudioClip waterRiseSound;

    private Coroutine hideTextCoroutine;

    private void Start()
    {
        if (waterRiseInfoText != null)
        {
            waterRiseInfoText.gameObject.SetActive(false);
        }
    }

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
        {
            transform.position += Vector3.up * waterRiseDistance;

            if (SoundFXManager.instance != null && waterRiseSound != null)
            {
                SoundFXManager.instance.PlaySoundFXClip(waterRiseSound, transform, 0.6f);
            }

            ShowWaterRiseText();
        }
    }

    private void ShowWaterRiseText()
    {
        if (waterRiseInfoText == null) return;

        if (hideTextCoroutine != null)
        {
            StopCoroutine(hideTextCoroutine);
        }

        hideTextCoroutine = StartCoroutine(DisplayTextRoutine());
    }

    private IEnumerator DisplayTextRoutine()
    {
        // Reset alpha to full visibility and activate
        waterRiseInfoText.alpha = 1f;
        waterRiseInfoText.gameObject.SetActive(true);

        // Hold at full opacity before fading
        yield return new WaitForSeconds(infoDisplayDuration);

        // Smoothly fade alpha from 1 to 0
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            waterRiseInfoText.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        waterRiseInfoText.alpha = 0f;
        waterRiseInfoText.gameObject.SetActive(false);
        hideTextCoroutine = null;
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
            SoundFXManager.instance.PlaySoundFXClip(beaverSplashSound, transform, 0.6f);
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