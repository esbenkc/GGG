using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Reset
{
    public static void ResetAll(bool full = false) {
        // Find all connectors and reset them
        foreach (Connector connector in GameObject.FindObjectsByType<Connector>(FindObjectsSortMode.None)) {
            connector.Reset(full);
        }
    }
}
