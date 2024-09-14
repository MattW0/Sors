using UnityEngine;
using UnityEngine.UI;

public class CheckBox : MonoBehaviour
{
    [SerializeField] private GameOption option;
    private GameOptionsMenu _options;
    private Toggle _toggle;

    private void Start()
    {
        _options = GetComponentInParent<GameOptionsMenu>();
        
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(delegate { SetOption(); });
        SetOption();
    }

    void SetOption() => _options.SetOption(option, _toggle.isOn);
}
