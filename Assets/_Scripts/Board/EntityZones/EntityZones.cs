using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DG.Tweening;

public class EntityZones : NetworkBehaviour
{
    [SerializeField] private List<CreatureEntity> _hostCreatures = new();
    [SerializeField] private List<CreatureEntity> _clientCreatures = new();
    [SerializeField] private List<TechnologyEntity> _hostTechnologies = new();
    [SerializeField] private List<TechnologyEntity> _clientTechnologies = new();
    [SerializeField] private Transform _spawnedEntityTransform;
    [SerializeField] private PlayZoneCardHolder[] _playerTechnologyHolders;
    [SerializeField] private PlayZoneCardHolder[] _playerCreatureHolders;
    [SerializeField] private PlayZoneCardHolder[] _opponentTechnologyHolders;
    [SerializeField] private PlayZoneCardHolder[] _opponentCreatureHolders;

    [Server]
    public void AddEntity(BattleZoneEntity entity, bool isHost)
    {
        if (entity.cardType == CardType.Technology)
        {
            var technology = entity.GetComponent<TechnologyEntity>();
            if(isHost) _hostTechnologies.Add(technology);
            else _clientTechnologies.Add(technology);
        }
        else if (entity.cardType == CardType.Creature)
        {
            var creature = entity.GetComponent<CreatureEntity>();
            if(isHost) _hostCreatures.Add(creature);
            else _clientCreatures.Add(creature);
        }

        ResetHolders();
    }

    [Server]
    public void RemoveTechnology(TechnologyEntity technology, bool isHost)
    {
        if(isHost) _hostTechnologies.Remove(technology);
        else _clientTechnologies.Remove(technology);
    }

    [Server]
    public void RemoveCreature(CreatureEntity creature, bool isHost)
    {
        if(isHost) _hostCreatures.Remove(creature);
        else _clientCreatures.Remove(creature);
    }

    [Server]
    public List<CreatureEntity> GetCreatures(bool isHost)
    {
        if(isHost) return _hostCreatures;
        else return _clientCreatures;
    }

    public List<CreatureEntity> GetAllCreatures()
    {
        var creatures = new List<CreatureEntity>();
        creatures.AddRange(_hostCreatures);
        creatures.AddRange(_clientCreatures);

        return creatures;
    }

    [Server]
    public List<TechnologyEntity> GetTechnologies(bool isHost)
    {
        if(isHost) return _hostTechnologies;
        else return _clientTechnologies;
    }

    public List<TechnologyEntity> GetAllTechnologies()
    {
        var technologies = new List<TechnologyEntity>();
        technologies.AddRange(_hostTechnologies);
        technologies.AddRange(_clientTechnologies);

        return technologies;
    }

    public List<BattleZoneEntity> GetAllEntities()
    {
        var technologies = GetAllTechnologies();
        var creatures = GetAllCreatures();

        var entities = new List<BattleZoneEntity>();
        foreach (var technology in technologies){
            entities.Add(technology.GetComponent<BattleZoneEntity>());
        }
        foreach (var creature in creatures){
            entities.Add(creature.GetComponent<BattleZoneEntity>());
        }

        return entities;
    }

    [ClientRpc]
    public void RpcMoveEntityToSpawned(BattleZoneEntity e)
    {
        e.transform.SetParent(_spawnedEntityTransform, false);
        e.gameObject.SetActive(true);
    }

    [ClientRpc]
    public void RpcMoveEntityToHolder(BattleZoneEntity entity)
    {
        var targetTransform = FindHolderTransform(entity);
        if(!targetTransform) {
            // TODO: Should check this at beginning of playCard phase -> dont allow it there
            print("No free holders found! Aborting to play entity...");
            return;
        }

        // TODO: Not so nice doing hard coded scaling here ...
        entity.transform.DOScale(0.33f, SorsTimings.cardMoveTime);
        entity.transform.DOMove(targetTransform.position, SorsTimings.cardMoveTime)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() => {
                entity.transform.SetParent(targetTransform, true);
                entity.transform.localScale = Vector3.one;
            });
    }

    #region Entity holders
    private Transform FindHolderTransform(BattleZoneEntity entity)
    {
        var index = 0;
        if(entity.isOwned){
            if(entity.cardType == CardType.Technology){
                index = GetFirstFreeHolderIndex(_playerTechnologyHolders);
                return _playerTechnologyHolders[index].transform;
            } else if(entity.cardType == CardType.Creature){
                index = GetFirstFreeHolderIndex(_playerCreatureHolders);
                return _playerCreatureHolders[index].transform;
            }
        }
        
        // Opponent Entity
        if(entity.cardType == CardType.Technology){
            index = GetFirstFreeHolderIndex(_opponentTechnologyHolders);
            return _opponentTechnologyHolders[index].transform;
        } else if(entity.cardType == CardType.Creature){
            index = GetFirstFreeHolderIndex(_opponentCreatureHolders);
            return _opponentCreatureHolders[index].transform;
        }
        
        // Returning null if no free holders found 
        return null;
    }

    private int GetFirstFreeHolderIndex(PlayZoneCardHolder[] holders)
    {
        for (int i = 0; i < holders.Length; i++)
        {
            // Holder has Entity -> continue
            if(holders[i].transform.GetComponentInChildren<BattleZoneEntity>()) continue;
            
            return i;
        }
        
        return -1;
    }

    public void HighlightCardHolders(TurnState state)
    {
        if(state == TurnState.Develop) HighlightTechnologyHolders();
        else if (state == TurnState.Deploy) HighlightCreatureHolders();
    }

    public void HighlightTechnologyHolders()
    {
        foreach (var holder in _playerTechnologyHolders) {
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
        foreach (var holder in _playerTechnologyHolders) {
            holder.ResetHighlight();
        }
        foreach (var holder in _playerCreatureHolders) {
            holder.ResetHighlight();
        }
    }
    #endregion
}
