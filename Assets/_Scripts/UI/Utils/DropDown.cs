using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropDown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private GameOption option;

    private void Start()
    {
        dropdown.onValueChanged.AddListener(delegate { UpdateDropDown(); });

        // To get initial setting to network manager
        UpdateDropDown();
    }

    public void UpdateDropDown()
    {
        var value = dropdown.value;

        if(option == GameOption.NumberPlayers) SorsNetworkManager.SetNumberPlayers(value);
        else if(option == GameOption.NumberPhases) SorsNetworkManager.SetNumberPhases(value);
    }
}
