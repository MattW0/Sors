using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    private TurnManager turnManager;
    public bool isMine = false;

    [Header("Player Stats")]
    public TMP_Text playerName;
    public TMP_Text playerScore;
    public TMP_Text playerHealth;

    [Header("Turn Stats")]
    public TMP_Text turnCash;
    public TMP_Text turnBuys;
    public TMP_Text turnRecruits;

    public Button readyButton;

    private void Start() {
        turnManager = TurnManager.Instance;
    }
    
    public void UpdateHealth(int healthDelta)
    {
        int currentHealth = int.Parse(playerHealth.text);
        currentHealth += healthDelta;
        playerHealth.text = currentHealth.ToString();
    }

    public void UpdateScore(int scoreDelta)
    {
        int currentScore = int.Parse(playerScore.text);
        currentScore += scoreDelta;
        playerScore.text = currentScore.ToString();
    }

    public void UpdateCash(int cashDelta)
    {
        int currentCash = int.Parse(turnCash.text);
        currentCash += cashDelta;
        turnCash.text = currentCash.ToString();
    }

    public void UpdateBuys(int buysDelta)
    {
        int currentBuys = int.Parse(turnBuys.text);
        currentBuys += buysDelta;
        turnBuys.text = currentBuys.ToString();
    }

    public void UpdateRecruits(int recruitsDelta)
    {
        int currentRecruits = int.Parse(turnRecruits.text);
        currentRecruits += recruitsDelta;
        turnRecruits.text = currentRecruits.ToString();
    }
    

    // public void OnReadyButtonPressed(){

    // }
}
