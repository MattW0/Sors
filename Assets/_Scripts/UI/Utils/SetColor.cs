using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SetColor : MonoBehaviour
{
    [SerializeField] private ColorType colorType;
    [SerializeField] private Image image;
    // [SerializeField] private SpriteRenderer spriteRenderer;
    [Range(0, 1), SerializeField] private float alpha = 1f;
    
    void OnEnable()
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
            ColorType.Neutral => SorsColors.neutral,
            ColorType.NeutralDark => SorsColors.neutralDark,
            ColorType.NeutralLight => SorsColors.neutralLight,
            ColorType.PrevailOption => SorsColors.prevailColor,
            ColorType.Player => SorsColors.player,
            ColorType.Opponent => SorsColors.opponent,
            _ => Color.white
        };

        if (alpha < 1f) color.a = alpha;
        
        if (image) image.color = color;
        else Debug.LogWarning("No image found on " + gameObject.name);
    }
}
