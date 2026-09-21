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

    [Header("UI & HUD")]
    [SerializeField] private CardHandManager player1Hand;
    [SerializeField] private CardHandManager player2Hand;
    [SerializeField] private int cardsPerRound = 3;
    [SerializeField] private TextMeshProUGUI timerText; // Optional timer UI display
    [SerializeField] private TextMeshProUGUI winText;   // Assign a UI Text element for "Player X Wins!"
    [SerializeField] private GameObject restartButton;
    [SerializeField] private TextMeshProUGUI currentPlayerText; // NEW: Text to display whose turn it is

    [Header("Turn Settings")]
    [SerializeField] private float turnDuration = 30f;

    public BeaverController ActiveBeaver { get; private set; }

    private int currentRound = 0;
    private int turnStep = 0; // 0 to 5 (6 total beaver turns per round)
    private bool firstPlayerIsP1 = true;

    private float timeRemaining;
    private bool isTurnRunning = false;
    private bool isGameOver = false;
    private GameObject activeEquipment;

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
        if (currentPlayerText != null) currentPlayerText.gameObject.SetActive(true);

        // Randomize who goes first at game start
        firstPlayerIsP1 = Random.value > 0.5f;
        StartNewRound();
    }

    private void Update()
    {
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

        // Populate fresh random hands for both players at the start of a round
        if (player1Hand != null) player1Hand.ClearHand();
        if (player2Hand != null) player2Hand.ClearHand();

        for (int i = 0; i < cardsPerRound; i++)
        {
            if (player1Hand != null) player1Hand.AddCard();
            if (player2Hand != null) player2Hand.AddCard();
        }

        StartTurn();
    }

    private void StartTurn()
    {
        if (CheckWinCondition()) return;

        // Calculate turn order (Interleaved P1 and P2)
        bool isP1Turn = (turnStep % 2 == 0) ? firstPlayerIsP1 : !firstPlayerIsP1;
        int beaverIndex = turnStep / 2; // Maps 0,1 -> Index 0 | 2,3 -> Index 1 | 4,5 -> Index 2

        // NEW: Update the current player UI text
        if (currentPlayerText != null)
        {
            currentPlayerText.text = isP1Turn ? "Player 1's Turn" : "Player 2's Turn";

            // Optional: You can color code the text here if you want!
            // currentPlayerText.color = isP1Turn ? Color.blue : Color.red; 
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
        StartCoroutine(WaitForEquipmentDestroyed(equipment));
    }

    private IEnumerator WaitForEquipmentDestroyed(GameObject equipment)
    {
        // Determine the time to set based on what component is on the equipment.
        // Defaults to 5s for weapons and traversals.
        float timeAfterAction = 5f;

        // If it's a grenade, set it to 3s
        if (equipment.GetComponent<GrenadeProjectile>() != null || equipment.GetComponentInChildren<GrenadeProjectile>() != null)
        {
            timeAfterAction = 3f;
        }

        // Wait until the equipped object is destroyed (by usage, depletion, or explosion)
        while (equipment != null)
        {
            yield return null;
        }

        // Set the remaining time for the turn immediately to 5 or 3 seconds
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

    private bool CheckWinCondition()
    {
        if (isGameOver) return true;

        // Count how many beavers are still alive (not null)
        int p1Alive = 0;
        foreach (var b in player1Beavers) if (b != null) p1Alive++;

        int p2Alive = 0;
        foreach (var b in player2Beavers) if (b != null) p2Alive++;

        // If either team has 0 beavers left, trigger Game Over
        if (p1Alive == 0 || p2Alive == 0)
        {
            isGameOver = true;
            isTurnRunning = false;

            // NEW: Hide the turn text when the game ends
            if (currentPlayerText != null) currentPlayerText.gameObject.SetActive(false);

            if (winText != null)
            {
                winText.gameObject.SetActive(true);

                if (p1Alive == 0 && p2Alive == 0) winText.text = "Draw!"; // Rare but possible!
                else if (p1Alive == 0) winText.text = "Player 2 Wins!";
                else winText.text = "Player 1 Wins!";
            }

            if (restartButton != null) restartButton.SetActive(true);

            // Freeze the game
            Time.timeScale = 0f;
            return true;
        }

        return false;
    }
}