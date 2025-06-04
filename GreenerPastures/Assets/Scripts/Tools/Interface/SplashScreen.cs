using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("NYFA Studio/Interface/SplashScreen")]
public class SplashScreen : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the splash screen

    public string titleText;
    [Tooltip("These values represent a proportion of screen space, as percentages of screen space. Like, less than 1")]
    public Rect title;

    public Font titleFont;
    public Color titleFontColor = Color.white;
    [Tooltip("Font will scale appropriately based on active screen width, using this 1024 size")]
    public int titleFontSizeAt1024 = 60;

    public string startButtonText = "START";
    public Rect startButton;

    public Font buttonFont;
    public Color buttonFontColor = Color.white;
    public int buttonFontSizeAt1024 = 48;


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnGUI()
    {
        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;
        GUIStyle g = new GUIStyle();
        g.font = titleFont;
        g.fontSize = Mathf.RoundToInt(titleFontSizeAt1024 * (w/1024f));
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = buttonFontColor;
        g.active.textColor = buttonFontColor;
        string s = titleText;

        r = title;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;

        GUI.Label(r, s, g);

        r = startButton;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;
        g = new GUIStyle(GUI.skin.button);
        g.font = buttonFont;
        g.fontSize = Mathf.RoundToInt(buttonFontSizeAt1024 * (w / 1024f));
        s = startButtonText;

        if (GUI.Button(r,s,g))
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
