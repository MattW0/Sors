using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EntityZones : NetworkBehaviour
{
    private const int MAX_ENTITIES = 6;
    [SerializeField] private List<CreatureEntity> _hostCreatures = new();
    [SerializeField] private List<CreatureEntity> _clientCreatures = new();
    [SerializeField] private List<DevelopmentEntity> _hostDevelopments = new();
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

    private void Start(){
        FindEntityHolders();
    }

    public List<DevelopmentEntity> GetDevelopments(bool isHost){
        if(isHost) return _hostDevelopments;
        else return _clientDevelopments;
    }

    public void AddDevelopment(DevelopmentEntity development, bool isHost){
        if(isHost) _hostDevelopments.Add(development);
        else _clientDevelopments.Add(development);

        // MoveEntityToHolder(development.gameObject.GetComponent<BattleZoneEntity>());
        ResetHolders();
    }

    public void RemoveDevelopment(DevelopmentEntity development, bool isHost){
        if(isHost) _hostDevelopments.Remove(development);
        else _clientDevelopments.Remove(development);
    }

    public List<CreatureEntity> GetCreatures(bool isHost){
        if(isHost) return _hostCreatures;
        else return _clientCreatures;
    }

    public void AddCreature(CreatureEntity creature, bool isHost){
        if(isHost) _hostCreatures.Add(creature);
        else _clientCreatures.Add(creature);

        // MoveEntityToHolder(creature.gameObject.GetComponent<BattleZoneEntity>());
        ResetHolders();
    }

    public void RemoveCreature(CreatureEntity creature, bool isHost){
        if(isHost) _hostCreatures.Remove(creature);
        else _clientCreatures.Remove(creature);
    }

    [ClientRpc]
    public void RpcMoveEntityToHolder(BattleZoneEntity entity)
    {
        var targetTransform = FindHolderTransform(entity);
        if(!targetTransform) {
            print("No free holders found! Aborting to play entity...");
            return;
        }
        entity.transform.SetParent(targetTransform, false);
        entity.transform.localPosition = Vector3.zero;
    }

    #region Entity holders
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

    public void HighlightCardHolders(TurnState state)
    {
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
    #endregion
}