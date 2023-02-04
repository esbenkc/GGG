using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    [SerializeField]
    LayerMask grassLayer = 64;

    private void OnCollisionEnter2D(Collision2D collision) {
        if ((1<<collision.gameObject.layer & grassLayer.value) == 0) return;

        collision.gameObject.GetComponent<GrassComponent>().StartAnimating(collision.contacts[0].point, collision.contacts[0].normal);
    }
}
