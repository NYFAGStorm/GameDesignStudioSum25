using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("NYFA Studio/Game/QuitOnEscape")]
public class QuitOnEscape : MonoBehaviour
{
    // Author: Glenn Storm
    // This quits to the main menu if ESC is pressed

    private bool popup;

    const int FONTSIZEAT1024 = 36;

    void Update()
    {
        if ( Input.GetKeyUp(KeyCode.Escape) )
			popup = true;
    }

    void OnGUI()
    {
        if (!popup)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;
        GUIStyle g = new GUIStyle();
        string s = "\nAre You Sure You Want To Quit?";

        r.x = 0.2f * w;
        r.y = 0.3f * h;
        r.width = 0.6f * w;
        r.height = 0.4f * h;
        g = GUI.skin.box;
        g.fontSize = Mathf.RoundToInt( FONTSIZEAT1024 * (w/1024f) );
        g.alignment = TextAnchor.UpperCenter;
        GUI.Box(r, s, g);

        r.x = 0.25f * w;
        r.y = 0.55f * h;
        r.width = 0.2f * w;
        r.height = 0.1f * h;
        g.alignment = TextAnchor.MiddleCenter;
        s = "QUIT";
        if (GUI.Button(r, s, g))
        {
            popup = false;
            SceneManager.LoadScene("Menu");
        }

        r.x = 0.55f * w;
        s = "CANCEL";
        if ( GUI.Button(r,s,g))
        {
            popup = false;
        }
    }
}
