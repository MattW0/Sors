using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;

[ExecuteInEditMode, RequireComponent(typeof(Animator))]
public class AnimatedButton : MonoBehaviour
{
    [Header("Settings")]
    public int fontSize = 36;
    public string buttonText = "My Title";
    public bool changeTextOnClick = false;

    [Header("Resources")]
    public TMP_Text normalText;
    public TMP_Text highlightedText;
    public TMP_Text pressedText;

    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void StartCallToAction()
    {
        // TODO: How to implement this? Animation gets overwritten by other state 
        // Button has only 4 conditions (Normal, Highlighted, Pressed, Disabled) and is triggered by Button component (?)
        // Also do on other elements (text, images, etc)
        _animator.Play("CallToAction");
    }

    void OnEnable()
    {
        normalText.fontSize = fontSize;
        highlightedText.fontSize = fontSize;
        pressedText.fontSize = fontSize;

        if (changeTextOnClick) return;

        if (normalText != null) { normalText.text = buttonText; }
        if (highlightedText != null) { highlightedText.text = buttonText; }
        if (pressedText != null) { pressedText.text = buttonText; }
    }
}
