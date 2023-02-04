using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class GrassComponent : MonoBehaviour
{
    [SerializeField]
    float grassStrength = 0;

    [SerializeField]
    AnimationCurve onHitCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 0.5f) });

    [SerializeField]
    float animationTime = 2;

    [SerializeField]
    Vector2 grassPoint = Vector2.zero;

    [SerializeField]
    Vector2 grassNormal = Vector2.up;

    [SerializeField]
    SpriteShapeRenderer spriteRenderer = null;

    MaterialPropertyBlock block;

    bool animating = false;
    float startTime;

    public void StartAnimating() {
        animating = true;
        startTime = Time.time;
    }
    public void StartAnimating(Vector2 worldPosition, Vector2 worldNormal) {
        grassPoint = transform.worldToLocalMatrix * new Vector4(worldPosition.x, worldPosition.y, 0, 1);
        grassNormal = transform.worldToLocalMatrix * new Vector4(worldNormal.x, worldNormal.y, 0, 0);
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
        if (animating) {
            float t = (Time.time - startTime) / animationTime;

            grassStrength = onHitCurve.Evaluate(t);
            UpdateGrass();

            if (t >= 1) animating = false;
        }
    }

    private void UpdateGrass() {
        if (spriteRenderer == null) return;
        if (block == null) block = new MaterialPropertyBlock();

        spriteRenderer.GetPropertyBlock(block);
        block.SetVector("_GrassPoint", grassPoint);
        block.SetFloat("_GrassStrength", grassStrength);
        block.SetVector("_GrassNormal", grassNormal);

        spriteRenderer.SetPropertyBlock(block);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawRay(new Ray(transform.localToWorldMatrix * new Vector4(grassPoint.x, grassPoint.y, 0, 1), grassNormal));
    }
}
