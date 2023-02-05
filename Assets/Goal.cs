using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Goal : MonoBehaviour
{
    [SerializeField]
    UnityEvent onGoalReached, onExit;
    // Start is called before the first frame update
    
    public void Reach() {
        if(onGoalReached != null) onGoalReached.Invoke();
    }

    public void Exit() {
        if (onExit != null) onExit.Invoke();

    }
}
