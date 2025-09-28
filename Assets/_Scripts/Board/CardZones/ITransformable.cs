using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public interface ITransformable
{
    [SerializeField] public TransformationSetting Default { get; set; }
	[SerializeField] public TransformationSetting Transformed { get; set; }
    public UniTask TransformTask(CancellationToken ct, float width, float time);
}

public abstract class Transformable : MonoBehaviour, ITransformable
{
    public TransformationSetting Default { get; set; }
	public TransformationSetting Transformed { get; set; }
	private RectTransform rectTransform;
    private Transform objectTransform;
    private float _height;

    public void InitTransformable(Transform t) 
    {
        rectTransform = GetComponent<RectTransform>();
        objectTransform = t;
    }

    public async UniTask MoveTask(CancellationToken ct, TransformationSetting setting, float duration) 
    {
        _height = setting.height;

        // Wait to complete task gave to argument.
        await UniTask.WhenAll(
            StartScale(ct, setting.scale, duration),
            StartMove(ct, setting.position, duration)
        );
    }

    public async UniTask TransformTask(CancellationToken ct, float width, float duration)
    {
        await UniTask.WhenAll(
            StartWidthTransform(ct, width, duration)
        );
    }

    private async UniTask StartWidthTransform(CancellationToken ct, float width, float time)
	{
        await rectTransform.DOSizeDelta(new Vector2(width, _height), time)
            .Play().ToUniTask(cancellationToken: ct);
	}

    private async UniTask StartMove(CancellationToken ct, Vector3 position, float time) 
    {
        await objectTransform.DOLocalMove(position, time)
            .Play().ToUniTask(cancellationToken: ct);

    }
    private async UniTask StartScale(CancellationToken ct, float scale, float time)
    {
        await objectTransform.DOScale(new Vector3(scale, scale, 1f), time)
            .Play().ToUniTask(cancellationToken: ct);
    }
}
