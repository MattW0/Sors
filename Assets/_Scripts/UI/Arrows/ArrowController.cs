using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ArrowController : MonoBehaviour
{
    public IArrowRenderer arrow;

    [Header("Control")]
    public bool followMouse = true;
    private bool _hasTarget;

    private void Awake()
    {
        arrow = GetComponent<IArrowRenderer>();
        DropZoneManager.OnDestroyArrows += DestroyArrow;
    }


    public void SetOrigin(Vector3 origin) => arrow.SetOrigin(origin);
    public void SetTarget(Vector3 end)
    {
        _hasTarget = true;
        arrow.SetTarget(end);
    }

    private void FixedUpdate()
    {
        if (! followMouse) return;

        if (_hasTarget) return;
        arrow.FollowMouse();
    }

    public void DestroyArrow()
    {
        DropZoneManager.OnDestroyArrows -= DestroyArrow;
        Destroy(gameObject);
    }
}
