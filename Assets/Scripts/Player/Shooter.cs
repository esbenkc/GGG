using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GameObjectEvent : UnityEvent<GameObject>
{
}

[System.Serializable]
public class CollisionEvent : UnityEvent<Collision2D>
{
}

[System.Serializable]
public class TriggerEvent : UnityEvent<Collider2D>
{
}

public class Shooter : MonoBehaviour
{
    [SerializeField]
    private bool movable = true;

    [SerializeField]
    private float minSpeed = 0.01f, minDelay = 7.5f, whenStillDelay = 0.5f, particleSystemLife = 5f, explosionPower = 2f, baseExplosionPower = 2f;

    float delay = 0f, stillDelay = 0f;

    // Drag the player character to send it flying
    [SerializeField] private Transform player;

    // Particle system preab when hitting ground
    [SerializeField] private GameObject groundHitParticles;

    // reference linerenderer
    [SerializeField] private LineRenderer lineRenderer;

    // Rigidbody2d
    private Rigidbody2D playerRigidbody;

    [SerializeField] private float power = 100f, torquePower = 20f;

    // Make spanwPosition vector2
    private Vector2 spawnPosition, startPosition;
    private Reset reset;

    public CollisionEvent onPlayerHitGround;
    public TriggerEvent onPlayerHitGoal;
    public GameObjectEvent onPlayerHitKey;
    private GameObject curParticleSystem;
    private float particleSystemTime = 0f;

    // Create a title in the inspector
    [Header("Audio settings")]
    [SerializeField] AudioSource playerAudioSource;
    [SerializeField] Audio playerJumpSound, playerHitSound, playerBounceSound, playerResetSound;
    [SerializeField] float playerVelocitySoundMultiplier = 0.5f;

    [Header("Goal moving settings")]
    [SerializeField] AnimationCurve speedCurve;
    [SerializeField] float stepMoveTime = 1f;

    private Vector3[] goalPositions;

    private void Start() {
        // Set the start position
        startPosition = player.position;
        // Disable the line renderer at the start
        lineRenderer.enabled = false;
        playerRigidbody = player.GetComponent<Rigidbody2D>();
        // Freeze the player character
        playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        spawnPosition = player.position;
        
        // Find the reset script in the scene
        reset = FindObjectOfType<Reset>();
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
            movable = false;
            delay = 0f;
            playerRigidbody.constraints = RigidbodyConstraints2D.None;
            playerRigidbody.AddForce((player.position - Camera.main.ScreenToWorldPoint(Input.mousePosition)) * power);
            // Add randmo angular momentum
            playerRigidbody.AddTorque(Random.Range(-torquePower, torquePower));

            // Play sound
            playerAudioSource.PlayOneShot(playerJumpSound.clip, playerJumpSound.volume);
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            player.position = startPosition;
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            movable = true;
            reset.ResetAll(true);
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

                // Play sound
                playerAudioSource.PlayOneShot(playerResetSound.clip, playerResetSound.volume);

                reset.ResetAll();
            }
        }

        if(curParticleSystem != null) {
            particleSystemTime += Time.deltaTime;
            if(particleSystemTime > particleSystemLife) {
                Destroy(curParticleSystem);
                curParticleSystem = null;
                particleSystemTime = 0f;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        // If the player hits the ground, unfreeze it
        // ONly when it hits "Earth", layer 6
        if (collision.gameObject.layer ==  6) {
            movable = true;
            curParticleSystem = Instantiate(groundHitParticles, player.position, Quaternion.identity);
            // Set the particle system speed and amount based on velocity of player
            var main = curParticleSystem.GetComponent<ParticleSystem>().main;
            main.startSpeed = baseExplosionPower + playerRigidbody.velocity.magnitude * explosionPower;
            // Update the amount of particles as well
            var emission = curParticleSystem.GetComponent<ParticleSystem>().emission;
            emission.rateOverTime = baseExplosionPower + playerRigidbody.velocity.magnitude * explosionPower + 40f;
            
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            spawnPosition = player.position;
            onPlayerHitGround.Invoke(collision);

            // Play sound
            playerAudioSource.PlayOneShot(playerHitSound.clip, playerHitSound.volume + playerRigidbody.velocity.magnitude * playerVelocitySoundMultiplier > 1f ? 1f : playerHitSound.volume + playerRigidbody.velocity.magnitude * playerVelocitySoundMultiplier);
        } else {
            // Play sound
            playerAudioSource.PlayOneShot(playerBounceSound.clip, playerBounceSound.volume + playerRigidbody.velocity.magnitude * playerVelocitySoundMultiplier > 1f ? 1f : playerBounceSound.volume + playerRigidbody.velocity.magnitude * playerVelocitySoundMultiplier);
        }

        if (collision.gameObject.layer ==  8) {
            onPlayerHitKey.Invoke(collision.gameObject);
        }
    }

    // When hitting trigger layer 7 (goal) follow the Goal's linerenderer path and move the player along it
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer ==  7) {
            movable = false;
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            // Get all positions in the goal's linerenderer
            LineRenderer otherLR = collision.GetComponent<LineRenderer>();
            // Make goalPositions the same length as the goal's linerenderer
            Vector3[] goalPositions = new Vector3[otherLR.positionCount];
            otherLR.GetPositions(goalPositions);
            StartCoroutine(MoveAlongPath(goalPositions));
            onPlayerHitGoal.Invoke(collision);
            // Play sound
            playerAudioSource.PlayOneShot(playerHitSound.clip, playerHitSound.volume + playerRigidbody.velocity.magnitude * playerVelocitySoundMultiplier > 1f ? 1f : playerHitSound.volume + playerRigidbody.velocity.magnitude * playerVelocitySoundMultiplier);
        }
    }

    // Coroutine to move the player along the path of the goal
    private IEnumerator MoveAlongPath(Vector3[] stepPositions) {
        float timer = 0f;
        int currentStep = 0;
        movable = false;
        playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        gameObject.GetComponent<Collider2D>().enabled = false;
        
        // Move the player along the path
        while (timer < stepMoveTime)  {
            timer += Time.deltaTime;
            if (currentStep == stepPositions.Length - 1) {
                player.position = stepPositions[stepPositions.Length - 1];
                break;
            }
            player.position = Vector3.Lerp(stepPositions[currentStep], stepPositions[currentStep + 1], timer / stepMoveTime);
            if(timer > stepMoveTime) {
                currentStep++;
                timer = 0f;
            }
            yield return null;
        }

        // Set the player to the last position
        player.position = stepPositions[stepPositions.Length - 1];
        gameObject.GetComponent<Collider2D>().enabled = true;
        playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        startPosition = player.position;
        spawnPosition = player.position;
        movable = true;
    }
}

// Audio struct with audio clip and volume
[System.Serializable]
public struct Audio
{
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume;
}