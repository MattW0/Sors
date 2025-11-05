using UnityEngine;

public static class MouseInputHelper
{
    static readonly float zDistance = 8f;
    static Camera _cam => Camera.main;

    /// <summary>
    /// Gets the world position for any point on the camera
    /// </summary>
    public static Vector3 GetWorldPositionForScreenPoint(Vector3 screenPosition)
    {
        screenPosition.z = zDistance;
        var worldPos = _cam.ScreenToWorldPoint(screenPosition);

        return worldPos;
    }

    /// <summary>
    /// Gets the world position of the mouse cursor projected onto a plane at z = zDistance = 5f
    /// </summary>
    public static Vector3 GetMouseWorldPosition()
    {
        return GetWorldPositionForScreenPoint(Input.mousePosition);
    }

    /// <summary>
    /// Gets the mouse delta movement in world space between last frame and this frame.
    /// </summary>
    public static Vector3 GetMouseWorldDelta()
    {
        Vector3 previous = GetWorldPositionForScreenPoint(Input.mousePosition);
        Vector3 current = GetMouseWorldPosition();
        return current - previous;
    }

    /// <summary>
    /// Gets the world space top-right bounds of the camera's viewport at a specified z-distance from the camera.
    /// </summary>
    /// <param name="cam">Camera to use for conversion</param>
    /// <returns>World space top-right bounds as Vector2 (x,y)</returns>
    public static Vector2 GetScreenBounds()
    {
        Vector3 topRightScreenPoint = new(Screen.width, Screen.height, zDistance);
        Vector3 worldTopRight = _cam.ScreenToWorldPoint(topRightScreenPoint);

        return new(worldTopRight.x, worldTopRight.y);
    }

    public static Vector3 GetMousePositionWithinBounds()
    {
        Vector3 mouseWorld = GetMouseWorldPosition();

        Vector3 bottomLeft = _cam.ScreenToWorldPoint(new Vector3(0, 0, zDistance));
        Vector3 topRight = GetScreenBounds();

        mouseWorld.x = Mathf.Clamp(mouseWorld.x, bottomLeft.x, topRight.x);
        mouseWorld.y = Mathf.Clamp(mouseWorld.y, bottomLeft.y, topRight.y);

        return mouseWorld;
    }
}
