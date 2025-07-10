using UnityEngine;

public class InGameAlmanac : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the mid-game menu biomancer's almanac

    public bool showAlmanac;

    public AlmanacData almanac;
    public int currentCategory;
    public int currentEntry;

    private int[] startingEntry;
    private int[] entriesInCategory;

    private bool updateAlmanac;
    private int revealedEntryIndex = -1;

    private PlayerControlManager pcm;
    private MultiGamepad padMgr;
    private QuitOnEscape qoe;
    private InGameControls igc;

    private Texture2D[] buttonTex;

    const int ENTRIESPERPAGE = 3;


    void Start()
    {
        // validate
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
            Debug.LogWarning("--- InGameAlmanac [Start] : no gamepad manager found in scene. will ignore.");
        qoe = GameObject.FindFirstObjectByType<QuitOnEscape>();
        if (qoe == null)
        {
            Debug.LogError("--- InGameAlmanac [Start] : no quit on escape tool found in scene. aborting.");
            enabled = false;
        }
        igc = GameObject.FindFirstObjectByType<InGameControls>();
        if (igc == null)
        {
            Debug.LogError("--- InGameAlmanac [Start] : no in game controls tool found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            InitAlmanac();

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

    void InitAlmanac()
    {
        almanac = AlmanacSystem.InitializeAlmanac();
        currentCategory = 0;
        currentEntry = 0;
        ConfigureEntryCount();
    }

    void ConfigureEntryCount()
    {
        startingEntry = new int[9];
        entriesInCategory = new int[9];
        entriesInCategory[0] = -1;
        int entryCount = 0;
        int categoryCount = 0;
        bool newCategory = true;
        AlmanacCateogory currentCategory = AlmanacCateogory.Default;
        for (int i = 0; i < almanac.entries.Length; i++)
        {
            if (newCategory)
            {
                startingEntry[categoryCount] = entryCount;
                currentCategory = almanac.entries[i].category;
                categoryCount++;
                newCategory = false;
                entryCount = 0;
            }
            if (almanac.entries[i].category != currentCategory)
                newCategory = true;
            entryCount++;
            entriesInCategory[((int)currentCategory) - 1]++;
        }
    }

    void Update()
    {
        if (pcm == null)
            return;

        if (updateAlmanac && pcm.playerData != null && 
            pcm.playerData.almanac.revealed != null)
        {
            updateAlmanac = false;

            InitAlmanac();

            if (pcm.playerData.almanac.revealed.Length == 0)
            {
                // set almanac discovery data for new player
                bool[] discoveredEntries = AlmanacSystem.GetAlmanacRevealedFlags(almanac);
                pcm.playerData.almanac.revealed = discoveredEntries;
            }
            else
            {
                // fill almanac discovery data from returning player
                bool[] discoveredEntries = pcm.playerData.almanac.revealed;
                almanac = AlmanacSystem.SetAlmanacRevealedFlags(almanac, discoveredEntries);
            }
            // lorem the almanac hidden entries
            int count = 0;
            for (int i = 0; i < almanac.entries.Length; i++)
            {
                if (!almanac.entries[i].revealed)
                {
                    AlmanacEntry loremEntry = AlmanacSystem.GenerateLoremAlmanacEntry(almanac.entries[i]);
                    almanac.entries[i] = loremEntry;
                    count++;
                }
            }
            // notify player of newly revealed entry
            if (revealedEntryIndex > -1)
            {
                GreenerGameManager ggm = GameObject.FindFirstObjectByType<GreenerGameManager>();
                if (ggm != null)
                    ggm.AddNotification("Almanac entry REVEALED\nin category "+almanac.entries[revealedEntryIndex].category.ToString()+"\n'" + almanac.entries[revealedEntryIndex].title +"'");
                revealedEntryIndex = -1;

            }
        }

        if (igc.controlsDisplay)
        {
            showAlmanac = false;
            return;
        }

        if ( Input.GetKeyDown(KeyCode.Backslash) )
        {
            showAlmanac = !showAlmanac;
            // control player hud
            if (showAlmanac && !pcm.hidePlayerHUD)
                pcm.hidePlayerHUD = true;
            if (!showAlmanac && pcm.hidePlayerHUD)
                pcm.hidePlayerHUD = false;
        }

    }

    /// <summary>
    /// Configures the local player control manager reference and triggers almanac data update
    /// </summary>
    /// <param name="pControlManager">player control manager</param>
    public void SetPlayerControlManager(PlayerControlManager pControlManager)
    {
        if (pcm != null)
            return;
        pcm = pControlManager;
        updateAlmanac = true;
    }

    /// <summary>
    /// Sets an entry in the Biomancer's Almanac to be revealed for the local player
    /// </summary>
    /// <param name="entryTitle">the entry title</param>
    public void AlmanacReveal(string entryTitle)
    {
        int idx = AlmanacSystem.GetAlmanacEntryIndex(almanac, entryTitle);
        if (pcm != null && pcm.playerData != null &&
            pcm.playerData.almanac.revealed.Length > 0)
        {
            pcm.playerData.almanac.revealed[idx] = true;
            updateAlmanac = true;
            revealedEntryIndex = idx;
        }
    }

    /// <summary>
    /// Sets an entry in the Biomancer's Almanac to be revealed for the local player
    /// </summary>
    /// <param name="entryIndex">the entry index</param>
    public void AlmanacReveal(int entryIndex)
    {
        if (pcm != null && pcm.playerData != null &&
            pcm.playerData.almanac.revealed.Length > 0)
        {
            pcm.playerData.almanac.revealed[entryIndex] = true;
            updateAlmanac = true;
            revealedEntryIndex = entryIndex;
        }
    }

    void OnGUI()
    {
        if (pcm == null || !showAlmanac)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        r.x = 0.1f * w;
        r.y = 0.15f * h;
        r.width = 0.8f * w;
        r.height = 0.7f * h;

        GUIStyle g = new GUIStyle(GUI.skin.box);
        g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.padding = new RectOffset(0, 0, 20, 0);
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;

        string s = "BIOMANCER'S ALMANAC";

        Color c = Color.white;

        GUI.Box(r, s, g);

        // PAGE BOX
        r.x = 0.125f * w;
        r.y = 0.125f * w;
        r.width = 0.75f * w;
        r.height = 0.55f * h;
        g = new GUIStyle(GUI.skin.box);
        c = Color.white;
        c.r = 0.85f;
        c.g = 0.84f;
        c.b = 0.78f;
        c.a = 1f;
        Texture2D t = Texture2D.whiteTexture;
        g.normal.background = t;
        GUI.color = c;
        GUI.Box(r, "", g);

        // CATEGORY TABS (press to begin at top of category)
        r.x = 0.1625f * w;
        r.y = 0.225f * h;
        r.width = 0.075f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(12f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        c = Color.white;
        c *= 0.8f;
        g.normal.textColor = c;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.black;
        if (!Application.isEditor)
        {
            g.normal.background = buttonTex[0];
            g.hover.background = buttonTex[1];
            g.active.background = buttonTex[2];
        }
        for (int i = 1; i < 10; i++)
        {
            if (i == (currentCategory+1))
                g.normal.textColor = Color.white;
            else
                g.normal.textColor = c;
            s = ((AlmanacCateogory)i).ToString().ToUpper();
            if (GUI.Button(r, s, g))
            {
                currentCategory = i-1;
                currentEntry = startingEntry[currentCategory];
            }
            r.x += 0.075f * w;
        }

        // CATEGORY ENTRIES
        r.x = 0.175f * w;
        r.y = 0.25f * h;
        r.width = .6f * w;
        r.height = 0.05f * w;
        g = new GUIStyle(GUI.skin.label);
        g.normal.textColor = Color.black;
        g.hover.textColor = Color.black;
        g.active.textColor = Color.black;
        for ( int n = 0; n < ENTRIESPERPAGE; n++ )
        {
            AlmanacEntry displayEntry = almanac.entries[currentEntry + n];
            // TODO: icon display
            r.x = 0.175f * w;
            r.height = 0.05f * w;
            r.width = .6f * w;
            g.fontSize = Mathf.RoundToInt(14f * (w / 1024f));
            g.fontStyle = FontStyle.BoldAndItalic;
            g.alignment = TextAnchor.MiddleLeft;
            g.wordWrap = false;
            // title
            s = displayEntry.title;
            GUI.Label(r, s, g);
            // subtitle
            g.fontSize = Mathf.RoundToInt(12f * (w / 1024f));
            g.fontStyle = FontStyle.Italic;
            g.alignment = TextAnchor.MiddleRight;
            s = displayEntry.subtitle;
            GUI.Label(r, s, g);
            // description
            r.y += 0.06f * h;
            r.height = 0.04f * w;
            g.fontSize = Mathf.RoundToInt(12f * (w / 1024f));
            g.fontStyle = FontStyle.Normal;
            g.alignment = TextAnchor.UpperLeft;
            g.wordWrap = true;
            s = displayEntry.description;
            GUI.Label(r, s, g);
            // details
            r.x += 0.01f * w;
            r.y += 0.075f * h;
            r.width = 0.1225f * w;
            r.height = 0.05f * h;
            g.fontSize = Mathf.RoundToInt(10f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            if (displayEntry.details != null)
            {
                for (int i = 0; i < displayEntry.details.Length; i++)
                {
                    s = displayEntry.details[i];
                    GUI.Label(r, s, g);
                    r.x += 0.125f * w;
                }
            }
            r.y += 0.0325f * h;
        }

        // NAVIGATION BUTTONS (up and down)
        r.x = 0.8125f * w;
        r.y = 0.4f * h;
        r.width = 0.05f * w;
        r.height = r.width; // square
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(14f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleCenter;
        if (!Application.isEditor)
        {
            g.normal.background = buttonTex[0];
            g.hover.background = buttonTex[1];
            g.active.background = buttonTex[2];
        }
        s = "UP";
        GUI.enabled = !(startingEntry[currentCategory] == currentEntry);
        if (GUI.Button(r,s,g))
        {
            currentEntry--;
        }
        r.y += r.width + (0.05f * h);
        g.alignment = TextAnchor.MiddleCenter;
        s = "DOWN";
        GUI.enabled = !(startingEntry[currentCategory] + entriesInCategory[currentCategory] - 1 == currentEntry);
        if (GUI.Button(r, s, g))
        {
            currentEntry++;
        }
        GUI.enabled = true;

        currentEntry = Mathf.Clamp(currentEntry, 
            startingEntry[currentCategory], 
            startingEntry[currentCategory] + entriesInCategory[currentCategory]-1);

        // NAV LABEL
        r.x = 0.3f * w;
        r.y = 0.755f * h;
        r.width = 0.4f * w;
        r.height = 0.1f * h;
        g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;
        g.alignment = TextAnchor.MiddleCenter;
        GUI.color = Color.white;
        s = "Almanac entries " + (currentEntry + 1 - startingEntry[currentCategory]) + "-" + (currentEntry + 1 - startingEntry[currentCategory] + ENTRIESPERPAGE) + " of " + (entriesInCategory[currentCategory] + ENTRIESPERPAGE);
        GUI.Label(r, s, g);
    }
}
