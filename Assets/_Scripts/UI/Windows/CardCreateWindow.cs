using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.Shift;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;

public class CardCreateWindow : MonoBehaviour, IModalWindow
{
    [SerializeField] private Button _createCard;
    [SerializeField] private Button _cancel;
    public bool sharpAnimations = false;
    private Animator _mWindowAnimator;
    private BlurManager _blurManager;
    private bool _isOn;

    void Start()
    {
        _mWindowAnimator = gameObject.GetComponent<Animator>();
        _blurManager = GetComponentInParent<BlurManager>();

        _createCard.onClick.AddListener(CreateCard);
        _cancel.onClick.AddListener(ModalWindowOut);
    }

    private void CreateCard()
    {
        throw new NotImplementedException();
    }

    public void ModalWindowIn()
    {
        // DisableWindow().Forget();
        gameObject.SetActive(true);
        _blurManager.BlurInAnim();

        if (_isOn) return;
        _isOn = true;

        if (sharpAnimations == false)
            _mWindowAnimator.CrossFade("Window In", 0.1f);
        else
            _mWindowAnimator.Play("Window In");
    }

    public void ModalWindowOut()
    {
        DisableWindow().Forget();
        _blurManager.BlurOutAnim();

        if (! _isOn) return;

        if (sharpAnimations == false)
            _mWindowAnimator.CrossFade("Window Out", 0.1f);
        else
            _mWindowAnimator.Play("Window Out");

        _isOn = false;
    }

    private async UniTask DisableWindow()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        gameObject.SetActive(false);
    }
}
