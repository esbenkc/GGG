using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootTrail : MonoBehaviour
{
    [SerializeField]
    LineRenderer lineRenderer;

    [SerializeField]
    float relaxDuration = 2, stepSize = 0.05f;

    [SerializeField]
    float growthSpeed = 2f;

    bool animating = false;
    float startTime;

    MaterialPropertyBlock block;

    public void StartAnimating() {
        if (lineRenderer == null) return;
        if (block == null) block = new MaterialPropertyBlock();

        lineRenderer.enabled = true;
        LineRelaxer.Relax(lineRenderer, relaxDuration, stepSize);
        animating = true;
        startTime = Time.time;
    }

    private void Update() {
        if (!animating) return;

        lineRenderer.GetPropertyBlock(block);
        float elapsedTime = Time.time - startTime;
        block.SetFloat("_VisibleLength", elapsedTime * growthSpeed);
        lineRenderer.SetPropertyBlock(block);

        if (elapsedTime > 20) animating = false;
    }
}
