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

        DropZoneManager.OnCombatStart += StartCombat;
        DropZoneManager.OnCombatEnd += EndCombat;
    }

    public void SetEntity(BattleZoneEntity e, Vector3 p) {
        _playerEntity = e;
        _playerEntity.transform.position = p;
    }

    private void StartCombat()
    {
        // print("Start combat on player UI");
        transform.position += _combatPosition;
        _playerEntity.transform.position += _combatPosition;
    }

    private void EndCombat()
    {
        transform.position -= _combatPosition;
        _playerEntity.transform.position -= _combatPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(!_isTargetable) return;

        var clicker = PlayerManager.GetLocalPlayer();
        if (clicker.PlayerIsChoosingAttack) clicker.PlayerChoosesTargetToAttack(_playerEntity);
        else if (clicker.PlayerIsChoosingTarget) clicker.PlayerChoosesEntityTarget(_playerEntity);
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
        DropZoneManager.OnCombatStart -= StartCombat;
        DropZoneManager.OnCombatEnd -= EndCombat;
    }
}
