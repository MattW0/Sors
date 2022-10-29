using System;
using UnityEngine;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float widhtFactor = 1f;
    [SerializeField] private Color color = Color.red;
    [SerializeField] private Image image; 
    
    // [SerializeField] private LineRenderer lineRenderer;
    // private Camera _cam;
    [SerializeField] private RectTransform lineTransform;
    private Vector3 _anchorPoint, _targetPoint;

    private const float InitialLength = 100f;
    private readonly Vector3 _horizontalLeft = new(0f, 0f, 90f);
    private readonly Vector3 _horizontalRight = new(0f, 0f, -90f);
    private Vector2 _sd;
    private bool _foundTarget;
 
    private void Awake() {
        _sd = lineTransform.sizeDelta;
        lineTransform.sizeDelta = new Vector2(_sd.x * widhtFactor, _sd.y);
        image.color = color;
    }
    
    public void SetAnchor(Vector3 point)
    {
        _anchorPoint = point;
        lineTransform.position = _anchorPoint;
    }

    public void FoundTarget(Vector3 targetPoint)
    {
        _foundTarget = true;
        ChangeRectShape(targetPoint);
    }
    
    private void LateUpdate () {
        if (_foundTarget) return;
        
        _targetPoint = Input.mousePosition;
        ChangeRectShape(_targetPoint);
    }

    private void ChangeRectShape(Vector3 targetPoint)
    {
        var distance = targetPoint - _anchorPoint;

        // Change height
        var magnitude = distance.magnitude;
        var lengthChange = magnitude - InitialLength;
        lineTransform.sizeDelta = new Vector2(_sd.x, _sd.y + lengthChange);
        
        UpdateRotation(distance, magnitude);
    }

    private void UpdateRotation(Vector3 distance, float mag)
    {
        var rot = new Quaternion();
        if (distance.y < 0) // clamp orientation in "forward" direction
        {
            rot.eulerAngles = distance.x >= 0 ? _horizontalRight : _horizontalLeft;
            lineTransform.rotation = rot;
            return;
        }
        
        var angleChange = MathF.Acos(distance.x / mag) * Mathf.Rad2Deg;
        if (angleChange is float.NaN) return;

        rot.eulerAngles = new Vector3(0f, 0f, angleChange - 90f); // dafuq
        lineTransform.rotation = rot;
    }
}
