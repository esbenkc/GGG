using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector : MonoBehaviour
{
    [SerializeField] private Transform connector1;
    [SerializeField] private Transform connector2;

    private LineRenderer lineRenderer;
    
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.SetPosition(0, connector1.position);
        lineRenderer.SetPosition(1, connector2.position);
        
    }
}
