using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode, RequireComponent(typeof(Image))]
public class SetColor : MonoBehaviour
{
    public SorsColors SorsColorPalette;
    [SerializeField] private ColorType colorType;
    [SerializeField] private Image image;
    [Range(0, 1), SerializeField] private float alpha = 1f;
    private bool _dynamicUpdates;

    void OnEnable()
    {
        try { SorsColorPalette = Resources.Load<SorsColors>("Sors Colors"); }
        catch { Debug.Log("No Sors Colors found. Assign it manually, otherwise it won't work properly.", this); }
    
        image = GetComponent<Image>();
        _dynamicUpdates = SorsColorPalette.enableDynamicUpdate;

        Set();
    }
    
    void LateUpdate()
    {
        if(SorsColorPalette == null) return;
        
        if(_dynamicUpdates) Set();
    }

    private void Set()
    {
        var color = colorType switch
        {
            ColorType.Player => SorsColorPalette.player,
            ColorType.Opponent => SorsColorPalette.opponent,
            ColorType.Creature => SorsColorPalette.creature,
            ColorType.Technology => SorsColorPalette.technology,
            ColorType.Money => SorsColorPalette.money,
            ColorType.Cost => SorsColorPalette.costValue,
            ColorType.Attack => SorsColorPalette.attackValue,
            ColorType.Health => SorsColorPalette.healthValue,
            ColorType.Points => SorsColorPalette.pointsValue,

            ColorType.Neutral => SorsColors.neutral,
            ColorType.NeutralDark => SorsColors.neutralDark,
            ColorType.NeutralLight => SorsColors.neutralLight,
            _ => Color.white
        };

        if (alpha < 1f) color.a = alpha;
        image.color = color;
    }
}
