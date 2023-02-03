using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    // Drag the player character to send it flying
    [SerializeField] private Transform player;

    // reference linerenderer
    [SerializeField] private LineRenderer lineRenderer;

// Rigidbody2d
    private Rigidbody2D playerRigidbody;

    private void Start() {
        // Disable the line renderer at the start
        lineRenderer.enabled = false;
        playerRigidbody = player.GetComponent<Rigidbody2D>();
    }

    private void Update() {
        // Draw a line from the player to the mouse position when left mouse is pressed
        if (Input.GetMouseButtonDown(0)) {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, player.position);
            lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        // And keep it updated as the mouse is kept down
        if (Input.GetMouseButton(0)) {
            lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        // When mouse is released send the player flying
        if (Input.GetMouseButtonUp(0)) {
            lineRenderer.enabled = false;
            playerRigidbody.AddForce((player.position - Camera.main.ScreenToWorldPoint(Input.mousePosition)) * 1000);
        }
    }
}
