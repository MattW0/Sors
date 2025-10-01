using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class PlayerEntityUI : MonoBehaviour, IPointerClickHandler
{
    private BattleZoneEntity _playerEntity;
    private Vector3 _idlePosition;
    [SerializeField] private Vector3 _combatPosition;

    [Header("Player Stats")]
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private TMP_Text playerHealth;
    [SerializeField] private TMP_Text playerScore;

    [Header("Turn Stats")]
    [SerializeField] private Image highlight;
    private bool _isTargetable;
    public static event Action<BattleZoneEntity> OnClickedPlayer;
    private SorsColors _colors;


    private void Awake()
    {
        _idlePosition = transform.position;

        // DropZoneManager.OnDeclareAttackers += StartCombat;
        // DropZoneManager.OnCombatEnd += EndCombat;
    }

    public void SetEntity(BattleZoneEntity e) 
    {
        _playerEntity = e;
        _playerEntity.transform.position = transform.GetChild(0).position;

        _colors = UIManager.ColorPalette;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(!_isTargetable) return;

        OnClickedPlayer?.Invoke(_playerEntity);
    }

    public void TargetHighlight(bool targetable, bool isOwned)
    {
        _isTargetable = targetable;

        if(targetable) highlight.color = _colors.targetHighlight;
        else ResetHighlight(isOwned);
    }

    private void ResetHighlight(bool isOwned)
    {
        if(isOwned) highlight.color = _colors.player;
        else highlight.color = _colors.opponent;
    }

    public void SetName(string name) => playerName.text = name;
    public void SetHealth(int value) => playerHealth.text = value.ToString();
    public void SetScore(int value) => playerScore.text = value.ToString();

    // private void StartCombat(bool start)
    // {
    //     if(!start) return;

    //     transform.position += _combatPosition;
    //     _playerEntity.transform.position += _combatPosition;
    // }

    // private void EndCombat()
    // {
    //     transform.position -= _combatPosition;
    //     _playerEntity.transform.position -= _combatPosition;
    // }

    // private void OnDestroy(){
    //     DropZoneManager.OnDeclareAttackers -= StartCombat;
    //     DropZoneManager.OnCombatEnd -= EndCombat;
    // }
}
