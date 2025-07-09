using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("NYFA Studio/Interface/CreditsScreen")]
public class CreditsScreen : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the credits screen

    public enum CreditAlign
    {
        Left,
        Right,
        Center
    }

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
        public int creditPage;
        public Rect creditPos;
        public CreditAlign creditAlign;
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

    private MultiGamepad padMgr;
    private int padButtonSelection = -1;
    private int padMaxButton = 0;

    private int currentPage;
    private int maxPage;
    private float pageTimer;

    private Texture2D[] buttonTex;

    const float CREDITPAGETIME = 5f;


    void Start()
    {
        // validate
        if (credits == null || credits.Length == 0)
        {
            Debug.LogWarning("--- CreditsScreen [Start] : no credit listing configured. will ignore.");
        }
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
        {
            Debug.LogError("--- CreditsScreen [Start] : no pad manager found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            maxPage = 0;
            for (int i = 0; i < credits.Length; i++)
            {
                if (credits[i].creditPage > maxPage)
                    maxPage = credits[i].creditPage;
            }
            pageTimer = CREDITPAGETIME;

            // GUI Button Textures for build
            if (!Application.isEditor)
            {
                buttonTex = new Texture2D[3];
                buttonTex[0] = (Texture2D)Resources.Load("Button_Normal");
                buttonTex[1] = (Texture2D)Resources.Load("Button_Hover");
                buttonTex[2] = (Texture2D)Resources.Load("Button_Active");
            }
        }
    }

    void Update()
    {
        // run credits page timer
        if (pageTimer > 0f)
        {
            pageTimer -= Time.deltaTime;
            if (pageTimer < 0f)
            {
                pageTimer = CREDITPAGETIME;
                currentPage++;
                if (currentPage > maxPage)
                    currentPage = 0;
            }
        }

        // determine ui selection from game pad input
        if (padMgr.gPadDown[0].YaxisL > 0f)
        {
            padButtonSelection--;
            if (padButtonSelection < 0)
                padButtonSelection = padMaxButton;
        }
        else if (padMgr.gPadDown[0].YaxisL < 0f)
        {
            padButtonSelection++;
            if (padButtonSelection > padMaxButton)
                padButtonSelection = 0;
        }
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
            if (credits[i].creditPage != currentPage)
                continue;
            r = credits[i].creditPos;
            r.x *= w;
            r.y *= h;
            r.width *= w;
            r.height *= h;
            g.font = creditFont;
            g.fontStyle = creditsFontStyle;
            g.fontSize = Mathf.RoundToInt(creditFontSizeAt1024 * (w / 1024f));
            g.alignment = TextAnchor.MiddleLeft;
            if ( credits[i].creditAlign == CreditAlign.Right )
                g.alignment = TextAnchor.MiddleRight;
            else if (credits[i].creditAlign == CreditAlign.Center )
                g.alignment = TextAnchor.MiddleCenter;
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
        if (padButtonSelection == 0)
            g.normal.textColor = Color.white;
        g.active.textColor = buttonFontColor;
        if (!Application.isEditor)
        {
            g.normal.background = buttonTex[0];
            g.hover.background = buttonTex[1];
            g.active.background = buttonTex[2];
        }
        s = backButtonText;

        if (GUI.Button(r, s, g) ||
            padButtonSelection == 0 && padMgr.gPadDown[0].aButton)
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
