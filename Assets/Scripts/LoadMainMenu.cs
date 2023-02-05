using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMainMenu : MonoBehaviour
{
    [SerializeField]
    string levelToLoad;

    public void LoadLevel() {
        SceneManager.LoadSceneAsync(levelToLoad);
    }
}
