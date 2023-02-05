using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSounds : MonoBehaviour
{
    [SerializeField] AudioClip[] drops;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlayDropRandomly());
    }

    public void PlayDrop() {
        audioSource.PlayOneShot(drops[Random.Range(0, drops.Length)]);
    }

    // Play drop sound randomly every 1-3 seconds
    IEnumerator PlayDropRandomly() {
        while(true) {
            yield return new WaitForSeconds(Random.Range(1.0f, 3.0f));
            PlayDrop();
        }
    }


}
