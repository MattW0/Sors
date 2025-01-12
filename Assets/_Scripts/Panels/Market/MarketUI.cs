using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class MarketUI : AnimatedPanel
{
    [SerializeField] private RectTransform _panels;
    [SerializeField] private AnimatedButton _technologiesButton;
    [SerializeField] private AnimatedButton _creaturesButton;
    [SerializeField] private AnimatedButton _closeButton;
    [SerializeField] private Image _technologiesCallToAction;
    [SerializeField] private Image _creaturesCallToAction;

    [Range(-1252f, 0)]
    public float creaturePanelOffset = -1252f;
    [Range(0, 2)] public float panelTransitionTime = 0.7f;
    private bool _isOpen;

    private void Start()
    {
        _technologiesButton.GetComponent<Button>().onClick.AddListener(ShowTechnologyPanel);
        _creaturesButton.GetComponent<Button>().onClick.AddListener(ShowCreaturePanel);
        _closeButton.GetComponent<Button>().onClick.AddListener(MinButton);

        PlayerInterfaceButtons.OnOpenMarket += MaxButton;
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

    public void BeginPhase(TurnState phase)
    {
        MaxButton();
        if(phase == TurnState.Invent) {
            ShowTechnologyPanel();
            _technologiesCallToAction.color = UIManager.ColorPalette.callToAction;
            _technologiesButton.StartCallToAction();
        } else {
            ShowCreaturePanel();
            _creaturesCallToAction.color = UIManager.ColorPalette.callToAction;
            _creaturesButton.StartCallToAction();
        }
    }

    internal void EndPhase()
    {
        _technologiesCallToAction.color = UIManager.ColorPalette.neutralLight;
        _creaturesCallToAction.color = UIManager.ColorPalette.neutralLight;
        MinButton();
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

    private void OnDestroy()
    {
        PlayerInterfaceButtons.OnOpenMarket -= MaxButton;
    }
}
