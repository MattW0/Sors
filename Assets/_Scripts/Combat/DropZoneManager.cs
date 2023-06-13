using System.Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class DropZoneManager : NetworkBehaviour
{
    private const int MAX_ENTITIES = 6;
    public static DropZoneManager Instance { get; private set; }
    private BoardManager _boardManager;
    private PlayerManager _player;
    private CombatState _combatState;
    [SerializeField] private MoneyZone playerMoneyZone;
    [SerializeField] private MoneyZone opponentMoneyZone;
    [SerializeField] private List<CreatureEntity> _hostCreatures = new();
    [SerializeField] private List<DevelopmentEntity> _hostDevelopments = new();
    [SerializeField] private List<CreatureEntity> _clientCreatures = new();
    [SerializeField] private List<DevelopmentEntity> _clientDevelopments = new();

    #region Entity Holders
    [SerializeField] private GameObject playerCreatureZone;
    [SerializeField] private GameObject playerDevelopmentZone;
    [SerializeField] private GameObject opponentCreatureZone;
    [SerializeField] private GameObject opponentDevelopmentZone;
    private PlayZoneCardHolder[] _playerDevelopmentHolders = new PlayZoneCardHolder[MAX_ENTITIES];
    private PlayZoneCardHolder[] _playerCreatureHolders = new PlayZoneCardHolder[MAX_ENTITIES];
    private PlayZoneCardHolder[] _opponentDevelopmentHolders= new PlayZoneCardHolder[MAX_ENTITIES];
    private PlayZoneCardHolder[] _opponentCreatureHolders = new PlayZoneCardHolder[MAX_ENTITIES];

    #endregion

    public static event Action OnDeclareAttackers;
    public static event Action OnDeclareBlockers;
    public static event Action OnCombatEnded;

    private void Start()
    {
        if (!Instance) Instance = this;
        _player = PlayerManager.GetLocalPlayer();

        FindEntityHolders();

        if (isServer) _boardManager = BoardManager.Instance;
    }

    #region Entities ETB and LTB
    
    [Server]
    public void EntityEntersDropZone(PlayerManager owner, BattleZoneEntity entity)
    {
        if(entity.cardType == CardType.Development) {
            var development = entity.GetComponent<DevelopmentEntity>();
            if(owner.isLocalPlayer) _hostDevelopments.Add(development);
            else _clientDevelopments.Add(development);
        } else if(entity.cardType == CardType.Creature) {
            var creature = entity.GetComponent<CreatureEntity>();
            if(owner.isLocalPlayer) _hostCreatures.Add(creature);
            else _clientCreatures.Add(creature);
        }

        RpcMoveEntityToHolder(entity);
        ResetHolders();
        entity.OnDeath += EntityLeavesPlayZone;
    }

    [ClientRpc]
    private void RpcMoveEntityToHolder(BattleZoneEntity entity)
    {
        var targetTransform = FindHolderTransform(entity);
        if(!targetTransform) {
            print("No free holders found! Aborting to play entity...");
            return;
        }
        entity.transform.SetParent(targetTransform, false);
    }

    [Server]
    private void EntityLeavesPlayZone(PlayerManager owner, BattleZoneEntity entity)
    {
        if(entity.cardType == CardType.Development) {
            var development = entity.GetComponent<DevelopmentEntity>();
            if(owner.isLocalPlayer) _hostDevelopments.Remove(development);
            else _clientDevelopments.Remove(development);
        } else if(entity.cardType == CardType.Creature) {
            var creature = entity.GetComponent<CreatureEntity>();
            if(owner.isLocalPlayer) _hostCreatures.Remove(creature);
            else _clientCreatures.Remove(creature);
        }

        entity.OnDeath -= EntityLeavesPlayZone;
    }

    #endregion

    #region Attackers

    [Server]
    public void StartDeclareAttackers(List<PlayerManager> players)
    {
        _combatState = CombatState.Attackers;
        foreach (var player in players) {
            if (player.isLocalPlayer) TargetDeclareAttackers(player.connectionToClient, _hostCreatures);
            else TargetDeclareAttackers(player.connectionToClient, _clientCreatures);
        }
    }

    [TargetRpc]
    private void TargetDeclareAttackers(NetworkConnection conn, List<CreatureEntity> creatures)
    {
        // Auto-skipping if player has empty board
        if (creatures.Count == 0) {
            if (isServer) PlayerFinishedChoosingAttackers(_player, true);
            else CmdPlayerFinishedChoosingAttackers(_player, true);
            return;
        }
        // Else we enable entities to be tapped and wait for player to declare attackers and press ready btn
        OnDeclareAttackers?.Invoke();
        foreach (var creature in creatures) {
            creature.CheckIfCanAct(CombatState.Attackers);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdPlayerFinishedChoosingAttackers(PlayerManager player, bool skip){
        PlayerFinishedChoosingAttackers(player, skip);
    }

    [Server]
    private void PlayerFinishedChoosingAttackers(PlayerManager player, bool skip = false)
    {
        if (skip) {
            _boardManager.DisableReadyButton(player);
            _boardManager.AttackersDeclared(player, new List<CreatureEntity>());
            return;
        }

        List<CreatureEntity> creaturesList = new();
        if (player.isLocalPlayer) creaturesList = _hostCreatures;
        else creaturesList = _clientCreatures;

        TargetReturnAttackersList(player.connectionToClient, creaturesList, player);
    }

    [TargetRpc]
    private void TargetReturnAttackersList(NetworkConnection conn, List<CreatureEntity> creatures, PlayerManager player)
    {
        var attackers = new List<CreatureEntity>();
        foreach (var creature in creatures) {
            creature.LocalPlayerIsReady();
            if (!creature.IsAttacking) continue;

            attackers.Add(creature);
        }

        if (isServer) _boardManager.AttackersDeclared(player, attackers);
        else CmdReturnAttackersList(player, attackers);
    }

    [Command(requiresAuthority = false)]
    private void CmdReturnAttackersList(PlayerManager player, List<CreatureEntity> attackers) => _boardManager.AttackersDeclared(player, attackers);
    
    #endregion

    #region Blockers

    [Server]
    public void StartDeclareBlockers(List<PlayerManager> players)
    {
        _combatState = CombatState.Blockers;
        foreach (var player in players) {
            // Host Logic
            if (player.isLocalPlayer) {
                // Is there an opponent creature attacking? 
                var isHostAttacked = _clientCreatures.Exists(entity => entity.IsAttacking);
                TargetDeclareBlockers(player.connectionToClient, _hostCreatures, isHostAttacked);
                return;
            }
            
            // Client Logic
            var isClientAttacked = _hostCreatures.Exists(entity => entity.IsAttacking);
            TargetDeclareBlockers(player.connectionToClient, _clientCreatures, isClientAttacked);
        }
    }

    [TargetRpc]
    private void TargetDeclareBlockers(NetworkConnection conn, List<CreatureEntity> creatures, bool isAttacked)
    {
        var nbAttackers = 0;
        foreach (var creature in creatures) {
            if (creature.IsAttacking) nbAttackers++;
        }

        // If not being attacked or all my creatures are attacking, we skip
        // isAttacked is true if there is at least one attacking opponent entity
        if (!isAttacked || creatures.Count == nbAttackers) {
            PlayerFinishedChoosingBlockers();
            return;
        }

        // Else we enable entities to be tapped and wait for player to declare attackers and press ready btn
        OnDeclareBlockers?.Invoke();
        foreach (var creature in creatures) {
            creature.CheckIfCanAct(CombatState.Blockers);
        }
    }

    private void PlayerFinishedChoosingBlockers()
    {
        if(isServer) {
            foreach(var entity in _hostCreatures){
                entity.SetHighlight(false);
            }
            _boardManager.DisableReadyButton(_player);
            ServerPlayerFinishedChoosingBlockers(_player, true);
        } else {
            foreach(var entity in _clientCreatures){
                entity.SetHighlight(false);
            }
            CmdPlayerFinishedChoosingBlockers(_player, true);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdPlayerFinishedChoosingBlockers(PlayerManager player, bool skip)
    {
        _boardManager.DisableReadyButton(player);
        ServerPlayerFinishedChoosingBlockers(player, skip);
    }

    [Server]
    public void ServerPlayerFinishedChoosingBlockers(PlayerManager player, bool skip = false)
    {
        _boardManager.BlockersDeclared(player, new List<CreatureEntity>());
    }

    [Server]
    public void CombatCleanUp()
    {
        _combatState = CombatState.CleanUp;

        OnCombatEnded?.Invoke();

        // var creatures = new List<CreatureEntity>();
        // creatures.AddRange(_hostCreatures);
        // creatures.AddRange(_clientCreatures);
        // RpcResetCreatures(creatures);
    }

    // [ClientRpc]
    // private void RpcResetCreatures(List<CreatureEntity> creatures)
    // {
    //     foreach (var creature in creatures){
    //         creature.ResetAfterCombat();
    //     }
    // }

    #endregion

    #region UI and utils
    private void FindEntityHolders(){
        for (int i = 0; i < MAX_ENTITIES; i++){
            _playerDevelopmentHolders[i] = playerDevelopmentZone.transform.GetChild(i).GetComponent<PlayZoneCardHolder>();
            _playerCreatureHolders[i] = playerCreatureZone.transform.GetChild(i).GetComponent<PlayZoneCardHolder>();
            _opponentDevelopmentHolders[i] = opponentDevelopmentZone.transform.GetChild(i).GetComponent<PlayZoneCardHolder>();
            _opponentCreatureHolders[i] = opponentCreatureZone.transform.GetChild(i).GetComponent<PlayZoneCardHolder>();
        }
    }
    private Transform FindHolderTransform(BattleZoneEntity entity)
    {
        var index = 0;
        if(entity.isOwned){
            if(entity.cardType == CardType.Development){
                index = GetFirstFreeHolderIndex(_playerDevelopmentHolders);
                return _playerDevelopmentHolders[index].transform;
            } else if(entity.cardType == CardType.Creature){
                index = GetFirstFreeHolderIndex(_playerCreatureHolders);
                return _playerCreatureHolders[index].transform;
            }
        }
        
        // Opponent Entity
        if(entity.cardType == CardType.Development){
            index = GetFirstFreeHolderIndex(_opponentDevelopmentHolders);
            return _opponentDevelopmentHolders[index].transform;
        } else if(entity.cardType == CardType.Creature){
            index = GetFirstFreeHolderIndex(_opponentCreatureHolders);
            return _opponentCreatureHolders[index].transform;
        }
        
        // Returning null if no free holders found 
        return null;
    }

    private int GetFirstFreeHolderIndex(PlayZoneCardHolder[] holders)
    {
        // Only other child is holder outline image -> childCount == 1
        for (int i = 0; i < holders.Length; i++){
            if(holders[i].transform.childCount == 1) return i;
        }
        return -1;
    }

    [ClientRpc]
    public void RpcHighlightCardHolders()
    {
        var state = TurnManager.GetTurnState();
        if(state == TurnState.Develop) HighlightDevelopmentHolders();
        else if (state == TurnState.Deploy) HighlightCreatureHolders();
    }

    public void HighlightDevelopmentHolders()
    {
        foreach (var holder in _playerDevelopmentHolders) {
            holder.SetHighlight();
        }
    }

    public void HighlightCreatureHolders()
    {
        foreach (var holder in _playerCreatureHolders) {
            holder.SetHighlight();
        }
    }
    
    public void ResetHolders()
    {
        foreach (var holder in _playerDevelopmentHolders) {
            holder.ResetHighlight();
        }
        foreach (var holder in _playerCreatureHolders) {
            holder.ResetHighlight();
        }
    }

    [ClientRpc]
    public void RpcDiscardMoney()
    {
        playerMoneyZone.DiscardMoney();
        opponentMoneyZone.DiscardMoney();
    }

    [Server]
    public void PlayerPressedReadyButton(PlayerManager player)
    {
        if (_combatState == CombatState.Attackers) {
            PlayerFinishedChoosingAttackers(player);
        }
        else if (_combatState == CombatState.Blockers) {
            ServerPlayerFinishedChoosingBlockers(player);
        }
    }

    #endregion
}
