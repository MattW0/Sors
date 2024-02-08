using UnityEngine;
using UnityEngine.UI;

public class CheckBox : MonoBehaviour
{
    private Toggle _toggle;
    [SerializeField] private GameOption option;

    private void Start(){
        //Fetch the Toggle GameObject
        _toggle = GetComponent<Toggle>();
        //Add listener for when the state of the Toggle changes, to take action
        _toggle.onValueChanged.AddListener(delegate {
                ToggleValueChanged();
            });
    }

    //Output the new state of the Toggle into Text
    void ToggleValueChanged()
    {
        var value = _toggle.isOn;

        if(option == GameOption.SinglePlayer) GameOptionsMenu.SetSinglePlayer(value);
        else if(option == GameOption.FullHand) GameOptionsMenu.SetFullHand(value);
        else if(option == GameOption.SkipCardSpawnAnimations) GameOptionsMenu.SetSpawnimations(value);
    }
}
