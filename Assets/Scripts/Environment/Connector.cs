using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector : MonoBehaviour
{
    [SerializeField] private Transform on;
    [SerializeField] private Transform off;

    [SerializeField] Color flipGreen = Color.green, flipRed = Color.red, intermediateColor = Color.yellow;

    [SerializeField]
    private float flipTime = 1.0f, delayBeforeInteractable = 2.0f;

    private float flipTimer = 0.0f;

    private LineRenderer lineRenderer;

    private BoxCollider2D connectCol1, connectCol2;
    private SpriteRenderer connectSprite1, connectSprite2;

    private bool initStatus = true, lastStatus = true;
    
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.SetPosition(0, on.position);
        lineRenderer.SetPosition(1, off.position);

        connectCol1 = on.GetComponent<BoxCollider2D>();
        connectCol2 = off.GetComponent<BoxCollider2D>();
        connectSprite1 = on.GetComponent<SpriteRenderer>();
        connectSprite2 = off.GetComponent<SpriteRenderer>();
        
        connectCol1.isTrigger = true;
        connectCol2.isTrigger = false;
        connectSprite1.color = flipGreen;
        connectSprite2.color = flipRed;

        // Set the init status of Connector1
        initStatus = connectCol1.isTrigger;
        lastStatus = initStatus;
    }

    public void Reset(bool full = false) {
        // Reset the connectors
        if(full) {
            connectCol1.isTrigger = initStatus;
            connectCol2.isTrigger = !initStatus;
            connectSprite1.color = initStatus ? flipGreen : flipRed;
            connectSprite2.color = initStatus ? flipRed : flipGreen;
        }
        else {
            connectCol1.isTrigger = lastStatus;
            connectCol2.isTrigger = !lastStatus;
            connectSprite1.color = lastStatus ? flipGreen : flipRed;
            connectSprite2.color = lastStatus ? flipRed : flipGreen;
        }

        StopCoroutine(FlipConnectors());
    }

    public void Flip() {
        if(flipTimer == 0.0f) StartCoroutine(FlipConnectors());
    }

    // Create a coroutine that flips the connectors
    IEnumerator FlipConnectors() {        

        // While the timer is less than the flip time
        while (flipTimer < flipTime) {
            // Increment the timer
            flipTimer += Time.deltaTime;

            // Lerp the color of the connectors from their color to the intermediate color
            if(connectCol1.isTrigger)
                connectSprite1.color = Color.Lerp(flipGreen, intermediateColor, flipTimer / flipTime);
            else
                connectSprite2.color = Color.Lerp(flipRed, intermediateColor, flipTimer / flipTime);

            // Wait for the next frame
            yield return null;
        }

        // Swap the triggers
        bool temp = connectCol1.isTrigger;
        connectCol1.isTrigger = !temp;
        connectCol2.isTrigger = temp;

        // Flip the colors
        connectSprite1.color = connectCol1.isTrigger ? flipGreen : flipRed;
        connectSprite2.color = connectCol2.isTrigger ? flipGreen : flipRed;

        lastStatus = connectCol1.isTrigger;

        yield return new WaitForSeconds(delayBeforeInteractable);

        // Set the timer to 0
        flipTimer = 0.0f;
    }
}
