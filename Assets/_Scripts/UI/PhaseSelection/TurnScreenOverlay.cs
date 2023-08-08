using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnScreenOverlay : MonoBehaviour
{
    public static TurnScreenOverlay Instance { get; private set; }
    [SerializeField] private TMP_Text overlayTurnText;
    [SerializeField] private Image overlayImage;
    [SerializeField] private int overlayScreenWaitTime = 1;
    [SerializeField] private float overlayScreenFadeTime = 0.5f;

    private void Awake(){
        if(!Instance) Instance = this;
    }

    public void UpdateTurnScreen(int currentTurn){
        overlayTurnText.text = "Turn " + currentTurn.ToString();
        StartCoroutine(WaitAndFade());
    }

    private IEnumerator WaitAndFade() {
        overlayImage.gameObject.SetActive(true);
        // overlayImage.enabled = true;
        
        // Wait and fade
        yield return new WaitForSeconds(overlayScreenWaitTime);
        overlayImage.CrossFadeAlpha(0f, overlayScreenFadeTime, false);
        overlayTurnText.text = "";

        // Wait and disable
        yield return new WaitForSeconds(overlayScreenFadeTime);

        // overlayImage.enabled = false;
        overlayImage.gameObject.SetActive(false);
        overlayImage.CrossFadeAlpha(1f, 0f, false);

    }
}
