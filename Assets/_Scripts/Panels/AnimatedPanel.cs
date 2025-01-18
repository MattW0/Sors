using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(CanvasGroup))]
public class AnimatedPanel : MonoBehaviour
{
    private Animator _animator;
    private CanvasGroup _canvasGroup;

    public virtual void Awake()
    {
        _animator = gameObject.GetComponent<Animator>();
        _canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }

    public void PanelIn() => _animator.Play("Panel In");
    public void PanelOut() => _animator.Play("Panel Out");
}
