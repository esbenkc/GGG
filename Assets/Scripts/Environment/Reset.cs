using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reset : MonoBehaviour
{
    public void ResetAll(bool full = false) {
        // Find all connectors and reset them
        foreach (Connector connector in FindObjectsOfType<Connector>()) {
            connector.Reset();
        }
    }
}
