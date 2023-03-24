using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class EntityUI : MonoBehaviour
{
    [Header("Entity Stats")]
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text cost;
    [SerializeField] private TMP_Text attack;
    [SerializeField] private TMP_Text health;
    [SerializeField] private TMP_Text points;
    [SerializeField] private List<TMP_Text> keyWords;

    [Header("Entity UI")]
    [SerializeField] private Image highlight;
    [SerializeField] private Image attackerHighlight;

    private Transform _transform;
    private Transform _playerPlayZone;
    private Transform _opponentPlayZone;

    // private readonly float _tapDistance = 60f;
    private readonly Vector3 _untappedPosition = new Vector3(0f, 0f, -1f);
    private readonly Vector3 _tappedPosition = new (0f, 60f, -1f);
    private readonly Vector3 _opponentTappedPosition = new (0f, -60f, -1f);

    private const float TappingDuration = 0.3f;

    [SerializeField] private Color attackingColor;
    private readonly Color _idleColor = new Color32( 0x50, 0x50, 0x50, 0xFF );
    
    private void Awake()
    {
        _transform = gameObject.transform;
        _transform.position = new Vector3(0f, 0f, -1f);
        // _playerPlayZone = GameObject.Find("PlayerPlayZone").transform.GetChild(1);
        // _opponentPlayZone = GameObject.Find("OpponentPlayZone").transform.GetChild(1);
    }
    public void SetEntityUI(CardInfo cardInfo)
    {
        // Set card stats
        title.text = cardInfo.title;
        cost.text = cardInfo.cost.ToString();
        attack.text = cardInfo.attack.ToString();
        health.text = cardInfo.health.ToString();
        points.text = cardInfo.points.ToString();

        // Set keywords
        int i = 0;
        foreach (var kw in cardInfo.keyword_abilities)
        {
            keyWords[i].text = kw.ToString();
            keyWords[i].enabled = true;
            i++;
        }
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
        _transform.DOLocalMove(_tappedPosition, TappingDuration).OnComplete(
            () => Highlight(false)
            );
    }

    public void UntapCreature(bool highlight) {
        _transform.DOLocalMove(_untappedPosition, TappingDuration).OnComplete(
            () => Highlight(highlight)
            );
        ShowAsAttacker(false);
    }

    public void TapOpponentCreature() => _transform.DOLocalMove(_untappedPosition, TappingDuration);
    public void UntapOpponentCreature() => _transform.DOLocalMove(_opponentTappedPosition, TappingDuration);

    // public void MoveToHolder(bool isMine, int holderNumber)
    // {
    //     if(isMine) _transform.SetParent(_playerPlayZone, false);
    //     else {
    //         _transform.SetParent(_opponentPlayZone, false);
    //         _transform.DOLocalMove(_tappedPosition, 0f);
    //     }
    // }
}
