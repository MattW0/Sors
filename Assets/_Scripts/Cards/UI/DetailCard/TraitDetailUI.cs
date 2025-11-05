using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TraitDetailUI : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text description;
    public Image icon;

    internal void Initialize(Trait trait)
    {
        title.text = trait.ToString();
        description.text = trait.GetDescription();
        icon.sprite = trait.GetSprite();
    }
}
