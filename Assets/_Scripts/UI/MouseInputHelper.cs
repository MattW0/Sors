using System.Runtime.InteropServices;
using UnityEngine;

public static class MouseInputHelper
{
    static readonly float zDistance = 8f;

    /// <summary>
    /// Gets the world position for any point on the camera
    /// </summary>
    public static Vector3 GetWorldPositionForScreenPoint(Camera cam, Vector3 screenPosition)
    {
        screenPosition.z = zDistance;
        var worldPos = cam.ScreenToWorldPoint(screenPosition);

        return worldPos;
    }

    /// <summary>
    /// Gets the world position of the mouse cursor projected onto a plane at z = zDistance = 5f
    /// </summary>
    public static Vector3 GetMouseWorldPosition(Camera cam)
    {
        return GetWorldPositionForScreenPoint(cam, Input.mousePosition);
    }

    /// <summary>
    /// Gets the mouse delta movement in world space between last frame and this frame.
    /// </summary>
    public static Vector3 GetMouseWorldDelta(Camera cam)
    {
        Vector3 previous = GetWorldPositionForScreenPoint(cam, Input.mousePosition);
        Vector3 current = GetMouseWorldPosition(cam);
        return current - previous;
    }

    /// <summary>
    /// Gets the world space top-right bounds of the camera's viewport at a specified z-distance from the camera.
    /// </summary>
    /// <param name="cam">Camera to use for conversion</param>
    /// <returns>World space top-right bounds as Vector2 (x,y)</returns>
    public static Vector2 GetScreenBounds(Camera cam)
    {
        if (cam == null)
        {
            Debug.LogError("Camera is null in GetScreenBounds!");
            return Vector2.zero;
        }

        Vector3 topRightScreenPoint = new Vector3(Screen.width, Screen.height, zDistance);
        Vector3 worldTopRight = cam.ScreenToWorldPoint(topRightScreenPoint);

        return new Vector2(worldTopRight.x, worldTopRight.y);
    }
}
