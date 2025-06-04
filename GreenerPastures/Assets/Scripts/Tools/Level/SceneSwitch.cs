using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("NYFA Studio/Level/SceneSwitch")]
public class SceneSwitch : MonoBehaviour
{
    // Author: Glenn Storm
    // This triggers a scene to load, as long as it is configured in the Build Settings

    public string sceneName;
    public bool useDelay;
    public float switchDelay;

    private float switchTimer;


    void Start()
    {
        if ( sceneName == "" )
        {
            Debug.LogError("--- SceneSwitch [Start] : no scene name configured. aborting.");
            enabled = false;
        }
        else
        {
            if ( SceneManager.GetSceneByName(sceneName) == null )
            {
                Debug.LogError("--- SceneSwitch [Start] : scene name "+sceneName+" found in Build Settings. aborting.");
                enabled = false;
            }
        }
        if ( useDelay && switchDelay <= 0f )
            Debug.LogWarning("--- SceneSwitch [Start] : use delay is on, but no switch delay is configured. will ignore.");


        if (enabled)
        {
            if (useDelay)
                switchTimer = switchDelay;
            else
                SceneManager.LoadScene(sceneName);
        }
    }

    void Update()
    {
        if (useDelay)
        {
            switchTimer -= Time.deltaTime;
            if ( switchTimer < 0f )
            {
                SceneManager.LoadScene(sceneName);
            }
        }
    }
}
