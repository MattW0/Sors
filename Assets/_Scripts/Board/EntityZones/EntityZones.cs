using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class EntityZones : NetworkBehaviour
{
    // List -> Server logic (which entity belongs to which player) 
    [SerializeField] private List<TechnologyEntity> _clientTechnologies = new();
    [SerializeField] private List<CreatureEntity> _clientCreatures = new();
    [SerializeField] private List<CreatureEntity> _hostCreatures = new();
    [SerializeField] private List<TechnologyEntity> _hostTechnologies = new();

    // PlayZoneCardHolder[] -> Client logic (which entity is where)
    [SerializeField] private PlayZoneCardHolder[] _opponentTechnologyHolders;
    [SerializeField] private PlayZoneCardHolder[] _opponentCreatureHolders;
    [SerializeField] private PlayZoneCardHolder[] _playerCreatureHolders;
    [SerializeField] private PlayZoneCardHolder[] _playerTechnologyHolders;
    [SerializeField] private Transform _spawnedEntityTransform;

    [ClientRpc]
    public void RpcAddEntity(BattleZoneEntity entity, bool isHost)
    {
        // Assign free holder to later make entity move there
        entity.EntityHolder = GetFirstFreeHolder(entity.cardType, entity.isOwned);
        entity.EntityHolder.IsOccupied = true;

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
    }

    [ClientRpc]
    public void RpcRemoveEntity(BattleZoneEntity entity, bool isHost)
    {
        entity.EntityHolder.IsOccupied = false;
        entity.EntityHolder = null;

        if (entity.cardType == CardType.Technology)
        {
            var technology = entity.GetComponent<TechnologyEntity>();
            if(isHost) _hostTechnologies.Remove(technology);
            else _clientTechnologies.Remove(technology);
        }
        else if (entity.cardType == CardType.Creature)
        {
            var creature = entity.GetComponent<CreatureEntity>();
            if(isHost) _hostCreatures.Remove(creature);
            else _clientCreatures.Remove(creature);
        }
    }

    #region Getters

    [Server]
    public List<CreatureEntity> GetCreatures(bool isHost)
    {
        if(isHost) return _hostCreatures;
        else return _clientCreatures;
    }

    [Server]
    public List<CreatureEntity> GetAllCreatures()
    {
        var creatures = GetCreatures(true);
        creatures.AddRange(GetCreatures(false));

        return creatures;
    }

    [Server]
    public List<TechnologyEntity> GetTechnologies(bool isHost)
    {
        if(isHost) return _hostTechnologies;
        else return _clientTechnologies;
    }

    [Server]
    public List<TechnologyEntity> GetAllTechnologies()
    {
        var technologies = GetTechnologies(true);
        technologies.AddRange(GetTechnologies(false));

        return technologies;
    }

    [Server]
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
    #endregion

    #region Entity holders

    [ClientRpc]
    public void RpcMoveEntityToSpawned(BattleZoneEntity e)
    {
        e.transform.SetParent(_spawnedEntityTransform, false);
        e.gameObject.SetActive(true);
    }

    [Client]
    public void HighlightCardHolders(TurnState state)
    {
        if(state == TurnState.Develop) HighlightTechnologyHolders();
        else if (state == TurnState.Deploy) HighlightCreatureHolders();
    }

    [Client]
    public void HighlightTechnologyHolders()
    {
        foreach (var holder in _playerTechnologyHolders) {
            if(holder.IsOccupied) continue;
            holder.SetHighlight();
        }
    }

    [Client]
    public void HighlightCreatureHolders()
    {
        foreach (var holder in _playerCreatureHolders) {
            if(holder.IsOccupied) continue;
            holder.SetHighlight();
        }
    }

    [Client]
    public void ResetHolders()
    {
        foreach (var holder in _playerTechnologyHolders) {
            holder.ResetHighlight();
        }
        foreach (var holder in _playerCreatureHolders) {
            holder.ResetHighlight();
        }
    }

    private PlayZoneCardHolder GetFirstFreeHolder(CardType entityType, bool isOwned)
    {
        PlayZoneCardHolder holder = null;
        if(isOwned){
            if(entityType == CardType.Technology) holder = _playerTechnologyHolders.FirstOrDefault(h => !h.IsOccupied);
            if(entityType == CardType.Creature) holder = _playerCreatureHolders.FirstOrDefault(h => !h.IsOccupied);
        } else {
            if(entityType == CardType.Technology) holder = _opponentTechnologyHolders.FirstOrDefault(h => !h.IsOccupied);
            if(entityType == CardType.Creature) holder = _opponentCreatureHolders.FirstOrDefault(h => !h.IsOccupied);
        }

        // TODO: Add function here to make player kill an existing entity if no free holders?
        
        return holder;
    }

    [Server]
    internal int GetNumberOfFreeHolders(bool isHost, TurnState state)
    {
        if(isHost){
            if(state == TurnState.Develop) return _playerTechnologyHolders.Count(h => !h.IsOccupied);
            if(state == TurnState.Deploy) return _playerCreatureHolders.Count(h => !h.IsOccupied);
        } else {
            if(state == TurnState.Develop) return _opponentTechnologyHolders.Count(h => !h.IsOccupied);
            if(state == TurnState.Deploy) return _opponentCreatureHolders.Count(h => !h.IsOccupied);
        }

        return -1;
    }

    #endregion
}
