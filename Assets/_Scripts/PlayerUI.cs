using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    private TurnManager turnManager;
    public bool isMine = false;
    public TMP_Text playerName;
    public TMP_Text playerScore;
    public TMP_Text playerHealth;
    public Button readyButton;

    private void Start() {
        turnManager = TurnManager.instance;
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

    public void OnReadyButtonPressed(){

    }
}
