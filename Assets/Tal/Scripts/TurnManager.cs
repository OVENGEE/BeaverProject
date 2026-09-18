using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Players & Beavers")]
    [SerializeField] private List<BeaverController> player1Beavers;
    [SerializeField] private List<BeaverController> player2Beavers;

    [Header("UI Hands")]
    [SerializeField] private CardHandManager player1Hand;
    [SerializeField] private CardHandManager player2Hand;
    [SerializeField] private int cardsPerRound = 3;

    [Header("Turn Settings")]
    [SerializeField] private float turnDuration = 30f;
    [SerializeField] private TextMeshProUGUI timerText; // Optional timer UI display

    public BeaverController ActiveBeaver { get; private set; }

    private int currentRound = 0;
    private int turnStep = 0; // 0 to 5 (6 total beaver turns per round)
    private bool firstPlayerIsP1 = true;

    private float timeRemaining;
    private bool isTurnRunning = false;
    private GameObject activeEquipment;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Randomize who goes first at game start
        firstPlayerIsP1 = Random.value > 0.5f;
        StartNewRound();
    }

    private void Update()
    {
        if (!isTurnRunning) return;

        timeRemaining -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }

        if (timeRemaining <= 0f)
        {
            EndTurn();
        }
    }

    public void StartNewRound()
    {
        currentRound++;
        turnStep = 0;

        // Populate fresh random hands for both players at the start of a round
        player1Hand.ClearHand();
        player2Hand.ClearHand();

        for (int i = 0; i < cardsPerRound; i++)
        {
            player1Hand.AddCard();
            player2Hand.AddCard();
        }

        StartTurn();
    }

    private void StartTurn()
    {
        // Calculate turn order (Interleaved P1 and P2)
        bool isP1Turn = (turnStep % 2 == 0) ? firstPlayerIsP1 : !firstPlayerIsP1;
        int beaverIndex = turnStep / 2; // Maps 0,1 -> Index 0 | 2,3 -> Index 1 | 4,5 -> Index 2

        // Deactivate all beavers first
        SetAllBeaversActive(false);

        // Assign active beaver
        List<BeaverController> currentList = isP1Turn ? player1Beavers : player2Beavers;
        ActiveBeaver = currentList[beaverIndex];
        ActiveBeaver.SetTurnActive(true);

        // Toggle UI Hands so only the active player sees their cards
        player1Hand.gameObject.SetActive(isP1Turn);
        player2Hand.gameObject.SetActive(!isP1Turn);

        // Reset Timer
        timeRemaining = turnDuration;
        isTurnRunning = true;
    }

    public void EndTurn()
    {
        if (!isTurnRunning) return;

        isTurnRunning = false;

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
            // All 3 beavers for both players have played -> Round Over
            StartNewRound();
        }
        else
        {
            StartTurn();
        }
    }

    // Call this whenever a weapon or traversal is instantiated onto a beaver
    public void RegisterEquipment(GameObject equipment)
    {
        activeEquipment = equipment;
        StartCoroutine(WaitForEquipmentDestroyed(equipment));
    }

    private IEnumerator WaitForEquipmentDestroyed(GameObject equipment)
    {
        // Wait until the equipped weapon/traversal object is destroyed by usage
        while (equipment != null)
        {
            yield return null;
        }

        // Give a short 0.5s buffer before wrapping the turn
        yield return new WaitForSeconds(0.5f);

        // Auto-end the turn when item finishes
        EndTurn();
    }

    private void SetAllBeaversActive(bool active)
    {
        foreach (var b in player1Beavers) if (b != null) b.SetTurnActive(active);
        foreach (var b in player2Beavers) if (b != null) b.SetTurnActive(active);
    }
}