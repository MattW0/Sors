using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DetailCard : MonoBehaviour
{
    [SerializeField] private DetailCardUI _ui;
    public void SetCardUI(CardInfo card) => _ui.SetCardUI(card);
    public void DisableFocus() => _ui.EnableFocus = false;
}