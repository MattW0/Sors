using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityUI : MonoBehaviour
{
    [SerializeField] private Color attackingColor;
    private Color _idleColor = new Color32( 0x50, 0x50, 0x50, 0xFF );

    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text cost;
    [SerializeField] private TMP_Text attack;
    [SerializeField] private TMP_Text health;
    [SerializeField] private Image highlight;
    [SerializeField] private Image attackerHighlight;

    private Transform _transform;
    [SerializeField] private Transform playerPlayZone;
    [SerializeField] private Transform opponentPlayZone;
    
    private Vector3 _tappedPosition = new Vector3(0f, 60f, 0f);
    private float _tappingDuration = 0.3f;
    
    private void Awake()
    {
        _transform = gameObject.transform;
        _transform.position = Vector3.zero;
        playerPlayZone = GameObject.Find("PlayerPlayZone").transform.GetChild(1);
        opponentPlayZone = GameObject.Find("OpponentPlayZone").transform.GetChild(1);
    }
    public void SetEntityUI(CardInfo cardInfo)
    {
        title.text = cardInfo.title;
        cost.text = cardInfo.cost.ToString();
        attack.text = cardInfo.attack.ToString();
        health.text = cardInfo.health.ToString();
    }

    public void Highlight(bool active)
    {
        highlight.enabled = active;
    }

    public void HighlightAttacker(bool active)
    {
        attackerHighlight.color = active ? attackingColor : _idleColor;
    }


    public void TapCreature() {
        _transform.DOLocalMove(_tappedPosition, _tappingDuration).OnComplete(
            () => Highlight(false)
            );
    }
    
    public void UntapCreature() {
        _transform.DOLocalMove(Vector3.zero, _tappingDuration).OnComplete(
            () => Highlight(true)
            );
    }

    public void MoveToHolder(int holderNumber, bool isMine)
    {
        if (holderNumber == -1) Debug.Log("Wrong CardHolder number!");
        if(isMine) _transform.SetParent(playerPlayZone.GetChild(holderNumber), false);
        else {
            _transform.SetParent(opponentPlayZone.GetChild(holderNumber), false);
            _transform.DOLocalMove(_tappedPosition, 0f);
        }
    }
}
