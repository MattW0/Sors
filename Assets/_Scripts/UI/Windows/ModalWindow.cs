using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Michsky.UI.Shift;

public class ModalWindow : MonoBehaviour, IModalWindow
{
    [Header("Resources")]
    [SerializeField] private TMP_Text _windowTitle;
    [SerializeField] private TMP_Text _windowDescription;
    [SerializeField] private AnimatedButton _acceptButton;
    [SerializeField] private AnimatedButton _declineButton;
    [SerializeField] private ModalWindowType _windowType;

    [Header("Settings")]
    public bool sharpAnimations = false;
    public string titleText = "Title";
    [TextArea] public string descriptionText = "Description here";
    public string acceptButtonText = "Ok";
    public string declineButtonText = "Cancel";

    private BlurManager _blurManager;
    private Animator _mWindowAnimator;
    private bool _isOn;
    public static event Action<ModalWindowType> OnAccept;
    public static event Action<ModalWindowType> OnDecline;

    void Start()
    {
        _mWindowAnimator = gameObject.GetComponent<Animator>();
        _blurManager = GetComponentInParent<BlurManager>();

        _windowTitle.text = titleText;
        _windowDescription.text = descriptionText;

        _acceptButton.buttonText = acceptButtonText;
        _declineButton.buttonText = declineButtonText;

        _acceptButton.gameObject.GetComponent<Button>().onClick.AddListener(Accept);
        _declineButton.gameObject.GetComponent<Button>().onClick.AddListener(Decline);

        gameObject.SetActive(false);
    }

    public void Accept()
    {
        OnAccept?.Invoke(_windowType);
        ModalWindowOut();
    }

    private void Decline()
    {
        OnDecline?.Invoke(_windowType);
        ModalWindowOut();
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

    IEnumerator DisableWindow()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }

    public void SetMessage(string message)
    {
        _windowDescription.text = message;
    }
}

public enum ModalWindowType
{
    INFO = 0,
    WARNING = 1,
    ERROR = 2,
    EXIT = 10,
    LOBBY_INVITE = 11,
}