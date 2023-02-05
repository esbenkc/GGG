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
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    // Linerenderer
    [SerializeField]
    private LineRenderer lineRenderer;

    // update line renderer when moving
    private bool updating = false;
    private int currentLine = -1;
    public void RemoveRoot()
    {
        var line = lineRenderers[currentLine];
        lineRenderers.RemoveAt(currentLine--);
        Destroy(line.gameObject);
        //int end = nodes.Count - 1;
        //int beginning = hitPoints[hitPoints.Count - 1];
        //hitPoints.RemoveAt(hitPoints.Count - 1);
        //nodes.RemoveRange(beginning, end - beginning);
    }

    

    IEnumerator DestroyLine()
    {
        float startTime = Time.timeSinceLevelLoad;
        LineRenderer line = lineRenderers[currentLine];
        while (startTime + 2 < Time.timeSinceLevelLoad)
        {
            yield return null;
        }

        RemoveRoot();
        if(currentLine >= 0)
        {

            StartCoroutine("DestroyLine");
        }
        else
        {
            hitPoints.Clear();
        }
    }
    

    public void NewLine()
    {
        updating = true;
        // Create line object
        GameObject lineobj = Instantiate(lineRenderer.gameObject);
        
        LineRenderer newline = lineobj.GetComponent<LineRenderer>();

      
        newline.positionCount += 1;
        newline.SetPosition(0, player.position);
        //newline.enabled = true;
        lineRenderers.Add(newline);
        currentLine++;
        

        
    }
    // Start is called before the first frame update
    void Start()
    {
        // Disable the line renderer at the start
        lineRenderer.enabled = false;
        AddNode();
        hitPoints.Add(0);
        transform.position = player.transform.position;
    }
    
    
    void AddNode()
    {
        nodes.Add(new Vector2(player.position.x, player.position.y));
    }
    void AddNode(Vector2 point)
    {
        nodes.Add(point);
    }
    public void AddHitPoint(Collision2D coll)
    {
        AddNode(coll.contacts[0].point);
        if (currentLine != 0)
        {
            LineRenderer oldLine = lineRenderers[lineRenderers.Count - 1];
            oldLine.SetPosition(oldLine.positionCount - 1, player.position);
            oldLine.GetComponent<RootTrail>().StartAnimating();
        }
        lineRenderers[currentLine].GetComponent<RootTrail>().StartAnimating();
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
            StartCoroutine("DestroyLine");

        }
    }
}
