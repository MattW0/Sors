using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class AnimatedButton : MonoBehaviour
{
    [Header("Settings")]
    public string buttonText = "My Title";
    public bool useCustomText = false;

    [Header("Resources")]
    public TMP_Text normalText;
    public TMP_Text highlightedText;
    public TMP_Text pressedText;

    void OnEnable()
    {
        if (useCustomText) return;
        
        if (normalText != null) { normalText.text = buttonText; }
        if (highlightedText != null) { highlightedText.text = buttonText; }
        if (pressedText != null) { pressedText.text = buttonText; }
    }
}
