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
    [Header("Player, respawn and particle system settings")]
    [SerializeField] float minSpeed = 0.01f;

    [SerializeField]
    private float minDelay = 1f, whenStillDelay = 0.5f, explosionPower = 2f, baseExplosionPower = 2f;

    float delay = 0f, stillDelay = 0f;
    private Transform player;
    private bool movable = true;

    // Particle system preab when hitting ground
    [SerializeField] private GameObject groundHitParticles;

    [Header("Drag line settings")]
    // reference linerenderer
    [SerializeField] private LineRenderer lineRenderer;

    // Rigidbody2d
    private Rigidbody2D playerRigidbody;

    [SerializeField] private float power = 100f, torquePower = 20f;

    // Make spanwPosition vector2
    private Vector2 spawnPosition;

    public UnityEvent onPlayerShoot;
    public CollisionEvent onPlayerHitGround;
    public TriggerEvent onPlayerHitGoal;
    public UnityEvent onPlayerReset;

    public GameObjectEvent onPlayerHitKey;
    private GameObject curParticleSystem;
    private float particleSystemTime = 0f;

    // Create a title in the inspector
    [Header("Audio settings")]
    [SerializeField] AudioSource playerAudioSource;
    [SerializeField] Audio playerJumpSound, playerHitSound, playerBounceSound, playerResetSound, tunnelDig;
    [SerializeField] float playerVelocitySoundMultiplier = 0.5f;

    [Header("Goal moving settings")]
    [SerializeField] AnimationCurve speedCurve;
    [SerializeField] float stepMoveTime = 1f;


    private Vector3[] goalPositions;
    
    [Header("Earth traversal")]
    [SerializeField] GameObject tunnel;
    bool inTunnel = false;
    bool nextLandingIsSpawn = false;


    private void Start() {
        player = transform;
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
        }

        // And keep it updated as the mouse is kept down
        if (Input.GetMouseButton(0) && movable) {
            lineRenderer.SetPosition(0, player.position);
            lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        // When mouse is released send the player flying
        if (Input.GetMouseButtonUp(0) && movable) {

            // Send raycast and check it if it goes through earth
            Vector2 direction = ((Vector2)(player.position - Camera.main.ScreenToWorldPoint(Input.mousePosition))).normalized;
            RaycastHit2D hit = Physics2D.Raycast(player.position, direction, 0.5f, LayerMask.GetMask("Earth"));
            if (!hit) {
                Jump();
            } else {
                // Instantiate tunnel and reference its line renderer to draw a line from the player's entrance to the earth to the exit

                Vector2 reverseDirection = -direction;
                RaycastHit2D[] reverseHits = Physics2D.RaycastAll(new Vector2(player.position.x, player.position.y) + direction * 20f, reverseDirection, 20f, LayerMask.GetMask("Earth"));
                Vector2 exitPoint = reverseHits[reverseHits.Length - 1].point;
                float distanceToExitPoint = Vector2.Distance(player.position, exitPoint);
                RaycastHit2D[] blockedExit = Physics2D.RaycastAll(player.position, direction, distanceToExitPoint + 0.3f);
                bool blocked = false;

                // Create a Unity gizmo for the blockedExit
                //Debug.DrawLine(player.position, new Vector2(player.position.x, player.position.y) + direction * (distanceToExitPoint + 0.3f), Color.red, 1f);
                //Debug.DrawLine(player.position, exitPoint, Color.green, 1f);

                foreach(RaycastHit2D hit2 in blockedExit) {
                    if (hit2.collider.gameObject.layer != LayerMask.NameToLayer("Earth") && hit2.collider.gameObject.layer != LayerMask.NameToLayer("Player")) {
                        blocked = true;
                    }
                }
                
                if (blocked) {
                    Jump();
                } else {
                    GameObject tunnelInstance = Instantiate(tunnel, player.position, Quaternion.identity);
                    LineRenderer tunnelLineRenderer = tunnelInstance.GetComponent<LineRenderer>();
                    tunnelLineRenderer.SetPosition(0, hit.point);
                    tunnelLineRenderer.SetPosition(1, (hit.point + exitPoint) / 2);
                    tunnelLineRenderer.SetPosition(2, exitPoint);

                    inTunnel = true;
                    StartCoroutine(MoveThroughTunnel(exitPoint));
                    
                    playerAudioSource.PlayOneShot(playerHitSound.clip, playerHitSound.volume);
                }
            }
        }

        // Reset the player's position
        if (movable && Input.GetKeyDown(KeyCode.R)) {
            ResetPlayer();
        }

        // If the player is not movable, start a timer and reset when it reaches a certain value
        if(!movable) {
            delay += Time.deltaTime;
            if(playerRigidbody.velocity.magnitude < minSpeed) {
                stillDelay += Time.deltaTime;
            } else {
                stillDelay = 0f;
            }
            if (delay > minDelay && playerRigidbody.velocity.magnitude < minSpeed && stillDelay > whenStillDelay) {
                ResetPlayer();
            }
        }
    }

    void Jump() {
        if (onPlayerShoot != null)
            onPlayerShoot.Invoke();

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

    // Coroutine to move player through a generated tunnel
    IEnumerator MoveThroughTunnel(Vector2 exitPoint) {
        // Play tunnel sound
        playerAudioSource.PlayOneShot(tunnelDig.clip, tunnelDig.volume);

        lineRenderer.enabled = false;
        Vector2 initPoint = player.position;
        exitPoint = exitPoint + (exitPoint - initPoint).normalized * 0.15f;
        float distance = Vector2.Distance(initPoint, exitPoint);
        float power = (player.position - Camera.main.ScreenToWorldPoint(Input.mousePosition)).magnitude;
        float totalTime = (2f - Mathf.Log(power, 10f)) * distance;
        float time = 0f;

        // Disable player's collider
        player.GetComponent<Collider2D>().enabled = false;
        movable = false;

        while (time < totalTime) {
            time += Time.deltaTime;
            player.position = Vector2.Lerp(player.position, exitPoint, time / stepMoveTime);
            yield return null;
        }
        player.position = exitPoint;

        player.GetComponent<Collider2D>().enabled = true;
        movable = true;
        inTunnel = false;
    }

    void SetMovable(bool move = true) {
        if (move) {
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            movable = true;
        } else {
            playerRigidbody.constraints = RigidbodyConstraints2D.None;
            movable = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        // If the player hits the ground, unfreeze it
        // ONly when it hits "Earth", layer 6
        if (collision.gameObject.layer ==  6) {
            if(nextLandingIsSpawn) {
                spawnPosition = player.position;
                nextLandingIsSpawn = false;
            }

            movable = true;
            curParticleSystem = Instantiate(groundHitParticles, player.position, Quaternion.identity);
            // Set the particle system speed and amount based on velocity of player
            var main = curParticleSystem.GetComponent<ParticleSystem>().main;
            main.startSpeed = baseExplosionPower + playerRigidbody.velocity.magnitude * explosionPower;
            // Update the amount of particles as well
            var emission = curParticleSystem.GetComponent<ParticleSystem>().emission;
            emission.rateOverTime = baseExplosionPower + playerRigidbody.velocity.magnitude * explosionPower + 40f;
            
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
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

            var goal = collision.GetComponent<Goal>();
            goal.Reach();

            // Get all positions in the goal's linerenderer
            LineRenderer otherLR = collision.GetComponent<LineRenderer>();
            // Make goalPositions the same length as the goal's linerenderer
            Vector3[] goalPositions = new Vector3[otherLR.positionCount];
            otherLR.GetPositions(goalPositions);
            if (!otherLR.useWorldSpace) {
                for (int i = 0; i < goalPositions.Length; i++) {
                    var pos = goalPositions[i];
                    goalPositions[i] = collision.transform.localToWorldMatrix * new Vector4(pos.x, pos.y, 0, 1);
                }
            }
            StartCoroutine(MoveAlongPath(goalPositions, goal));
            onPlayerHitGoal.Invoke(collision);
            // Play sound
            playerAudioSource.PlayOneShot(playerHitSound.clip, playerHitSound.volume + playerRigidbody.velocity.magnitude * playerVelocitySoundMultiplier > 1f ? 1f : playerHitSound.volume + playerRigidbody.velocity.magnitude * playerVelocitySoundMultiplier);
        }
    }

    // Coroutine to move the player along the path of the goal
    private IEnumerator MoveAlongPath(Vector3[] stepPositions, Goal goal) {
        int numSteps = stepPositions.Length;
        float timer = 0f;
        int currentStep = 0;
        movable = false;
        playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        gameObject.GetComponent<Collider2D>().enabled = false;
        
        // Move the player along the path
        while (timer < stepMoveTime)  {
            delay = 0;
            timer += Time.deltaTime;
            if (currentStep == numSteps - 1) {
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
        player.position = stepPositions[numSteps - 1];
        gameObject.GetComponent<Collider2D>().enabled = true;
        playerRigidbody.constraints = RigidbodyConstraints2D.None;
        playerRigidbody.velocity = numSteps > 1 ? (stepPositions[numSteps - 1] - stepPositions[numSteps - 2]) / stepMoveTime : Vector2.zero;
        playerRigidbody.simulated = true;
        spawnPosition = player.position;
        nextLandingIsSpawn = true;
        movable = false;
        delay = 0;

        goal.Exit();
    }

    private void ResetPlayer() {
        player.position = spawnPosition;
        playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        movable = true;
        Reset.ResetAll(true);
        playerAudioSource.PlayOneShot(playerResetSound.clip, playerResetSound.volume);

        if (onPlayerReset != null) onPlayerReset.Invoke();
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