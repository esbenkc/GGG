using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField]
    CameraZone activeZone;

    [SerializeField]
    Camera cam;

    [SerializeField]
    Transform target;

    // Update is called once per frame
    private void LateUpdate() {
        if (activeZone == null) return;
        if (cam == null) return;
        if (target == null) return;

        Vector2 wantedPosition = target.position;
        var bounds = CalculateBounds(wantedPosition);
        var zoneBounds = activeZone.Bounds;

        var position = wantedPosition;

        if (bounds.xMin < zoneBounds.xMin)
            position.x += zoneBounds.xMin - bounds.xMin;

        if (bounds.xMax > zoneBounds.xMax)
            position.x -= bounds.xMax - zoneBounds.xMax;

        if (bounds.yMin < zoneBounds.yMin)
            position.y += zoneBounds.yMin - bounds.yMin;

        if (bounds.yMax > zoneBounds.yMax)
            position.y -= bounds.yMax - zoneBounds.yMax;

        transform.position = new Vector3(position.x, position.y, -10);
    }

    private Rect CalculateBounds(Vector2 center) {
        Vector2 size = new Vector2(cam.aspect * cam.orthographicSize, cam.orthographicSize) * 2;
        return new Rect(center - size / 2, size);
    }
}
