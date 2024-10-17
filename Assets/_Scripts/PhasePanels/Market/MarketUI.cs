using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MarketUI : AnimatedPanel
{
    [SerializeField] private RectTransform _panels;
    [SerializeField] private Button _technologiesButon;
    [SerializeField] private Button _creaturesButon;
    [SerializeField] private Button _closeButton;
    public float creaturePanelOffset = -1252f;
    [Range(0, 2)] public float panelTransitionTime = 0.7f;
    private bool _isOpen;

    private void Start()
    {
        _technologiesButon.onClick.AddListener(ShowTechnologyPanel);
        _creaturesButon.onClick.AddListener(ShowCreaturePanel);
        _closeButton.onClick.AddListener(MinButton);

        PlayerInterfaceButtons.OnOpenMarket += MaxButton;
    }

    public void BeginPhase(Phase phase)
    {
        MaxButton();
        if(phase == Phase.Recruit) ShowCreaturePanel();
        else ShowTechnologyPanel();
    }

    private void ShowTechnologyPanel()
    {
        _panels.DOLocalMoveX(0, panelTransitionTime)
            .SetEase(Ease.OutQuint);
    }

    private void ShowCreaturePanel()
    {
        _panels.DOLocalMoveX(creaturePanelOffset, panelTransitionTime)
            .SetEase(Ease.OutQuint);
    }

    public void MaxButton()
    {
        if (_isOpen) PanelOut();
        else PanelIn();

        _isOpen = !_isOpen;
    }
    public void MinButton()
    {
        PanelOut();
        _isOpen = false;
    } 

    private void OnDestroy()
    {
        PlayerInterfaceButtons.OnOpenMarket -= MaxButton;
    }
}
