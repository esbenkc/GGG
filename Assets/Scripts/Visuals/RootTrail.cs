using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RootTrail : MonoBehaviour {
    public enum AnimationState {
        Growing,
        Reverting,
        NotAnimating
    }

    [SerializeField]
    LineRenderer lineRenderer;

    [SerializeField]
    float relaxDuration = 2, stepSize = 0.05f;

    [SerializeField]
    float growthSpeed = 2f, revertSpeed = 5f;

    AnimationState state = AnimationState.NotAnimating;
    float startTime;
    float totalLength;

    MaterialPropertyBlock block;

    void UpdateTotalLength() {
        Vector3[] positions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(positions);

        totalLength = 0;

        for (int i = 0; i < positions.Length - 1; i++) {
            totalLength += (positions[i] - positions[i + 1]).magnitude;
        }

        totalLength = Mathf.Ceil(totalLength);
    }

    public void StartAnimating() {
        if (lineRenderer == null) return;
        if (block == null) block = new MaterialPropertyBlock();

        lineRenderer.enabled = true;
        LineRelaxer.Relax(lineRenderer, relaxDuration, stepSize, LayerMask.NameToLayer("Trail Simulation"));

        UpdateTotalLength();

        state = AnimationState.Growing;
        startTime = Time.time;
    }

    public void StartReverting() {
        if (lineRenderer == null) return;
        if (block == null) block = new MaterialPropertyBlock();

        lineRenderer.enabled = true;

        UpdateTotalLength();

        state = AnimationState.Reverting;
        startTime = Time.time;
    }

    private void Update() {
        if (state == AnimationState.NotAnimating) return;

        lineRenderer.GetPropertyBlock(block);
        float elapsedTime = Time.time - startTime;

        float currentProgress =
                state == AnimationState.Growing ?
                    elapsedTime * growthSpeed
                    : totalLength - elapsedTime * revertSpeed;

        block.SetFloat("_VisibleLength", currentProgress);
        lineRenderer.SetPropertyBlock(block);

        if (currentProgress > totalLength || currentProgress < 0) state = AnimationState.NotAnimating;
    }

    public float GetGrowTime() {
        UpdateTotalLength();
        return totalLength / growthSpeed;
    }

    public float GetRevertTime() {
        UpdateTotalLength();
        return totalLength / revertSpeed;
    }
}
