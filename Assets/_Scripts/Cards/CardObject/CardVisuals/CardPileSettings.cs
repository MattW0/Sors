using UnityEngine;

[CreateAssetMenu(fileName = "CardPileSettings", menuName = "Card Pile/Settings")]
public class CardPileSettings : ScriptableObject
{
    public Vector3 position;
    public Vector3 scale = new(1.2f, 1.2f, 1f);
	public float height;
	public float width;
	[Range(0f, 90f)] public float maxCardAngle;
	public float yPerCard;
	public float zDistance;

    // private CardPileSettings handSettings = new(15f, _handWidthDefault.x, 5f, 2f, -3f);
	// private CardPileSettings selectionSettings = new(0f, 100f, 0.1f, 0.1f, -1f);
	// private CardPileSettings pileSettings = new(20f, 20f, 0f, 1f, -1f);
}