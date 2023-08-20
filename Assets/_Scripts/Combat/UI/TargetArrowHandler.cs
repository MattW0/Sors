using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetArrowHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private BattleZoneEntity entity;
    [SerializeField] private GameObject arrowPrefab;
    private ArrowRenderer _arrow;
    private bool _hasTarget;
    public void SpawnArrow()
    {
        var obj = Instantiate(arrowPrefab);
        _arrow = obj.GetComponent<ArrowRenderer>();

        var origin = entity.transform.position;
        origin.y = 0.5f;
        _arrow.SetOrigin(origin);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // return if not in Blockers Phase
        if (!entity.Targetable) return;

        _arrow.SetTarget(entity.transform.position);
        _hasTarget = true;
    }

    private void FixedUpdate()
    {
        if (!_arrow || _hasTarget) return;
        var input = new Vector3(Input.mousePosition.x, 0.5f, Input.mousePosition.y);

        // Input range X: [0, 1920], Y: 0, Z: [0, 1080]
        // Arrow renderer range X: [-9.7, 9.7], Y: 0.5, Z: [-5.5, 5.5]

        // X: [0, 1920] -> X: [-9.7, 9.7]
        input.x = (input.x / 1920f) * 19.4f - 9.7f;

        // Z: [0, 1080] -> Z: [-5.5, 5.5]
        input.z = (input.z / 1080f) * 11f - 5.5f;

        // clamp
        input.x = Mathf.Clamp(input.x, -9.7f, 9.7f);
        input.z = Mathf.Clamp(input.z, -5.5f, 5.5f);

        _arrow.SetTarget(input);
    }
}
