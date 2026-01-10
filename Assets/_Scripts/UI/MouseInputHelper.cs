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
    /// Gets the world position of the mouse cursor projected onto a plane at z = zDistance
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

    /// <summary>
    /// Converts a screen-space position (e.g. mouse) into the local space of a Screen Space - Camera canvas.
    /// </summary>
    public static Vector3 GetCanvasLocalPosition(Vector3 screenPos, Canvas canvas)
    {
        if (canvas == null)
        {
            Debug.LogWarning("MouseInputHelper: Canvas reference is null!");
            return Vector3.zero;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.worldCamera,
            out Vector2 localPoint))
        {
            return localPoint;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Clamps a local canvas position so it stays fully within canvas bounds.
    /// </summary>
    public static Vector3 ClampToCanvasBounds(Vector3 localPos, RectTransform canvasRect, Vector2 elementSize, float scale = 1f)
    {
        Vector2 scaledHalfSize = 0.5f * scale * elementSize;

        float xMin = -canvasRect.rect.width * 0.5f + scaledHalfSize.x;
        float xMax =  canvasRect.rect.width * 0.5f - scaledHalfSize.x;
        float yMin = -canvasRect.rect.height * 0.5f + scaledHalfSize.y;
        float yMax =  canvasRect.rect.height * 0.5f - scaledHalfSize.y;

        localPos.x = Mathf.Clamp(localPos.x, xMin, xMax);
        localPos.y = Mathf.Clamp(localPos.y, yMin, yMax);

        return localPos;
    }
}
