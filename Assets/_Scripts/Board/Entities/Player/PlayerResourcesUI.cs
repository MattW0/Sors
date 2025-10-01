using TMPro;
using UnityEngine;

public class PlayerResourcesUI : MonoBehaviour
{
    [SerializeField] private TMP_Text turnCash;
    [SerializeField] private TMP_Text turnBuys;
    [SerializeField] private TMP_Text turnPlays;
    [SerializeField] private TMP_Text turnPrevails;
    public void SetCash(int value) => turnCash.text = value.ToString();
    public void SetBuys(int value) => turnBuys.text = value.ToString();
    public void SetPlays(int value) => turnPlays.text = value.ToString();
    public void SetPrevails(int value) => turnPrevails.text = value.ToString();
}
