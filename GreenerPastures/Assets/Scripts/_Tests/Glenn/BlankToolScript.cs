using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlankToolScript : MonoBehaviour
{
    // Author: Glenn Storm
    // This is not an actual tool to be used
    // (just needed a placeholder to demontrate the usual tools we make)

    public GameData game;

    // Start is called before the first frame update
    void Start()
    {
        // easy access to static classes and functions everywhere :)
        game = GameSystem.InitializeGame();        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
