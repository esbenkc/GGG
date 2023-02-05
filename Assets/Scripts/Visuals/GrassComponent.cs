using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class GrassComponent : MonoBehaviour
{
    [Serializable]
    public struct GrassState {
        public float strength;
        public Vector2 point;
        public Vector2 normal;
        public bool animating;
        public float startTime;
    }

    [SerializeField]
    GrassState[] states = new GrassState[4];

    [SerializeField]
    int nextToOverride = 0;

    [SerializeField]
    AnimationCurve onHitCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 0.5f) });

    [SerializeField]
    float animationTime = 2;

    [SerializeField]
    SpriteShapeRenderer spriteRenderer = null;

    MaterialPropertyBlock block;

    public void StartAnimating() {
        states[nextToOverride].animating = true;
        states[nextToOverride].startTime = Time.time;
        nextToOverride = (nextToOverride + 1) % states.Length;
    }
    public void StartAnimating(Vector2 worldPosition, Vector2 worldNormal) {
        states[nextToOverride].point = transform.worldToLocalMatrix * new Vector4(worldPosition.x, worldPosition.y, 0, 1);
        states[nextToOverride].normal = transform.worldToLocalMatrix * new Vector4(worldNormal.x, worldNormal.y, 0, 0);
        StartAnimating();
    }

    private void OnValidate() {
        UpdateGrass();
    }

    void Start()
    {
        UpdateGrass();
    }

    private void Update() {
        bool update = false;

        for (int i = 0; i < states.Length; i++) {
            if (states[i].animating) {
                float t = (Time.time - states[i].startTime) / animationTime;

                states[i].strength = onHitCurve.Evaluate(t);

                update = true;

                if (t >= 1) states[i].animating = false;
            }
        }
        if (update) UpdateGrass();
    }

    private void UpdateGrass() {
        if (spriteRenderer == null) return;
        if (block == null) block = new MaterialPropertyBlock();

        spriteRenderer.GetPropertyBlock(block);

        for (int i = 0; i < states.Length; i++) {
            var state = states[i];
            var iplus1 = i + 1;
            block.SetVector($"_GrassPoint_{iplus1}", state.point);
            block.SetFloat($"_GrassStrength_{iplus1}", state.strength);
            block.SetVector($"_GrassNormal_{iplus1}", state.normal);
        }

        spriteRenderer.SetPropertyBlock(block);
    }

    private void OnDrawGizmosSelected() {
        for (int i = 0; i < states.Length; i++) {
            var state = states[i];
            if (state.strength == 0) continue;
            var point = state.point;
            Gizmos.DrawRay(new Ray(transform.localToWorldMatrix * new Vector4(point.x, point.y, 0, 1), state.normal));
        }
    }
}
