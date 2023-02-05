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
    private Vector2 lastHit;
    private bool hasLastHit = false;

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
        RootTrail trail = line.GetComponent<RootTrail>();
        trail.StartReverting();

        yield return new WaitForSeconds(2);
        

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
    bool hasBeenShot = false;

    public void NewLine()
    {
        hasBeenShot = true;
        updating = true;
        // Create line object
        GameObject lineobj = Instantiate(lineRenderer.gameObject);
        
        LineRenderer newline = lineobj.GetComponent<LineRenderer>();

      
        newline.positionCount += 1;
        newline.SetPosition(0, hasLastHit ? lastHit : player.position);
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
        lastHit = coll.contacts[0].point;
        hasLastHit = true;

        if (currentLine != 0)
        {

            LineRenderer oldLine = lineRenderers[lineRenderers.Count - 1];
            oldLine.SetPosition(oldLine.positionCount - 1, lastHit);
            if (hitPoints.Count == lineRenderers.Count)
            {
                nodes.Add(player.transform.position);
                hitPoints[hitPoints.Count - 1] = nodes.Count - 1;
            }
            else
            {
                oldLine.GetComponent<RootTrail>().StartAnimating();
            }
        }
        if (hitPoints.Count < lineRenderers.Count)
        {
            hitPoints.Add(nodes.Count - 1);

        }
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
