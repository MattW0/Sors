using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class HandCardUI : CardUI
{
    public GameObject _front;

    public override void SetCardUI(CardInfo cardInfo){
        base.SetCardUI(cardInfo);
        HighlightReset();
    }

    public void CardBackUp(){
        _front.SetActive(false);
        // _border.enabled = false;
    }
    public void CardFrontUp(){
        _front.SetActive(true);
        // _border.enabled = true;
    }

    public void Highlight(bool active, Color color){
        // if(!_highlight) return;

        highlight.color = color;
        highlight.enabled = active;
    }
    
    private void HighlightReset(){
        highlight.color = SorsColors.standardHighlight;
        highlight.enabled = false;
    }
}
