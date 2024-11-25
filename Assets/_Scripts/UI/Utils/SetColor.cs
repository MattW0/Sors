using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode, RequireComponent(typeof(Image))]
public class SetColor : MonoBehaviour
{
    [SerializeField] private ColorType colorType;
    [SerializeField] private Image image;
    [Range(0, 1), SerializeField] private float alpha = 1f;
    private SorsColors _colorPalette;
    private bool _dynamicUpdates;

    void OnEnable()
    {
        try { _colorPalette = Resources.Load<SorsColors>("Sors Colors"); }
        catch { Debug.Log("No Sors Colors found at Resources/Sors Colors. Assign it manually on " + gameObject.name, this); }
    
        image = GetComponent<Image>();
        _dynamicUpdates = _colorPalette.enableDynamicUpdate;

        Set();
    }
    
    void LateUpdate()
    {
        if(_colorPalette == null) return;
        
        if(_dynamicUpdates) Set();
    }

    private void Set()
    {
        var color = colorType switch
        {
            ColorType.Player => _colorPalette.player,
            ColorType.Opponent => _colorPalette.opponent,
            ColorType.Creature => _colorPalette.creature,
            ColorType.Technology => _colorPalette.technology,
            ColorType.Money => _colorPalette.money,
            ColorType.Cost => _colorPalette.costValue,
            ColorType.Attack => _colorPalette.attackValue,
            ColorType.Health => _colorPalette.healthValue,
            ColorType.Points => _colorPalette.pointsValue,

            ColorType.Neutral => SorsColors.neutral,
            ColorType.NeutralDark => SorsColors.neutralDark,
            ColorType.NeutralLight => SorsColors.neutralLight,
            _ => Color.white
        };

        if (alpha < 1f) color.a = alpha;
        image.color = color;
    }
}
