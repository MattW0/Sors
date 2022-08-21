using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public bool isMine = false;

    [Header("Player Stats")]
    public TMP_Text playerName;
    public TMP_Text playerHealth;
    public TMP_Text playerScore;

    [Header("Turn Stats")]
    public TMP_Text turnCash;
    public TMP_Text turnDeploys;
    public TMP_Text turnRecruits;

    public Button readyButton;

    private void Awake() {

    }

    public void SetPlayerUI(string name, string startHealth, string startScore){
        isMine = true;
        playerName.text = name;
        playerHealth.text = startHealth;
        playerScore.text = startScore;
        readyButton.gameObject.SetActive(true);
    }

    public void SetOpponentUI(string name, string startHealth, string startScore){
        playerName.text = name;
        playerHealth.text = startHealth;
        playerScore.text = startScore;
    }

    public void SetCash(int value) => turnCash.text = value.ToString();
    public void SetDeploys(int value) => turnDeploys.text = value.ToString();
    public void SetRecruits(int value) => turnRecruits.text = value.ToString();
}
