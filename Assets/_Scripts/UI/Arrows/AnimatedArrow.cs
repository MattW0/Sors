using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AnimatedArrow : BaseArrowRenderer
{
    [Space] [SerializeField] float fadeDistance = 0.35f;
    [SerializeField] float speed = 1f;
    [SerializeField] private GameObject tipPrefab;
    [SerializeField] private GameObject segmentPrefab;

    private Transform _arrow;
    private readonly List<Transform> _segments = new();
    private readonly List<MeshRenderer> _renderers = new();

    protected override float Offset => Time.time * speed;
    protected override float FadeDistance => fadeDistance;

    protected override void UpdateArrow()
    {
        base.UpdateArrow();
        UpdateSegments();
    }

    void UpdateSegments()
    {
        Debug.DrawLine(start, end, Color.yellow);

        CheckSegments(Positions.Count - 1);

        for (var i = 0; i < Positions.Count - 1; i++)
        {
            _segments[i].localPosition = Positions[i];
            _segments[i].localRotation = Rotations[i];

            var meshRenderer = _renderers[i];

            if (!meshRenderer)
                continue;

            var material = meshRenderer.material;

            var currentColor = material.color;
            currentColor.a = Alphas[i];
            material.color = currentColor;
        }

        if (!_arrow)
            _arrow = Instantiate(tipPrefab, transform).transform;

        _arrow.localPosition = Positions.Last();
        _arrow.localRotation = Rotations.Last();

        transform.position = start;
        transform.rotation = Quaternion.LookRotation(end - start, upwards);
    }

    void CheckSegments(int segmentsCount)
    {
        while (_segments.Count < segmentsCount)
        {
            var segment = Instantiate(segmentPrefab, transform).transform;
            _segments.Add(segment);
            _renderers.Add(segment.GetComponent<MeshRenderer>());
        }

        for (var i = 0; i < _segments.Count; i++)
        {
            var segment = _segments[i].gameObject;
            if (segment.activeSelf != i < segmentsCount)
                segment.SetActive(i < segmentsCount);
        }
    }
}