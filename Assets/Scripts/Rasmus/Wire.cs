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

    [SerializeField]
    private List<int> hitPoints = new List<int>();
    // List of nodes
    [SerializeField]
    private List<Vector2> nodes = new List<Vector2>();
    [SerializeField]
    private List<LineRenderer> lineRenderers;
    // Linerenderer
    [SerializeField]
    private LineRenderer lineRenderer;

    // update line renderer when moving
    private bool updating = false;
    private int currentLine = 0;
    public void RemoveRoot()
    {
        var line = lineRenderers[currentLine];
        lineRenderers.RemoveAt(currentLine--);
        Destroy(line.gameObject);
        int end = nodes.Count - 1;
        int beginning = hitPoints[hitPoints.Count -2];
        hitPoints.RemoveAt(hitPoints.Count - 1);
        nodes.RemoveRange(beginning, end - beginning);
    }

    

    public void NewLine()
    {
        // Create line object
        GameObject lineobj = Instantiate(lineRenderer.gameObject);
        
        LineRenderer newline = lineobj.GetComponent<LineRenderer>();

      
        newline.positionCount += 1;
        newline.SetPosition(0, player.position);
        newline.enabled = true;
        lineRenderers.Add(newline);
        currentLine++;

        
    }
    // Start is called before the first frame update
    void Start()
    {
        // Disable the line renderer at the start
        lineRenderer.enabled = false;
        lineRenderers.Add(Instantiate(lineRenderer, player.position, Quaternion.identity).GetComponent<LineRenderer>());
        lineRenderers[currentLine].enabled = true;
        updating = true;
        AddNode();
        hitPoints.Add(0);
        transform.position = player.transform.position;
    }
    

    void AddNode()
    {
        nodes.Add(new Vector2(player.position.x, player.position.y));
    }

    public void AddHitPoint()
    {
        AddNode();
        if (currentLine != 0)
        {
            LineRenderer oldLine = lineRenderers[lineRenderers.Count - 1];
            oldLine.SetPosition(oldLine.positionCount - 1, player.position);
        }
        hitPoints.Add(nodes.Count - 1);
        //NewLine();
    }
    

    public void UpdateLine()
    {
        if (!updating)
            return;
        if (Vector2.Distance(nodes[nodes.Count-1], player.position) > maxDistance)
        {
            AddNode();
            lineRenderers[currentLine].positionCount++;
            lineRenderers[currentLine].SetPosition(lineRenderers[currentLine].positionCount - 1, nodes[nodes.Count -1]);
        }
    }

    public void StopUpdating()
    {
        updating = false;
    }
    // Update is called once per frame
    void Update()
    {
        // If space is down, add nodes per distance between the latest node
        // And add the node to the LineRenderer points
        if (Input.GetKey(KeyCode.Space)) {
            updating = true;
        }

        UpdateLine();

        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveRoot();
            player.transform.position = nodes[hitPoints[hitPoints.Count - 1]];

        }
    }
}
