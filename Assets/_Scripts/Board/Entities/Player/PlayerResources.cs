using System;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    [SerializeField] private PlayerEntityUI playerEntityUI;
    [SerializeField] private PlayerEntityUI opponentEntityUI;
    public PlayerResourcesUI playerResourcesUI;
    public PlayerResourcesUI opponentResourcesUI;

    internal void SetPlayerEntity(bool isOwned, BattleZoneEntity entity, string name)
    {
        if(isOwned) {
            entity.SetPlayer(name, playerEntityUI);
            playerEntityUI.SetEntity(entity);
        } else {
            entity.SetPlayer(name, opponentEntityUI);
            opponentEntityUI.SetEntity(entity);
        }
    }

    public void UISetPlayerName(bool isOwned, string newValue)
    {
        if (isOwned) playerEntityUI.SetName(newValue);
        else opponentEntityUI.SetName(newValue);
    }
    
    public void UISetHealth(bool isOwned, int newValue)
    {
        if (isOwned) playerEntityUI.SetHealth(newValue);
        else opponentEntityUI.SetHealth(newValue);
    }

    public void UISetScore(bool isOwned, int newValue)
    {
        if (isOwned) playerEntityUI.SetScore(newValue);
        else opponentEntityUI.SetScore(newValue);
    }

    public void UISetBuys(bool isOwned, int newValue)
    {
        if (isOwned) playerResourcesUI.SetBuys(newValue);
        else opponentResourcesUI.SetBuys(newValue);
    }

    public void UISetPlays(bool isOwned, int newValue)
    {
        if (isOwned) playerResourcesUI.SetPlays(newValue);
        else opponentResourcesUI.SetPlays(newValue);
    }

    public void UISetPrevails(bool isOwned, int newValue)
    {
        if (isOwned) playerResourcesUI.SetPrevails(newValue);
        else opponentResourcesUI.SetPrevails(newValue);
    }
}
