using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour {
    [SerializeField]
    CameraZone activeZone;

    [SerializeField]
    CinemachineVirtualCamera cam;

    [SerializeField]
    Transform target;

    // Update is called once per frame
    private void LateUpdate() {
        if (cam == null) return;
        if (target == null) return;

        Vector2 wantedPosition = target.position;
        var bounds = CalculateBounds(wantedPosition);

        var position = wantedPosition;

        if (activeZone != null) {
            var zoneBounds = activeZone.Bounds;
            if (bounds.xMin < zoneBounds.xMin)
                position.x += zoneBounds.xMin - bounds.xMin;

            if (bounds.xMax > zoneBounds.xMax)
                position.x -= bounds.xMax - zoneBounds.xMax;

            if (bounds.yMin < zoneBounds.yMin)
                position.y += zoneBounds.yMin - bounds.yMin;

            if (bounds.yMax > zoneBounds.yMax)
                position.y -= bounds.yMax - zoneBounds.yMax;
        }

        transform.position = new Vector3(position.x, position.y, -10);
    }

    private Rect CalculateBounds(Vector2 center) {
        Vector2 size = new Vector2(cam.m_Lens.Aspect * cam.m_Lens.OrthographicSize, cam.m_Lens.OrthographicSize) * 2;
        return new Rect(center - size / 2, size);
    }
}
