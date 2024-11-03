using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

public class TurnScreenOverlay : ModalWindow
{
    [SerializeField] private TMP_Text _turnText;

    public void UpdateTurnScreen(int currentTurn)
    {
        _turnText.text = "Turn " + currentTurn.ToString();
        WaitAndFade().Forget();
    }

    private async UniTaskVoid WaitAndFade()
    {
        WindowIn();

        // TODO: Has to be equal to the time that TurnManager waits
        await UniTask.Delay(SorsTimings.waitLong);
        
        WindowOut();
    }
}
