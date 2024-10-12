using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndScreen : ModalWindow
{
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

    private void Start()
    {
        exitButton.onClick.AddListener(OnExitButtonPressed);
    }

    public void SetPlayerScore(PlayerManager player, int health, int score)
    {
        ModalWindowIn();

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

    public void SetGameWinner(PlayerManager player)
    {
        if (player.isOwned){
            resultText.text = "Victory";
            playerHighlight.enabled = true;
        } else {
            resultText.text = "Defeat";
            opponentHighlight.enabled = true;
        }
    }

    public void SetDraw() => resultText.text = "Draw";

    public void OnExitButtonPressed()
    {
        Application.Quit();
    }
}