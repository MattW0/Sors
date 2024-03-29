using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class EntityUI : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text cost;
    [SerializeField] private TMP_Text attack;
    [SerializeField] private TMP_Text health;
    [SerializeField] private TMP_Text points;
    [SerializeField] private TMP_Text description;

    [SerializeField] private Image highlight;
    [SerializeField] private Image attackerHighlight;
    [SerializeField] private TMP_Text keyWords;

    private Transform _transform;
    [SerializeField] private Transform playerPlayZone;
    [SerializeField] private Transform opponentPlayZone;

    private readonly float _tapDistance = 60f;
    private readonly Vector3 _tappedPosition = new (0f, 60f, 0f);
    private readonly Vector3 _opponentTappedPosition = new (0f, -60f, 0f);

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
        description.text = string.Join(" ", cardInfo.keyword_abilities.ConvertAll(f => f.ToString()));

        // var keyword_strings = cardInfo.keyword_abilities.ConvertAll(f => f.ToString());
        // description.text = String.Join(" ", keyword_strings);
    }

    public void SetHealth(int newValue) => health.text = newValue.ToString();
    public void SetAttack(int newValue) => attack.text = newValue.ToString();
    public void SetPoints(int newValue) => points.text = newValue.ToString();

    public void Highlight(bool active)
    {
        highlight.enabled = active;
    }

    public void ShowAsAttacker(bool active) => attackerHighlight.color = active ? attackingColor : _idleColor;

    public void TapCreature() {
        _transform.DOLocalMove(new Vector3(0f, _tapDistance, 0f), TappingDuration).OnComplete(
            () => Highlight(false)
            );
    }

    public void UntapCreature(bool highlight) {
        _transform.DOLocalMove(Vector3.zero, TappingDuration).OnComplete(
            () => Highlight(highlight)
            );
        ShowAsAttacker(false);
    }

    public void TapOpponentCreature() => _transform.DOLocalMove(Vector3.zero, TappingDuration);
    public void UntapOpponentCreature() => _transform.DOLocalMove(new Vector3(0f, _tapDistance, 0f), TappingDuration);

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
