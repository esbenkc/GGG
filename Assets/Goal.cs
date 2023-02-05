using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Goal : MonoBehaviour
{
    [SerializeField]
    UnityEvent onGoalReached;
    // Start is called before the first frame update
    
    public void Reach() {
        if(onGoalReached != null) onGoalReached.Invoke();
    }
}
