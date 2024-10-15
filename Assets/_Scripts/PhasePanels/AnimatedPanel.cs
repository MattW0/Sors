using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(CanvasGroup))]
public class AnimatedPanel : MonoBehaviour
{
    private Animator _mWindowAnimator;
    private CanvasGroup _canvasGroup;

    public void Awake()
    {
        _mWindowAnimator = gameObject.GetComponent<Animator>();
        _canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }

    public void PanelIn()
    {
        print("Panel In");
        _mWindowAnimator.Play("Panel In");
    }

    public void PanelOut()
    {
        print("Panel Out");
        _mWindowAnimator.Play("Panel Out");
    }
}
