using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("NYFA Studio/Interface/MainMenu")]
public class MainMenu : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the main menu

    public string titleText;
    [Tooltip("These values represent a proportion of screen space, as percentages of screen space. Like, less than 1")]
    public Rect title;

    public Font titleFont;
    public FontStyle titleFontStyle;
    public Color titleFontColor = Color.white;
    [Tooltip("Font will scale appropriately based on active screen width, using this 1024 size")]
    public int titleFontSizeAt1024 = 60;

    [System.Serializable]
    public struct ButtonDef
    {
        public string buttonText;
        public Rect buttonPos;
        public string sceneName;
    }
    public ButtonDef[] buttons;

    public Font buttonFont;
    public FontStyle buttonFontStyle;
    public Color buttonFontColor = Color.white;
    [Tooltip("Font will scale appropriately based on active screen width, using this 1024 size")]
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
        g.fontStyle = titleFontStyle;
        g.fontSize = Mathf.RoundToInt(titleFontSizeAt1024 * (w / 1024f));
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = titleFontColor;
        g.active.textColor = titleFontColor;
        string s = titleText;

        r = title;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;

        GUI.Label(r, s, g);

        if (buttons == null || buttons.Length == 0)
            return;

        // buttons array
        g = new GUIStyle(GUI.skin.button);
        g.font = buttonFont;
        g.fontStyle = buttonFontStyle;
        g.fontSize = Mathf.RoundToInt(buttonFontSizeAt1024 * (w / 1024f));
        g.normal.textColor = buttonFontColor;
        g.active.textColor = buttonFontColor;
        for ( int i=0; i<buttons.Length; i++)
        {
            r = buttons[i].buttonPos;
            r.x *= w;
            r.y *= h;
            r.width *= w;
            r.height *= h;
            s = buttons[i].buttonText;

            if (GUI.Button(r,s,g))
            {
                if (buttons[i].sceneName != "")
                {
                    // disable menu vfx when not in menu
                    GameObject.Find("VFX_Splash").SetActive(!(buttons[i].sceneName == "Proto_GreenerStuff"));
                    SceneManager.LoadScene(buttons[i].sceneName);
                }
                else
                    Application.Quit();
            }
        }
    }
}
