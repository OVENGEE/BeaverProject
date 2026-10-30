using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Players & Beavers")]
    [SerializeField] private List<BeaverController> player1Beavers;
    [SerializeField] private List<BeaverController> player2Beavers;

    [Header("Card Drop System")]
    [SerializeField] private CardDrops cardDropSpawner;

    [Header("HUD")]
    [SerializeField] private CardHandManager player1Hand;
    [SerializeField] private CardHandManager player2Hand;
    [SerializeField] private int cardsPerRound = 3;

    [Header("Hand Refresh Message")]
    [SerializeField] private TextMeshProUGUI handRefreshedText;
    [SerializeField] private float handRefreshedDisplayDuration = 1.5f;
    [SerializeField] private float handRefreshedFadeDuration = 0.5f;

    [Header("Game UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI currentPlayerText;   // Text to display whose turn it is
    [SerializeField] private TextMeshProUGUI roundText;           // Text display for current round
    [SerializeField] private Image currentPlayerBackgroundImage;
    [SerializeField] private Color player1TurnBackgroundColor = new Color(1f, 0.55f, 0.1f, 1f);
    [SerializeField] private Color player2TurnBackgroundColor = new Color(0.1f, 0.72f, 0.95f, 1f);
    private BeaverRosterHUD beaverRosterHUD;

    [Header("Timer FX")]
    [SerializeField] private Color warningTimerColor = Color.red;  // Color when <= 5s
    [SerializeField] private float pulseSpeed = 10f;               // Speed of scale pulsation
    [SerializeField] private float pulseAmount = 0.25f;            // Max scale increase percentage

    [Header("Win Screen")]
    [SerializeField] private TextMeshProUGUI winText;              // Assign a UI Text element for "Player X Wins!"
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject menuButton;

    [Header("Turn Settings")]
    [SerializeField] private float turnDuration = 30f;

    public BeaverController ActiveBeaver { get; private set; }
    public List<BeaverController> Player1Beavers => player1Beavers;
    public List<BeaverController> Player2Beavers => player2Beavers;

    private int currentRound = 0;
    private int turnStep = 0; // 0 to 5 (6 total beaver turns per round)
    private bool firstPlayerIsP1 = true;

    private float timeRemaining;
    private bool isTurnRunning = false;
    private bool isGameOver = false;
    private GameObject activeEquipment;
    private Coroutine equipmentCoroutine;
    private Coroutine handRefreshedCoroutine;

    // Cache default visual state for timer text
    private Color defaultTimerColor;
    private Vector3 defaultTimerScale;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Time.timeScale = 1f; // Ensure time is unpaused at start
    }

    private void Start()
    {
        if (winText != null) winText.gameObject.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (menuButton != null) menuButton.SetActive(false);
        if (currentPlayerText != null) currentPlayerText.gameObject.SetActive(true);

        if (handRefreshedText != null)
        {
            Color c = handRefreshedText.color;
            c.a = 0f;
            handRefreshedText.color = c;
            handRefreshedText.gameObject.SetActive(false);
        }

        // Save original timer styling so we can reset it each turn
        if (timerText != null)
        {
            defaultTimerColor = timerText.color;
            defaultTimerScale = timerText.transform.localScale;
        }

        // Randomize who goes first at game start
        firstPlayerIsP1 = Random.value > 0.5f;
        StartNewRound();
    }

    private void Update()
    {
        if (beaverRosterHUD != null)
        {
            beaverRosterHUD.Refresh(player1Beavers, player2Beavers, ActiveBeaver);
        }

        if (isGameOver || !isTurnRunning) return;

        // Check if any team has been wiped out
        if (CheckWinCondition()) return;

        // If the current beaver is destroyed DURING its own turn (e.g. blew itself up)
        if (ActiveBeaver == null)
        {
            EndTurn();
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();

            // Check if timer is 5 seconds or lower
            if (timeRemaining <= 5f)
            {
                // Turn red
                timerText.color = warningTimerColor;

                // Rhythmically pulse size up and down
                float pulse = 1f + Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)) * pulseAmount;
                timerText.transform.localScale = defaultTimerScale * pulse;
            }
            else
            {
                ResetTimerVisuals();
            }
        }

        if (timeRemaining <= 0f)
        {
            EndTurn();
        }
    }

    public void StartNewRound()
    {
        if (CheckWinCondition()) return;

        currentRound++;
        turnStep = 0;

        // Update round text in HUD
        if (roundText != null)
        {
            roundText.text = $"Round {currentRound}";
        }

        // Populate fresh random hands for both players at the start of a round
        if (player1Hand != null) player1Hand.ClearHand();
        if (player2Hand != null) player2Hand.ClearHand();

        for (int i = 0; i < cardsPerRound; i++)
        {
            if (player1Hand != null) player1Hand.AddCard();
            if (player2Hand != null) player2Hand.AddCard();
        }

        if (cardDropSpawner != null)
        {
            cardDropSpawner.SpawnDrop();
        }

        StartTurn();
    }

    private void StartTurn()
    {
        if (CheckWinCondition()) return;

        // Reset timer text back to standard size and color for the new turn
        ResetTimerVisuals();

        // Calculate turn order (Interleaved P1 and P2)
        bool isP1Turn = (turnStep % 2 == 0) ? firstPlayerIsP1 : !firstPlayerIsP1;
        int beaverIndex = turnStep / 2; // Maps 0,1 -> Index 0 | 2,3 -> Index 1 | 4,5 -> Index 2

        // Trigger hand refreshed notification on Round 2+ for both players' first turns of the round
        if (currentRound >= 2 && (turnStep == 0 || turnStep == 1))
        {
            ShowHandRefreshedMessage();
        }
        else
        {
            HideHandRefreshedMessage();
        }

        // Update the current player UI text
        if (currentPlayerText != null)
        {
            currentPlayerText.text = isP1Turn ? "Player 1's Turn" : "Player 2's Turn";
        }

        if (currentPlayerBackgroundImage == null && currentPlayerText != null)
        {
            currentPlayerBackgroundImage = currentPlayerText.GetComponent<Image>();
        }

        if (currentPlayerBackgroundImage != null)
        {
            currentPlayerBackgroundImage.color = isP1Turn
                ? player1TurnBackgroundColor
                : player2TurnBackgroundColor;
        }

        // Deactivate all beavers first
        SetAllBeaversActive(false);

        List<BeaverController> currentList = isP1Turn ? player1Beavers : player2Beavers;

        // Safety check to get the beaver, keeping array bounds safe
        ActiveBeaver = (beaverIndex < currentList.Count) ? currentList[beaverIndex] : null;

        // If this specific beaver has been destroyed (is null), skip its turn step immediately
        if (ActiveBeaver == null)
        {
            turnStep++;
            if (turnStep >= 6) StartNewRound();
            else StartTurn();
            return;
        }

        // Active beaver is alive, proceed with turn
        ActiveBeaver.SetTurnActive(true);

        // Toggle UI Hands so only the active player sees their cards
        if (player1Hand != null) player1Hand.gameObject.SetActive(isP1Turn);
        if (player2Hand != null) player2Hand.gameObject.SetActive(!isP1Turn);

        // Reset Timer
        timeRemaining = turnDuration;
        isTurnRunning = true;
    }

    private void ShowHandRefreshedMessage()
    {
        if (handRefreshedText == null) return;

        if (handRefreshedCoroutine != null)
        {
            StopCoroutine(handRefreshedCoroutine);
        }

        handRefreshedCoroutine = StartCoroutine(FadeHandRefreshedMessage());
    }

    private void HideHandRefreshedMessage()
    {
        if (handRefreshedText == null) return;

        if (handRefreshedCoroutine != null)
        {
            StopCoroutine(handRefreshedCoroutine);
            handRefreshedCoroutine = null;
        }

        Color color = handRefreshedText.color;
        color.a = 0f;
        handRefreshedText.color = color;
        handRefreshedText.gameObject.SetActive(false);
    }

    private IEnumerator FadeHandRefreshedMessage()
    {
        handRefreshedText.text = "New round. Hand Refreshed!";
        Color color = handRefreshedText.color;
        color.a = 1f;
        handRefreshedText.color = color;
        handRefreshedText.gameObject.SetActive(true);

        yield return new WaitForSeconds(handRefreshedDisplayDuration);

        float elapsed = 0f;
        while (elapsed < handRefreshedFadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / handRefreshedFadeDuration);
            handRefreshedText.color = color;
            yield return null;
        }

        color.a = 0f;
        handRefreshedText.color = color;
        handRefreshedText.gameObject.SetActive(false);
        handRefreshedCoroutine = null;
    }

    public void AwardCardToBeaver(BeaverController beaver, GameObject handCardPrefab)
    {
        if (beaver == null || handCardPrefab == null) return;

        if (beaver.CompareTag("BeaverP1") && player1Hand != null)
            player1Hand.AddCard(handCardPrefab);
        else if (beaver.CompareTag("BeaverP2") && player2Hand != null)
            player2Hand.AddCard(handCardPrefab);
    }

    public void EndTurn()
    {
        if (!isTurnRunning) return;

        isTurnRunning = false;

        ResetTimerVisuals();
        HideHandRefreshedMessage();

        // Stop the coroutine immediately so it doesn't affect the next player
        if (equipmentCoroutine != null)
        {
            StopCoroutine(equipmentCoroutine);
            equipmentCoroutine = null;
        }

        // Clean up active equipment if turn ended due to timer
        if (activeEquipment != null)
        {
            Destroy(activeEquipment);
        }

        if (ActiveBeaver != null)
        {
            ActiveBeaver.SetTurnActive(false);
        }

        turnStep++;

        if (turnStep >= 6)
        {
            // All 3 beavers for both players have processed -> Round Over
            StartNewRound();
        }
        else
        {
            StartTurn();
        }
    }

    public void RegisterEquipment(GameObject equipment)
    {
        activeEquipment = equipment;

        // Stop any previous coroutine just to be safe
        if (equipmentCoroutine != null)
        {
            StopCoroutine(equipmentCoroutine);
        }

        // Store the new coroutine so we can cancel it later if needed
        equipmentCoroutine = StartCoroutine(WaitForEquipmentDestroyed(equipment));
    }

    private IEnumerator WaitForEquipmentDestroyed(GameObject equipment)
    {
        float timeAfterAction = 5f;

        if (equipment.GetComponent<GrenadeProjectile>() != null || equipment.GetComponentInChildren<GrenadeProjectile>() != null)
        {
            timeAfterAction = 3f;
        }

        while (equipment != null)
        {
            yield return null;
        }

        if (isTurnRunning)
        {
            timeRemaining = timeAfterAction;
        }
    }

    private void SetAllBeaversActive(bool active)
    {
        foreach (var b in player1Beavers) if (b != null) b.SetTurnActive(active);
        foreach (var b in player2Beavers) if (b != null) b.SetTurnActive(active);
    }

    private void ResetTimerVisuals()
    {
        if (timerText != null)
        {
            timerText.color = defaultTimerColor;
            timerText.transform.localScale = defaultTimerScale;
        }
    }

    private bool CheckWinCondition()
    {
        if (isGameOver) return true;

        int p1Alive = 0;
        foreach (var b in player1Beavers) if (b != null) p1Alive++;

        int p2Alive = 0;
        foreach (var b in player2Beavers) if (b != null) p2Alive++;

        if (p1Alive == 0 || p2Alive == 0)
        {
            isGameOver = true;
            isTurnRunning = false;

            ResetTimerVisuals();
            HideHandRefreshedMessage();

            if (currentPlayerText != null) currentPlayerText.gameObject.SetActive(false);

            if (winText != null)
            {
                winText.gameObject.SetActive(true);

                if (p1Alive == 0 && p2Alive == 0) winText.text = "Draw!";
                else if (p1Alive == 0) winText.text = "Player 2 Wins!";
                else winText.text = "Player 1 Wins!";
            }

            if (restartButton != null) restartButton.SetActive(true);
            if (menuButton != null) menuButton.SetActive(true);

            Time.timeScale = 0f;
            return true;
        }

        return false;
    }
}