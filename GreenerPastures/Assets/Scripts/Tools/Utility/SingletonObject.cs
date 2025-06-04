using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NYFA Studio/Utility/SingletonObject")]
public class SingletonObject : MonoBehaviour
{
    // Author: Glenn Storm
    // This ensures the game object will:
    //  a) not be deleted between scenes
    //  b) not be duplicated when scene is reloaded
    // It will do this by matching the game object name

    void Awake()
    {
        // upon existence, search scene for all singletons
        SingletonObject[] so = GameObject.FindObjectsByType<SingletonObject>(FindObjectsSortMode.None);
        // ensure this is not an already existing singleton
        bool exists = false;
        foreach ( SingletonObject s in so )
        {
            if (s != this && s.gameObject.name == gameObject.name)
            {
                exists = true;
                break;
            }
        }
        // only the first instance of this singleton will remain
        if (exists)
            Destroy(gameObject);
    }

    void Start()
    {
        gameObject.transform.parent = null;
        DontDestroyOnLoad(gameObject);
    }
}
