using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("NYFA Studio/Interface/CreditsScreen")]
public class CreditsScreen : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the credits screen

    public string titleText;
    [Tooltip("These values represent a proportion of screen space, as percentages of screen space. Like, less than 1")]
    public Rect title;

    public Font titleFont;
    public FontStyle titleFontStyle;
    public Color titleFontColor = Color.white;
    [Tooltip("Font will scale appropriately based on active screen width, using this 1024 size")]
    public int titleFontSizeAt1024 = 60;

    [System.Serializable]
    public struct CreditListing
    {
        public string creditText;
        public Rect creditPos;
        public bool alignRight;
    }
    public CreditListing[] credits;
    public Font creditFont;
    public FontStyle creditsFontStyle;
    public Color creditFontColor = Color.white;
    public int creditFontSizeAt1024 = 40;

    public string backButtonText = "BACK";
    public Rect backButton;

    public Font buttonFont;
    public FontStyle buttonFontStyle;
    public Color buttonFontColor = Color.white;
    [Tooltip("Font will scale appropriately based on active screen width, using this 1024 size")]
    public int backButtonFontSizeAt1024 = 48;


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
        g.normal.textColor = buttonFontColor;
        g.active.textColor = buttonFontColor;
        string s = titleText;

        r = title;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;

        GUI.Label(r, s, g);

        for ( int i=0; i<credits.Length; i++ )
        {
            r = credits[i].creditPos;
            r.x *= w;
            r.y *= h;
            r.width *= w;
            r.height *= h;
            g.font = creditFont;
            g.fontStyle = creditsFontStyle;
            g.fontSize = Mathf.RoundToInt(creditFontSizeAt1024 * (w / 1024f));
            g.alignment = TextAnchor.MiddleLeft;
            if ( credits[i].alignRight )
                g.alignment = TextAnchor.MiddleRight;
            g.normal.textColor = creditFontColor;
            s = credits[i].creditText;
            GUI.Label(r, s, g);
        }

        r = backButton;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;
        g = new GUIStyle(GUI.skin.button);
        g.font = buttonFont;
        g.fontStyle = buttonFontStyle;
        g.fontSize = Mathf.RoundToInt(backButtonFontSizeAt1024 * (w / 1024f));
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = buttonFontColor;
        s = backButtonText;

        if (GUI.Button(r, s, g))
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
