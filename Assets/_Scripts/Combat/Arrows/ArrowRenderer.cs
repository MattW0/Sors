using System.Collections.Generic;
using UnityEngine;

public class ArrowRenderer : MonoBehaviour
{
    [SerializeField] ArrowType arrowType;
    public bool stopUpdating;
    public float height = 0.5f;
    public float segmentLength = 0.5f;
    public float fadeDistance = 0.35f;
    public float speed = 1f;

    [SerializeField] GameObject arrowPrefab;
    [SerializeField] GameObject segmentPrefab;

    [Space] [SerializeField] Vector3 start;
    [SerializeField] Vector3 end;
    [SerializeField] Vector3 upwards = Vector3.up;
    private Transform _arrow;

    readonly List<Transform> segments = new List<Transform>();
    readonly List<MeshRenderer> renderers = new List<MeshRenderer>();

    private void Awake(){
        if (arrowType == ArrowType.Blocker) DropZoneManager.OnDestroyBlockerArrows += DestroyArrow;
        else if (arrowType == ArrowType.Blocker) DropZoneManager.OnDestroyTargetArrows += DestroyArrow;
    }

    public void SetOrigin(Vector3 origin) => start = origin;
    public void SetTarget(){
        var input = new Vector3(Input.mousePosition.x, 0.5f, Input.mousePosition.y);

        // Input range X: [0, 1920], Y: 0, Z: [0, 1080]
        // Arrow renderer range X: [-9.7, 9.7], Y: 0.5, Z: [-5.5, 5.5]
        // X: [0, 1920] -> X: [-9.7, 9.7]
        input.x = (input.x / 1920f) * 19.4f - 9.7f;
        // Z: [0, 1080] -> Z: [-5.5, 5.5]
        input.z = (input.z / 1080f) * 11f - 5.5f;

        // clamp to screen size
        input.x = Mathf.Clamp(input.x, -9.7f, 9.7f);
        input.z = Mathf.Clamp(input.z, -5.5f, 5.5f);

        end = input;
    }

    public void SetTarget(Vector3 target) => end = target;

    private void Update(){
        if (stopUpdating) return;
        UpdateSegments();
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

    private void DestroyArrow(){
        Destroy(gameObject);
    }

    private void OnDestroy(){
        DropZoneManager.OnDestroyBlockerArrows -= DestroyArrow;
        DropZoneManager.OnDestroyTargetArrows -= DestroyArrow;
    }
}

public enum ArrowType{
    Target,
    Blocker
} 