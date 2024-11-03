using UnityEngine;
using Michsky.UI.Shift;

[RequireComponent(typeof(Animator), typeof(CanvasGroup))]
public class ModalWindow : MonoBehaviour, IModalWindow
{
    public bool sharpAnimations = false;
    private BlurManager _blurManager;
    private Animator _mWindowAnimator;
    private CanvasGroup _canvasGroup;

    public void Awake()
    {
        _blurManager = GetComponentInParent<BlurManager>();
        _mWindowAnimator = gameObject.GetComponent<Animator>();
        _canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }
    
    public void WindowIn()
    {
        _blurManager.BlurInAnim();
        _canvasGroup.alpha = 1;

        if (sharpAnimations == false)
            _mWindowAnimator.CrossFade("Window In", 0.1f);
        else
            _mWindowAnimator.Play("Window In");
    }

    public void WindowOut()
    {
        _blurManager.BlurOutAnim();
        _canvasGroup.alpha = 0;

        if (sharpAnimations == false)
            _mWindowAnimator.CrossFade("Window Out", 0.1f);
        else
            _mWindowAnimator.Play("Window Out");

    }
}
