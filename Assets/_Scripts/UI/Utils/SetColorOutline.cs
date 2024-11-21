using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode, RequireComponent(typeof(Outline))]
public class SetColorOutline : MonoBehaviour
{
    public SorsColors SorsColorPalette;
    [SerializeField] private ColorType colorType = ColorType.Neutral;
    [SerializeField] private Outline outline;
    [Range(0, 1), SerializeField] private float alpha = 1f;
    private bool _dynamicUpdates;

    void OnEnable()
    {
        try { SorsColorPalette = Resources.Load<SorsColors>("Sors Colors"); }
        catch { Debug.Log("No Sors Colors found. Assign it manually, otherwise it won't work properly.", this); }
    
        outline = GetComponent<Outline>();
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
            ColorType.Neutral => SorsColors.neutral,
            ColorType.NeutralDark => SorsColors.neutralDark,
            ColorType.NeutralLight => SorsColors.neutralLight,
            _ => Color.white
        };

        if (alpha < 1f) color.a = alpha;
        outline.effectColor = color;
    }
}
