using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnScreenOverlay : MonoBehaviour
{
    [SerializeField] private TMP_Text overlayTurnText;
    [SerializeField] private Image overlayImage;

    public void UpdateTurnScreen(int currentTurn){
        overlayTurnText.text = "Turn " + currentTurn.ToString();
        StartCoroutine(WaitAndFade());
    }

    private IEnumerator WaitAndFade() {
        overlayImage.gameObject.SetActive(true);
        // overlayImage.enabled = true;
        
        // Wait and fade
        yield return new WaitForSeconds(SorsTimings.overlayScreenDisplayTime);
        overlayImage.CrossFadeAlpha(0f, SorsTimings.overlayScreenFadeTime, false);
        overlayTurnText.text = "";

        // Wait and disable
        yield return new WaitForSeconds(SorsTimings.overlayScreenFadeTime);

        // overlayImage.enabled = false;
        overlayImage.gameObject.SetActive(false);
        overlayImage.CrossFadeAlpha(1f, 0f, false);

    }
}
