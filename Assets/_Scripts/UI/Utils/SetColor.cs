using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SetColor : MonoBehaviour
{
    [SerializeField] private ColorType colorType;
    [SerializeField] private Image image;
    [Range(0, 1), SerializeField] private float alpha = 1f;
    
// #if UNITY_EDITOR
    void Start()
    {
        var color = colorType switch
        {
            ColorType.Creature => SorsColors.creature,
            ColorType.Technology => SorsColors.technology,
            ColorType.Trash => SorsColors.trash,
            ColorType.Cash => SorsColors.cash,
            ColorType.Cost => SorsColors.costValue,
            ColorType.Attack => SorsColors.attackValue,
            ColorType.Health => SorsColors.healthValue,
            ColorType.Points => SorsColors.pointsValue,
            ColorType.MoneyValue => SorsColors.moneyValue,
            ColorType.Neutral => SorsColors.neutral_dark,
            _ => Color.white
        };

        if (alpha < 1f) color.a = alpha;
        image.color = color;
    }
// #endif
}
