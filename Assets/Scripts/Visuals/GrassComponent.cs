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
    Vector2 grassPoint = Vector2.zero;

    [SerializeField]
    Vector2 grassNormal = Vector2.up;

    [SerializeField]
    SpriteShapeRenderer spriteRenderer = null;

    MaterialPropertyBlock block;
    // Start is called before the first frame update

    private void OnValidate() {
        UpdateGrass();
    }

    void Start()
    {
        UpdateGrass();
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
