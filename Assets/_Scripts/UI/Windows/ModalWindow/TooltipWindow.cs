using UnityEngine;

[RequireComponent(typeof(CanvasGroup), typeof(Animator))]
public class TooltipWindow : MonoBehaviour, IModalWindow
{
    public bool sharpAnimations = false;
    private Animator _mWindowAnimator;
    private CanvasGroup _canvasGroup;

    public void Awake()
    {
        _mWindowAnimator = GetComponent<Animator>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void WindowIn()
    {
        _canvasGroup.alpha = 1;

        if (sharpAnimations == false)
            _mWindowAnimator.CrossFade("In", 0.1f);
        else
            _mWindowAnimator.Play("In");
    }

    public void WindowOut()
    {
        _canvasGroup.alpha = 0;

        if (sharpAnimations == false)
            _mWindowAnimator.CrossFade("Out", 0.1f);
        else
            _mWindowAnimator.Play("Out");

    }
}
