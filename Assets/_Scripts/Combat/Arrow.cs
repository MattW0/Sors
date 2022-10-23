using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [RequireComponent(typeof(LineRenderer))]
public class Arrow : MonoBehaviour
{
    public float width = 0.01f;
    public Color color = Color.red;
    
    // [SerializeField] private LineRenderer lineRenderer;
    // private Camera _cam;
    [SerializeField] private RectTransform lineTransform;
    private Vector3 _anchorPoint, _targetPoint;
    public void SetAnchor(Vector3 point)
    {
        print("Updated anchor with: " + point);
        _anchorPoint = point;
        lineTransform.position = _anchorPoint;
    } 
    
    private Vector2 _sd;
    
    private readonly float _initialLength = 100f;
    private readonly Vector3 _horizontalLeft = new(0f, 0f, 90f);
    private readonly Vector3 _horizontalRight = new(0f, 0f, -90f);
 
    private void Awake() {
        // _anchorPoint = transform.position;
        _sd = lineTransform.sizeDelta;

        // _cam = Camera.main;
        // lineRenderer.enabled = true;
        // lineRenderer.sortingOrder = 1;
        // lineRenderer.material = new Material (Shader.Find ("Sprites/Default"))
        // {
        //     color = color
        // };
        // lineRenderer.widthMultiplier = width;
        // lineRenderer.positionCount = _linePoints.Length;

    }
    private void LateUpdate () {
        // _anchorPoint = Vector3.zero;
        _targetPoint = Input.mousePosition;
        var distance = _targetPoint - _anchorPoint;

        // Change height
        var mag = distance.magnitude;
        var lengthChange = mag - _initialLength;
        lineTransform.sizeDelta = new Vector2(_sd.x, _sd.y + lengthChange);

        // Change rotation
        var rot = new Quaternion();
        if (distance.y < 0) // clamp orientation in "forward" direction
        {
            rot.eulerAngles = distance.x >= 0 ? _horizontalRight : _horizontalLeft;
            lineTransform.rotation = rot;
            return;
        }
        
        var angleChange = MathF.Acos(distance.x / mag) * Mathf.Rad2Deg;
        if (angleChange is Single.NaN) return;

        rot.eulerAngles = new Vector3(0f, 0f, angleChange - 90f); // dafuq
        lineTransform.rotation = rot;

        // lineRenderer.SetPositions (_linePoints);
    }
}
