using System.Collections.Generic;
using UnityEngine;

public class ArrowRenderer : MonoBehaviour
{
    public bool stopUpdating;
    public float height = 0.5f;
    public float segmentLength = 0.5f;
    public float fadeDistance = 0.35f;
    public float speed = 1f;

    [Space] [SerializeField] private Vector3 start;
    [SerializeField] private Vector3 end;
    [SerializeField] private Vector3 upwards = Vector3.up;

    [Header("Resolution Constants")]
    [SerializeField] private const float ARROW_RENDER_WIDHT_L = 5f;
    [SerializeField] private const float ARROW_RENDER_WIDHT_R = 3.65f;
    [SerializeField] private const float ARROW_RENDER_HEIGHT = 2.9f;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject segmentPrefab;

    private Transform _arrow;
    private readonly List<Transform> segments = new();
    private readonly List<MeshRenderer> renderers = new();
    private bool _hasTarget;

    private void Awake()
    {
        DropZoneManager.OnDestroyArrows += DestroyArrow;
    }

    public void SetOrigin(Vector3 origin) => start = origin;
    public void SetTarget(Vector3 target)
    {
        end = target;
        _hasTarget = true;
    }

    public void FollowMouse()
    {
        var input = new Vector3(Input.mousePosition.x, 0.5f, Input.mousePosition.y);
        input = ScaleMouseInput(input);

        end = input;
    }

    private void FixedUpdate()
    {
        if (stopUpdating) return;
        UpdateSegments();

        if (_hasTarget) return;
        FollowMouse();
    }

    private Vector3 ScaleMouseInput(Vector3 vec)
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(ray, out var hit);
        vec = hit.point;
        vec.x = Mathf.Clamp(vec.x, -ARROW_RENDER_WIDHT_L, ARROW_RENDER_WIDHT_R);
        vec.z = Mathf.Clamp(vec.z, -ARROW_RENDER_HEIGHT, ARROW_RENDER_HEIGHT);

        return vec;
    }

    private void UpdateSegments()
    {
        Debug.DrawLine(start, end, Color.yellow);

        float distance = Vector3.Distance(start, end);
        float radius = - height / 2f + distance * distance / (8f * height);
        float diff = radius - height;
        float angle = 2f * Mathf.Acos(diff / radius);
        float length = angle * radius;
        float segmentAngle = segmentLength / radius * Mathf.Rad2Deg;

        Vector3 center = new Vector3(0, -diff, distance / 2f);
        Vector3 left = Vector3.zero;
        Vector3 right = new Vector3(0, 0, distance);

        int segmentsCount = (int)(length / segmentLength) + 1;

        CheckSegments(segmentsCount);

        float offset = Time.time * speed * segmentAngle;
        Vector3 firstSegmentPos =
            Quaternion.Euler(Mathf.Repeat(offset, segmentAngle), 0f, 0f) * (left - center) + center;

        float fadeStartDistance = (Quaternion.Euler(segmentAngle / 2f, 0f, 0f) * (left - center) + center).z;

        for (int i = 0; i < segmentsCount; i++)
        {
            Vector3 pos = Quaternion.Euler(segmentAngle * i, 0f, 0f) * (firstSegmentPos - center) + center;
            segments[i].localPosition = pos;
            segments[i].localRotation = Quaternion.FromToRotation(Vector3.up, pos - center);

            MeshRenderer rend = renderers[i];

            if (!rend)
                continue;

            Color currentColor = rend.material.color;
            currentColor.a = GetAlpha(pos.z - left.z, right.z - fadeDistance - pos.z, fadeStartDistance);
            rend.material.color = currentColor;
        }

        if (!_arrow)
            _arrow = Instantiate(arrowPrefab, transform).transform;

        _arrow.localPosition = right;
        _arrow.localRotation = Quaternion.FromToRotation(Vector3.up, right - center);

        transform.position = start;
        transform.rotation = Quaternion.LookRotation(end - start, upwards);
    }

    private void CheckSegments(int segmentsCount)
    {
        while (segments.Count < segmentsCount)
        {
            var segment = Instantiate(segmentPrefab, transform).transform;
            segments.Add(segment);
            renderers.Add(segment.GetComponent<MeshRenderer>());
        }

        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i].gameObject;
            if (segment.activeSelf != i < segmentsCount)
                segment.SetActive(i < segmentsCount);
        }
    }

    private static float GetAlpha(float distance0, float distance1, float distanceMax)
    {
        return Mathf.Clamp01(Mathf.Clamp01(distance0 / distanceMax) + Mathf.Clamp01(distance1 / distanceMax) - 1f);
    }

    public void DestroyArrow()
    {
        DropZoneManager.OnDestroyArrows -= DestroyArrow;
        Destroy(gameObject);
    }
}