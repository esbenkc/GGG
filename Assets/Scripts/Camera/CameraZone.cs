using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraZone : MonoBehaviour {
    public Vector2 Size => size;
    public float Margin => margin;

    public Rect Bounds {
        get {
            var totalSize = size + 2 * margin * Vector2.one;
            return new Rect((Vector2)transform.position - totalSize / 2, totalSize);
        }
    }

    [SerializeField]
    Vector2 size = Vector2.one;

    [SerializeField]
    float margin = 1;

    public void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, size);
        var bounds = Bounds;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
