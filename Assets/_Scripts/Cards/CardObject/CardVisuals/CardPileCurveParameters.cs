using UnityEngine;

[CreateAssetMenu(fileName = "CardPileCurveParameters", menuName = "Card Pile Curve Parameters")]
public class CardPileCurveParameters : ScriptableObject
{
    public AnimationCurve positioning;
    public float positioningInfluence = .1f;
    public AnimationCurve rotation;
    public float rotationInfluence = 10f;
}