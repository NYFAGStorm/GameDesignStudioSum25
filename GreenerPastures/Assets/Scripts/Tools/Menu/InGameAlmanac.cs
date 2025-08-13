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

    private string[] entryTitles; // pre-lorem titles

    private PlayerControlManager pcm;
    private MagicManager mm;
    private MultiGamepad padMgr;
    private QuitOnEscape qoe;
    private InGameControls igc;

    private bool islandUpgrades;

    private AudioManager sfxAudio;

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
        GameObject sfxObj = GameObject.Find("AudioMgr SFX");
        if (sfxObj != null)
            sfxAudio = sfxObj.GetComponent<AudioManager>();
        // initialize
        if (enabled)
        {
            currentCategory = 1;

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

    public void SetIslandUpgrades()
    {
        islandUpgrades = true;
    }

    void InitAlmanac()
    {
        almanac = AlmanacSystem.InitializeAlmanac();
        entryTitles = new string[almanac.entries.Length];
        for (int i = 0; i < almanac.entries.Length; i++)
        {
            entryTitles[i] = almanac.entries[i].title;
        }
        currentCategory = 1;
        currentEntry = 0;
        ConfigureEntryCount();
    }

    void ConfigureEntryCount()
    {
        startingEntry = new int[10];
        entriesInCategory = new int[10];
        for (int i = 0; i < almanac.entries.Length; i++)
        {
            entriesInCategory[(int)almanac.entries[i].category]++;
            if (i > 0 && almanac.entries[i].category != almanac.entries[i - 1].category)
                startingEntry[(int)almanac.entries[i].category] = i;
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
                    ggm.AddNotification(almanac.entries[revealedEntryIndex].category.ToString().ToUpper() + " Almanac entry\nREVEALED '" + almanac.entries[revealedEntryIndex].title +"'");
                revealedEntryIndex = -1;
            }
        }

        if (islandUpgrades)
        {
            IslandUpgradeMenu ium = GameObject.FindFirstObjectByType<IslandUpgradeMenu>();
            if (ium == null)
                islandUpgrades = false;
        }

        if (igc.controlsDisplay || mm.IsDisplayingMagic() || islandUpgrades)
        {
            showAlmanac = false;
            return;
        }

        if ( Input.GetKeyDown(KeyCode.Backslash) ||
            (padMgr != null && padMgr.gamepads[0].isActive && padMgr.gPadDown[0].RTrigger > 0f))
        {
            showAlmanac = !showAlmanac;
            // control player hud
            if (showAlmanac && !pcm.hidePlayerHUD)
                pcm.hidePlayerHUD = true;
            if (!showAlmanac && pcm.hidePlayerHUD)
                pcm.hidePlayerHUD = false;

            // REVIEW: why do I need to consume input on gPadDown?
            if (padMgr != null && padMgr.gamepads[0].isActive)
                padMgr.gPadDown[0].RTrigger = 0f;
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
        mm = pcm.gameObject.GetComponent<MagicManager>();
        updateAlmanac = true;
    }

    int GetAlmanacEntryIndex(string title)
    {
        int retInt = -1;

        for (int i = 0; i < entryTitles.Length; i++)
        {
            if (entryTitles[i] == title)
            {
                retInt = i;
                break;
            }
        }
        if (retInt == -1)
            Debug.LogWarning("--- InGameAlmanac [GetAlmanacEntryIndex] : no entry with title '"+title+"' found. will return -1.");

        return retInt;
    }

    /// <summary>
    /// Returns true if the almanac entry has not yet been revealed
    /// </summary>
    /// <param name="entryTitle">title of almanac entry (pre-lorem)</param>
    /// <returns>true if entry is not revealed, false if revealed</returns>
    public bool IsEntryHidden(string entryTitle)
    {
        bool retBool = false;

        int idx = GetAlmanacEntryIndex(entryTitle);
        if (idx > -1 && !almanac.entries[idx].revealed)
            retBool = true;

        return retBool;
    }

    /// <summary>
    /// Sets an entry in the Biomancer's Almanac to be revealed for the local player
    /// </summary>
    /// <param name="entryTitle">the entry title</param>
    public void AlmanacReveal(string entryTitle)
    {
        int idx = GetAlmanacEntryIndex(entryTitle);
        if (pcm != null && pcm.playerData != null &&
            pcm.playerData.almanac.revealed.Length > 0)
        {
            pcm.playerData.almanac.revealed[idx] = true;
            updateAlmanac = true;
            revealedEntryIndex = idx;
            if (sfxAudio != null)
                sfxAudio.StartSound("Player Almanac Reveal");
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
            if (sfxAudio != null)
                sfxAudio.StartSound("Player Almanac Reveal");
        }
    }

    void OnGUI()
    {
        if (pcm == null || igc.controlsDisplay)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        GUIStyle g = new GUIStyle(GUI.skin.box);

        string s = "";

        if (!showAlmanac)
        {
            if (pcm.hidePlayerHUD)
                return;
            r.x = 0.85f * w;
            r.y = 0.0625f * h;
            r.width = 0.15f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(12f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            if (padMgr != null && padMgr.gamepads[0].isActive)
                s = "BIOMANCER'S\nALMANAC [RT]";
            else
                s = "BIOMANCER'S\nALMANAC [\\]";
            GUI.Label(r, s, g);
            return;
        }

        r.x = 0.1f * w;
        r.y = 0.15f * h;
        r.width = 0.8f * w;
        r.height = 0.7f * h;

        g = new GUIStyle(GUI.skin.box);
        g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.padding = new RectOffset(0, 0, 20, 0);
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;

        s = "BIOMANCER'S ALMANAC";

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

        if (padMgr.gamepads[0].isActive)
        {
            // GAMEPAD CONTROL LABELS
            r.x = 0.125f * w;
            r.y = 0.1625f * h;
            r.width = 0.15f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            g.alignment = TextAnchor.MiddleCenter;
            g.fontSize = Mathf.RoundToInt(12f * (w / 1024f));
            GUI.color = Color.white;
            s = "L Bump / R Bump\nchange categories";
            GUI.Label(r, s, g);
            r.x = .725f * w;
            s = "Right Stick\nUp Down Entry List";
            GUI.Label(r, s, g);
        }

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
            if (i == currentCategory)
                g.normal.textColor = Color.white;
            else
                g.normal.textColor = c;
            s = ((AlmanacCategory)i).ToString().ToUpper();
            if (GUI.Button(r, s, g))
            {
                currentCategory = i;
                currentEntry = startingEntry[currentCategory-1];
            }
            if (padMgr.gamepads[0].isActive)
            {
                bool changed = false;
                if (padMgr.gPadDown[0].LBump)
                {
                    changed = true;
                    currentCategory--;
                }
                if (padMgr.gPadDown[0].RBump)
                {
                    changed = true;
                    currentCategory++;
                }
                if (changed)
                {
                    currentCategory = Mathf.Clamp(currentCategory, 1, 9);
                    currentEntry = startingEntry[currentCategory - 1];
                    padMgr.gPadDown[0].LBump = false;
                    padMgr.gPadDown[0].RBump = false;
                }
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
            if (currentEntry + n >= 
                startingEntry[currentCategory] + entriesInCategory[currentCategory])
                continue;

            AlmanacEntry displayEntry = almanac.entries[currentEntry + n];
            GUI.enabled = pcm.playerData.almanac.revealed[currentEntry + n];

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
            r.height = 0.0425f * w;
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
        GUI.enabled = true;

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
        GUI.enabled = (currentEntry > startingEntry[currentCategory]);
        if (GUI.Button(r,s,g) || padMgr.gamepads[0].isActive && padMgr.gPadDown[0].YaxisR > 0f)
        {
            if (GUI.enabled)
                currentEntry--;

            // consuming input, por qua?
            if (padMgr.gamepads[0].isActive)
                padMgr.gPadDown[0].YaxisR = 0f;
        }

        r.y += r.width + (0.05f * h);
        g.alignment = TextAnchor.MiddleCenter;
        s = "DOWN";
        GUI.enabled = (currentEntry + ENTRIESPERPAGE < startingEntry[currentCategory] + entriesInCategory[currentCategory]);
        if (GUI.Button(r, s, g) || padMgr.gamepads[0].isActive && padMgr.gPadDown[0].YaxisR < 0f)
        {
            if (GUI.enabled)
                currentEntry++;

            // consuming input, por qua?
            if (padMgr.gamepads[0].isActive)
                padMgr.gPadDown[0].YaxisR = 0f;
        }

        GUI.enabled = true;

        currentEntry = Mathf.Clamp(currentEntry, 
            startingEntry[currentCategory], 
            startingEntry[currentCategory] + entriesInCategory[currentCategory]-1);

        // NAV LABEL
        r.x = 0.2f * w;
        r.y = 0.755f * h;
        r.width = 0.6f * w;
        r.height = 0.1f * h;
        g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;
        g.alignment = TextAnchor.MiddleCenter;
        GUI.color = Color.white;
        int entNum = currentEntry - startingEntry[currentCategory] + 1;
        s = "Almanac entries "+entNum+"-"+ Mathf.Min(entNum + ENTRIESPERPAGE - 1, entriesInCategory[currentCategory]) + " of " + entriesInCategory[currentCategory] + " in Category: " + ((AlmanacCategory)currentCategory).ToString();
        GUI.Label(r, s, g);
    }
}
