using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class EndScreen : NetworkBehaviour
{
    [SerializeField] private GameObject endView;
    [SerializeField] private Button exitButton;
    
    [SerializeField] private TMP_Text resultText; 
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text opponentNameText;
    
    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] private TMP_Text playerPointsText;
    [SerializeField] private TMP_Text opponentHealthText;
    [SerializeField] private TMP_Text opponentPointsText;

    [SerializeField] private Image playerHighlight;
    [SerializeField] private Image opponentHighlight;
    
    public static EndScreen Instance { get; private set; }

    private void Awake() {
        if (!Instance) Instance = this;
        
        endView.SetActive(false);
    }

    [ClientRpc]
    public void RpcSetFinalScore(PlayerManager player, int health, int score)
    {
        endView.SetActive(true);

        if (player.isOwned){
            playerNameText.text = player.PlayerName;
            playerHealthText.text = health.ToString();
            playerPointsText.text = score.ToString();
        } else {
            playerNameText.text = player.PlayerName;
            opponentHealthText.text = health.ToString();
            opponentPointsText.text = score.ToString();
        }
    }

    [ClientRpc]
    public void RpcGameHasWinner(PlayerManager player)
    {
        if (player.isOwned){
            resultText.text = "Victory";
            playerHighlight.enabled = true;
        } else {
            resultText.text = "Defeat";
            opponentHighlight.enabled = true;
        }
    }

    [ClientRpc]
    public void RpcGameIsDraw()
    {
        resultText.text = "Draw";
    }

    public void OnExitButtonPressed()
    {
        Application.Quit();
    }
}