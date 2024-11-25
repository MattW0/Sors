using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode, RequireComponent(typeof(Outline))]
public class SetColorOutline : MonoBehaviour
{
    [SerializeField] private ColorType colorType = ColorType.Neutral;
    [SerializeField] private Outline outline;
    [Range(0, 1), SerializeField] private float alpha = 1f;
    private SorsColors _colorPalette;
    private bool _dynamicUpdates;
    

    void OnEnable()
    {
        try { _colorPalette = Resources.Load<SorsColors>("Sors Colors"); }
        catch { Debug.Log("No Sors Colors found. Assign it manually, otherwise it won't work properly.", this); }
    
        outline = GetComponent<Outline>();
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
            ColorType.CallToAction => _colorPalette.callToAction,
            ColorType.Neutral => _colorPalette.neutral,
            ColorType.NeutralDark => _colorPalette.neutralDark,
            ColorType.NeutralLight => _colorPalette.neutralLight,
            _ => Color.white
        };

        if (alpha < 1f) color.a = alpha;
        outline.effectColor = color;
    }
}
