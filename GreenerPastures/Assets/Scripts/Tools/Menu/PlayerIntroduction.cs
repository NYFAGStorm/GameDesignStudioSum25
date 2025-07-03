using UnityEngine;

public class PlayerIntroduction : MonoBehaviour
{
    // Author: Glenn Storm
    // Handles a popup seen only when a new player arrives, to configure options

    public bool introPop;

    private Texture2D[] characterLines;
    private Texture2D[] characterSkins;
    private Texture2D[] characterAccents;
    private Texture2D[] characterFills;

    private Color[] characterSkinTones;
    private Color[] characterColors;

    private PlayerOptions configOptions;
    private bool configValid;
    private bool[] configsTouched = new bool[4];

    private int modelSelection = 0;
    private int skinSelection = 0;
    private int accentSelection = 0;
    private int fillSelection = 0;


    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {
            InitializeCharacterTextures();
            InitializeCharacterColors();
        }
    }

    void InitializeCharacterTextures()
    {
        characterLines = new Texture2D[2];
        characterLines[0] = (Texture2D)Resources.Load("ProtoWizard_LineArt");
        characterLines[1] = (Texture2D)Resources.Load("ProtoWizardF_LineArt");
        characterSkins = new Texture2D[2];
        characterSkins[0] = (Texture2D)Resources.Load("ProtoWizard_FillSkin");
        characterSkins[1] = (Texture2D)Resources.Load("ProtoWizardF_FillSkin");
        characterAccents = new Texture2D[2];
        characterAccents[0] = (Texture2D)Resources.Load("ProtoWizard_FillAccent");
        characterAccents[1] = (Texture2D)Resources.Load("ProtoWizardF_FillAccent");
        characterFills = new Texture2D[2];
        characterFills[0] = (Texture2D)Resources.Load("ProtoWizard_FillMain");
        characterFills[1] = (Texture2D)Resources.Load("ProtoWizardF_FillMain");
    }

    void InitializeCharacterColors()
    {
        characterSkinTones = new Color[8];
        for (int i = 0; i < 8; i++)
        {
            characterSkinTones[i] = PlayerSystem.GetPlayerSkinColor((PlayerSkinColor)i);
        }
        characterColors = new Color[8];
        for (int i = 0; i < 8; i++)
        {
            characterColors[i] = PlayerSystem.GetPlayerColor((PlayerColor)i);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            introPop = !introPop;
    }

    public void SetPlayerOptions( PlayerOptions options )
    {
        configOptions = options;
    }

    public PlayerOptions GetPlayerOptions()
    {
        return configOptions;
    }

    void TryConfigPlayerInScene( PlayerOptions options )
    {
        PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        if (pcm == null)
            return;
        pcm.playerData.options = options;
        pcm.ConfigureAppearance(options);
    }

    void OnGUI()
    {
        if (!introPop)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        r.x = 0.1f * w;
        r.y = 0.1f * h;
        r.width = 0.8f * w;
        r.height = 0.8f * h;

        GUIStyle g = new GUIStyle(GUI.skin.box);
        g.fontSize = Mathf.RoundToInt(20f * ( w / 1024f ));
        g.fontStyle = FontStyle.Bold;
        g.padding = new RectOffset(0, 0, 30, 0);
        g.normal.textColor = Color.black;
        g.hover.textColor = Color.black;
        g.active.textColor = Color.black;
        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.white;
        c.r = 0.85f;
        c.g = 0.8f;
        c.b = 0.618f;
        c.a = 1f;
        g.normal.background = t;
        g.hover.background = t;
        g.active.background = t;
        GUI.color = c;
        string s = "Player Character Customization";

        GUI.Box(r, s, g);

        // CHARACTER IMAGE BOX
        r.x = 0.15f * w;
        r.y = 0.2f * h;
        r.width = 0.325f * w;
        r.height = r.width; // square
        g = new GUIStyle(GUI.skin.box);
        s = "";
        GUI.color = Color.white;
        GUI.Box(r, s, g);
        // frame
        r.x += 0.0125f * w;
        r.y += 0.0125f * w;
        r.width -= 0.025f * w;
        r.height = r.width;
        t = Texture2D.whiteTexture;
        c = Color.white;
        c.r = 0.85f;
        c.g = 0.8f;
        c.b = 0.618f;
        c.a = 1f;
        g.normal.background = t;
        g.hover.background = t;
        g.active.background = t;
        GUI.color = c;
        GUI.Box(r, s, g);
        // bg
        r.x += 0.0125f * w;
        r.y += 0.0125f * w;
        r.width -= 0.025f * w;
        r.height = r.width;
        t = Texture2D.whiteTexture;
        g.normal.background = t;
        g.hover.background = t;
        g.active.background = t;
        c = Color.white;
        GUI.color = c;
        GUI.Box(r, s, g);
        // main
        t = characterFills[modelSelection];
        g.normal.background = t;
        g.hover.background = t;
        g.active.background = t;
        c = characterColors[fillSelection];
        c.a = 1f;
        GUI.color = c;
        GUI.Box(r, s, g);
        // accent
        t = characterAccents[modelSelection];
        g.normal.background = t;
        g.hover.background = t;
        g.active.background = t;
        c = characterColors[accentSelection];
        GUI.color = c;
        GUI.Box(r, s, g);
        // skin
        t = characterSkins[modelSelection];
        g.normal.background = t;
        g.hover.background = t;
        g.active.background = t;
        c = characterSkinTones[skinSelection];
        GUI.color = c;
        GUI.Box(r, s, g);
        // line
        t = characterLines[modelSelection];
        g.normal.background = t;
        g.hover.background = t;
        g.active.background = t;
        c = Color.white;
        GUI.color = c;
        GUI.Box(r, s, g);

        // CHARACTER DETAILS
        // name label
        r.x = 0.5f * w;
        r.y = 0.3f * h;
        r.width = 0.375f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
        g.fontStyle = FontStyle.BoldAndItalic;
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = Color.black;
        g.hover.textColor = Color.black;
        g.active.textColor = Color.black;
        s = "New Player";
        GUI.color = Color.white;
        GUI.Label(r, s, g);
        // model label
        r.x = 0.55f * w;
        r.y += 0.075f * h;
        r.width = 0.3f * w;
        g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        s = "PLAYER MODEL";
        GUI.Label(r, s, g);
        // skin tone label
        r.y += 0.075f * h;
        s = "SKIN TONE";
        GUI.Label(r, s, g);
        // main color label
        r.y += 0.075f * h;
        s = "MAIN COLOR";
        GUI.Label(r, s, g);
        // accent color label
        r.y += 0.075f * h;
        s = "ACCENT COLOR";
        GUI.Label(r, s, g);

        // CONTROL BUTTONS
        r.x = 0.525f * w;
        r.y = 0.35f * h;
        r.width = 0.05f * w;
        r.height = r.width; // square
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(24f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = Color.black;
        g.hover.textColor = Color.black;
        g.active.textColor = Color.yellow;
        GUI.color = Color.white;
        // model -/+
        s = "<";
        if (GUI.Button(r, s, g))
        {
            modelSelection--;
            if (modelSelection < 0)
                modelSelection = characterLines.Length-1;
            configsTouched[0] = true;
        }
        r.x += 0.3f * w;
        s = ">";
        if (GUI.Button(r, s, g))
        {
            modelSelection++;
            if (modelSelection > characterLines.Length - 1)
                modelSelection = 0;
            configsTouched[0] = true;
        }
        // skin -/+
        r.x = 0.525f * w;
        r.y += 0.075f * h;
        s = "<";
        if (GUI.Button(r, s, g))
        {
            skinSelection--;
            if (skinSelection < 0)
                skinSelection = characterSkinTones.Length - 1;
            configsTouched[1] = true;
        }
        r.x += 0.3f * w;
        s = ">";
        if (GUI.Button(r, s, g))
        {
            skinSelection++;
            if (skinSelection > characterSkinTones.Length - 1)
                skinSelection = 0;
            configsTouched[1] = true;
        }
        // main -/+
        r.x = 0.525f * w;
        r.y += 0.075f * h;
        s = "<";
        if (GUI.Button(r, s, g))
        {
            fillSelection--;
            if (fillSelection < 0)
                fillSelection = characterColors.Length - 1;
            configsTouched[2] = true;
        }
        r.x += 0.3f * w;
        s = ">";
        if (GUI.Button(r, s, g))
        {
            fillSelection++;
            if (fillSelection > characterColors.Length - 1)
                fillSelection = 0;
            configsTouched[2] = true;
        }
        // accent -/+
        r.x = 0.525f * w;
        r.y += 0.075f * h;
        s = "<";
        if (GUI.Button(r, s, g))
        {
            accentSelection--;
            if (accentSelection < 0)
                accentSelection = characterColors.Length - 1;
            configsTouched[3] = true;
        }
        r.x += 0.3f * w;
        s = ">";
        if (GUI.Button(r, s, g))
        {
            accentSelection++;
            if (accentSelection > characterColors.Length - 1)
                accentSelection = 0;
            configsTouched[3] = true;
        }

        // store player options
        configOptions.model = (PlayerModelType)modelSelection+1;
        configOptions.skinColor = (PlayerSkinColor)skinSelection;
        configOptions.mainColor = (PlayerColor)fillSelection;
        configOptions.accentColor = (PlayerColor)accentSelection;

        // detect config valid
        configValid = (configsTouched[0] && configsTouched[1] &&
            configsTouched[2] && configsTouched[3]);

        // accept button
        r.x = 0.4f * w;
        r.y = 0.8f * h;
        r.width = 0.2f * w;
        r.height = 0.075f * h;
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.white;
        s = "ACCEPT";
        GUI.enabled = configValid;
        if (GUI.Button(r,s,g))
        {
            introPop = false;
            // confirm player options
            TryConfigPlayerInScene(configOptions);
        }
    }
}
