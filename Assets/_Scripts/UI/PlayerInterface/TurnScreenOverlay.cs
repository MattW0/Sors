using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class TurnScreenOverlay : ModalWindow
{
    [SerializeField] private TMP_Text _turnText;
    // [SerializeField] private Image overlayImage;

    public void UpdateTurnScreen(int currentTurn)
    {
        _turnText.text = "Turn " + currentTurn.ToString();
        ModalWindowIn();
        // WaitAndFade().Forget();
    }

    // private async UniTaskVoid WaitAndFade()
    // {
    //     overlayImage.gameObject.SetActive(true);
    //     await UniTask.Delay(SorsTimings.overlayScreenDisplayTime);
        
    //     // Wait and fade
    //     overlayImage.CrossFadeAlpha(0f, SorsTimings.overlayScreenFadeTime, false);
    //     await UniTask.Delay(SorsTimings.overlayScreenFadeTime);

    //     // Wait and disable
    //     overlayImage.gameObject.SetActive(false);
    // }
}
