using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Interactable : MonoBehaviour
{
    [SerializeField]
    UnityEvent onTrigger, onHit;

    private void OnTriggerEnter2D(Collider2D collision) {
        onTrigger.Invoke();
    }
    
    private void OnCollisionEnter2D(Collision2D collision) {
        onHit.Invoke();
    }   
}

