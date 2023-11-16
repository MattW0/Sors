using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PlayerUI : MonoBehaviour, IPointerClickHandler
{
    private BattleZoneEntity _playerEntity;
    private Vector3 _idlePosition;
    [SerializeField] private Vector3 _combatPosition;


    [Header("Player Stats")]
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private TMP_Text playerHealth;
    [SerializeField] private TMP_Text playerScore;

    [Header("Turn Stats")]
    [SerializeField] private TMP_Text turnCash;
    [SerializeField] private TMP_Text turnInvents;
    [SerializeField] private TMP_Text turnDevelops;
    [SerializeField] private TMP_Text turnDeploys;
    [SerializeField] private TMP_Text turnRecruits;
    [SerializeField] private Image highlight;
    private bool _isTargetable;

    private void Awake()
    {
        _idlePosition = transform.position;

        PhasePanel.OnCombatStart += StartCombat;
        PhasePanel.OnCombatEnd += EndCombat;
    }

    public void SetEntity(BattleZoneEntity e) => _playerEntity = e;

    private void StartCombat()
    {
        transform.position += _combatPosition;
    }

    private void EndCombat()
    {
        transform.position -= _combatPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(!_isTargetable) return;

        var clicker = PlayerManager.GetLocalPlayer();
        if (!(clicker.PlayerIsChoosingAttack || clicker.PlayerIsChoosingTarget)) return;

        clicker.PlayerChoosesTargetToAttack(_playerEntity);
    }

    public void TargetHighlight(bool targetable, bool isPlayer){
        _isTargetable = targetable;

        if(targetable) highlight.color = SorsColors.targetColor;
        else {
            if(isPlayer) highlight.color = SorsColors.player;
            else highlight.color = SorsColors.opponent;
        }
    }
    public void SetName(string name) => playerName.text = name;
    public void SetHealth(int value) => playerHealth.text = value.ToString();
    public void SetScore(int value) => playerScore.text = value.ToString();
    public void SetCash(int value) => turnCash.text = value.ToString();
    public void SetInvents(int value) => turnInvents.text = value.ToString();
    public void SetDevelops(int value) => turnDevelops.text = value.ToString();
    public void SetDeploys(int value) => turnDeploys.text = value.ToString();
    public void SetRecruits(int value) => turnRecruits.text = value.ToString();

    private void OnDestroy(){
        PhasePanel.OnCombatStart -= StartCombat;
        PhasePanel.OnCombatEnd -= EndCombat;
    }
}
