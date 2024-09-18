using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.Shift;
using UnityEngine.UI;
using System;

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
        StopCoroutine(DisableWindow());
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
        StartCoroutine(DisableWindow());
        _blurManager.BlurOutAnim();

        if (! _isOn) return;

        if (sharpAnimations == false)
            _mWindowAnimator.CrossFade("Window Out", 0.1f);
        else
            _mWindowAnimator.Play("Window Out");

        _isOn = false;
    }

    private IEnumerator DisableWindow()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
}
