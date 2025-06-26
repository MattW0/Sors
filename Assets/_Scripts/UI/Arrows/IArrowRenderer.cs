using UnityEngine;

public interface IArrowRenderer
{
    public void FollowMouse();
    public void SetOrigin(Vector3 origin);
    public void SetTarget(Vector3 end);
    public void SetPositions(Vector3 startPosition, Vector3 endPosition);
}
