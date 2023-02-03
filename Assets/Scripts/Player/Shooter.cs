using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField]
    private bool movable = false;

    // Drag the player character to send it flying
    [SerializeField] private Transform player;

    // reference linerenderer
    [SerializeField] private LineRenderer lineRenderer;

    // Rigidbody2d
    private Rigidbody2D playerRigidbody;

    [SerializeField] private float power = 100;

    private void Start() {
        // Disable the line renderer at the start
        lineRenderer.enabled = false;
        playerRigidbody = player.GetComponent<Rigidbody2D>();
        // Freeze the player character
        if(movable == false)
        {
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void Update() {
        // Draw a line from the player to the mouse position when left mouse is pressed
        if (Input.GetMouseButtonDown(0) && movable) {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, player.position);
            lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        // And keep it updated as the mouse is kept down
        if (Input.GetMouseButton(0) && movable) {
            lineRenderer.SetPosition(0, player.position);
            lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        // When mouse is released send the player flying
        if (Input.GetMouseButtonUp(0) && movable) {
            lineRenderer.enabled = false;
            SetMovable(false);
            playerRigidbody.constraints = RigidbodyConstraints2D.None;
            playerRigidbody.AddForce((player.position - Camera.main.ScreenToWorldPoint(Input.mousePosition)) * power);
        }
    }

    public void SetMovable (bool movable)
    {
        this.movable = movable;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        // If the player hits the ground, unfreeze it
        if (collision.gameObject.tag == "Earth") {
            SetMovable(true);
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }
}
