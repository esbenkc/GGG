using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    // Prefab for the wire nodes
    [SerializeField] private GameObject wireNodePrefab;
    [SerializeField] private Transform player;

    // Maximum distance between nodes
    [SerializeField] private float maxDistance = 0.5f;

    // List of nodes
    private List<GameObject> nodes = new List<GameObject>();

    // Linerenderer
    [SerializeField]
    private LineRenderer lineRenderer;


    // Start is called before the first frame update
    void Start()
    {
        // Disable the line renderer at the start
        lineRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // If space is down, add nodes per distance between the latest node
        // And add the node to the LineRenderer points
        if (Input.GetKey(KeyCode.Space)) {
            if (nodes.Count == 0) {
                lineRenderer.enabled = true;
                nodes.Add(Instantiate(wireNodePrefab, player.position, Quaternion.identity));
            } else {
                if (Vector2.Distance(nodes[nodes.Count - 1].transform.position, transform.position) > maxDistance) {
                    nodes.Add(Instantiate(wireNodePrefab, player.position, Quaternion.identity));
                    lineRenderer.positionCount = nodes.Count;
                    lineRenderer.SetPosition(nodes.Count - 1, nodes[nodes.Count - 1].transform.position);
                }
            }
        }
    }
}
