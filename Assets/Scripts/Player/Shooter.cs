using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GameObjectEvent : UnityEvent<GameObject>
{
}

public class Shooter : MonoBehaviour
{
    [SerializeField]
    private bool movable = true;

    [SerializeField]
    private float minSpeed = 0.01f, minDelay = 7.5f, whenStillDelay = 0.5f;

    float delay = 0f, stillDelay = 0f;

    // Drag the player character to send it flying
    [SerializeField] private Transform player;

    // reference linerenderer
    [SerializeField] private LineRenderer lineRenderer;

    // Rigidbody2d
    private Rigidbody2D playerRigidbody;

    [SerializeField] private float power = 100f, torquePower = 20f;

    // Make spanwPosition vector2
    private Vector2 spawnPosition;

    public UnityEvent onPlayerHitGoal, onPlayerHitGround;
    public GameObjectEvent onPlayerHitKey;

    private void Start() {
        // Disable the line renderer at the start
        lineRenderer.enabled = false;
        playerRigidbody = player.GetComponent<Rigidbody2D>();
        // Freeze the player character
        playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        spawnPosition = player.position;
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
            Debug.Log("Mouse released");
            lineRenderer.enabled = false;
            movable = false;
            delay = 0f;
            playerRigidbody.constraints = RigidbodyConstraints2D.None;
            playerRigidbody.AddForce((player.position - Camera.main.ScreenToWorldPoint(Input.mousePosition)) * power);
            // Add randmo angular momentum
            playerRigidbody.AddTorque(Random.Range(-torquePower, torquePower));
        }

        if(!movable) {
            delay += Time.deltaTime;
            if(playerRigidbody.velocity.magnitude < minSpeed) {
                stillDelay += Time.deltaTime;
            } else {
                stillDelay = 0f;
            }
            if (delay > minDelay && playerRigidbody.velocity.magnitude < minSpeed && stillDelay > whenStillDelay) {
                playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
                player.position = spawnPosition;
                movable = true;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        // If the player hits the ground, unfreeze it
        // ONly when it hits "Earth", layer 6
        if (collision.gameObject.layer ==  6) {
            movable = true;
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            spawnPosition = player.position;
            onPlayerHitGround.Invoke();
        }

        if (collision.gameObject.layer ==  7) {
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            onPlayerHitGoal.Invoke();
        }

        if (collision.gameObject.layer ==  8) {
            onPlayerHitKey.Invoke(collision.gameObject);
        }
    }
}
