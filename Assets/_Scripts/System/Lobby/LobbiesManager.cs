using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class LobbiesManager : MonoBehaviour
{
    public static LobbiesManager Instance;
    public List<GameObject> lobbiesInstances = new();
    [SerializeField] private GameObject _lobbyDataEntryPrefab;
    [SerializeField] private GameObject _buttonsParent;
    [SerializeField] private GameObject _lobbiesListParent;
    [SerializeField] private Transform _lobbiesListTransform;


    private void Awake()
    {
        if(Instance == null) Instance = this;
    }

    public void GetListOfLobbies()
    {
        _buttonsParent.SetActive(false);
        _lobbiesListParent.SetActive(true);

        SteamLobby.Instance.GetLobbies();
    }

    public void DisplayLobbies(List<CSteamID> lobbyIds, LobbyDataUpdate_t lobbyDataUpdate)
    {
        ClearLobbyDataEntries();
        foreach (var lobbyId in lobbyIds)
        {
            if (lobbyDataUpdate.m_ulSteamIDLobby != lobbyId.m_SteamID) continue;

            var lobbyDataEntry = Instantiate(_lobbyDataEntryPrefab, _lobbiesListTransform).GetComponent<LobbyDataEntry>();
            lobbyDataEntry.SetLobbyData(lobbyId, SteamMatchmaking.GetLobbyData(lobbyId, "name"));
            lobbiesInstances.Add(lobbyDataEntry.gameObject);
        }
    }

    public void ClearLobbyDataEntries()
    {
        foreach (var lobbyDataEntry in lobbiesInstances)
        {
            Destroy(lobbyDataEntry.gameObject);
        }
        lobbiesInstances.Clear();
    }
}
