using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityUI : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text cost;
    [SerializeField] private TMP_Text attack;
    [SerializeField] private TMP_Text health;
    [SerializeField] private Image highlight;
    [SerializeField] private Image attackerHighlight;

    private Transform _transform;
    [SerializeField] private Transform playerPlayZone;
    [SerializeField] private Transform opponentPlayZone;
    
    private readonly Vector3 _tappedPosition = new (0f, 60f, 0f);
    private const float TappingDuration = 0.3f;

    [SerializeField] private Color attackingColor;
    private readonly Color _idleColor = new Color32( 0x50, 0x50, 0x50, 0xFF );
    
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

    public void SetHealth(int newValue) => health.text = newValue.ToString();
    public void SetAttack(int newValue) => attack.text = newValue.ToString();

    public void Highlight(bool active)
    {
        highlight.enabled = active;
    }

    public void ShowAsAttacker(bool active)
    {
        attackerHighlight.color = active ? attackingColor : _idleColor;
    }

    public void TapCreature() {
        _transform.DOLocalMove(_tappedPosition, TappingDuration).OnComplete(
            () => Highlight(false)
            );
    }
    
    public void UntapCreature() {
        _transform.DOLocalMove(Vector3.zero, TappingDuration).OnComplete(
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
