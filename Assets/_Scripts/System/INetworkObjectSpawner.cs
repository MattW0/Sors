using UnityEngine;

public interface INetworkObjectSpawner
{
    void PlayerGainCard(PlayerManager player, CardInfo cardInfo, CardLocation destination);
    GameObject SpawnCard(PlayerManager player, ScriptableCard card, CardLocation destination);
    BattleZoneEntity SpawnFieldEntity(PlayerManager owner, CardInfo cardInfo);
    void PlayerGainCurse(PlayerManager player);
}