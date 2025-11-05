using UnityEngine;

[CreateAssetMenu(fileName = "CardPileSettings", menuName = "Card Pile/Settings")]
public class CardPileSettings : TransformationSetting
{
	public bool isHorizontalLayout;
}

public class TransformationSetting : ScriptableObject
{
    public Vector3 position;
    public float scale = 1.2f;
	public float height;
}