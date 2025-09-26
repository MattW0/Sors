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
    public Transform objectTransform;

    public void InitTransformable(Transform t) 
    {
        rectTransform = GetComponent<RectTransform>();
        objectTransform = t;
    }

    public void StartTransform(TransformationSetting setting, float time = -1)
	{
        if (time == -1) time = SorsTimings.cardPileRearrangement;

		var endValue = new Vector2(setting.width, setting.height);
        rectTransform.DOSizeDelta(endValue, time);
	}

    public void StartMove(Vector3 position, float scale = 1f) 
    {
        objectTransform.DOLocalMove(position, SorsTimings.cardPileRearrangement);
        objectTransform.DOScale(new Vector3(scale, scale, 1f), SorsTimings.cardPileRearrangement);
    }
}
