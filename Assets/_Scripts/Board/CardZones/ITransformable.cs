using DG.Tweening;
using UnityEngine;

public interface ITransformable
{
    [SerializeField] public TransformationSetting Default { get; set; }
	[SerializeField] public TransformationSetting Transformed { get; set; }
    public void StartTransform(TransformationSetting setting, float time);
}

public abstract class Transformable : MonoBehaviour, ITransformable
{
    public TransformationSetting Default { get; set; }
	public TransformationSetting Transformed { get; set; }
	public RectTransform rectTransform;

    public void InitTransformable() 
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void StartTransform(TransformationSetting setting, float time = -1)
	{
        if (time == -1) time = SorsTimings.cardPileRearrangement;
		var endValue = new Vector2(setting.width, setting.height);
        rectTransform.DOSizeDelta(endValue, time);
	}
}
