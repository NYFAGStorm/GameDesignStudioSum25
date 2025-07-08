using UnityEngine;

public class CheatManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles cheat codes and non-player controls

    // INSTRUCTIONS:
    // to add new cheat codes, first add to the const TOTALCHEATCODES
    // than append the list of cheat codes in GetCodeInfo()
    // finally, append to PerformValidCode() with function calls
    // (before leaving, confirm these three elements match)

    [System.Serializable]
    public struct CheatCode
    {
        public string name;
        public string description;
    }
    public CheatCode[] codes;

    private bool debugLogCodes;
    private string currentCode;
    private float codeTimer;
    private int validCode;

    private GreenerGameManager ggMgr;

    private float cheatDisplayTimer;
    private string cheatDisplayString;

    const int TOTALCHEATCODES = 16;
    const float CHEATCODEWINDOW = 1f;
    const float CHEATDISPLAYTIME = 20f;


    void Start()
    {
        // validate
        if ( codes != null && codes.Length > 0 )
            Debug.LogWarning("--- CheatManager [Start] : cheat code data detected in "+gameObject.name+", but data set via script. will ignore.");
        ggMgr = GameObject.FindFirstObjectByType<GreenerGameManager>();
        if (ggMgr == null)
        {
            Debug.LogError("--- CheatManager [Start] : no greener game manager found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            ResetCurrentCode();
            validCode = -1;
            InitCodes();
        }
    }

    void InitCodes()
    {
        int numCodes = TOTALCHEATCODES;

        codes = new CheatCode[numCodes];
        for (int i=0; i<numCodes; i++)
        {
            codes[i] = new CheatCode();
            string n, d;
            GetCodeInfo(i, out n, out d);
            codes[i].name = n;
            codes[i].description = d;
        }
    }

    void Update()
    {
        // run cheat display timer
        if (cheatDisplayTimer > 0f)
        {
            cheatDisplayTimer -= Time.deltaTime;
            if (cheatDisplayTimer < 0f)
                cheatDisplayTimer = 0f;
        }
        
        // detect keyboard input
        if (Input.anyKeyDown)
        {
            if ( Input.GetKeyDown(KeyCode.Backspace) )
            {
                ResetCurrentCode();
                if (debugLogCodes)
                    Debug.Log("--- CheatManager [Update] : input cleared.");
                return;
            }

            currentCode += Input.inputString.ToLower();
            codeTimer = CHEATCODEWINDOW;

            if (debugLogCodes)
                Debug.Log("--- CheatManager [Update] : current input is '" + currentCode + "'.");
        }

        // run cheat code timer
        if (codeTimer > 0f)
        {
            codeTimer -= Time.deltaTime;
            if (codeTimer < 0f)
            {
                ResetCurrentCode();
                validCode = -1; // initialized
                if (debugLogCodes)
                    Debug.Log("--- CheatManager [Update] : input cleared.");
            }
        }

        if (codeTimer == 0f)
            return;

        // detect valid cheat code
        DetectCode();

        if (validCode == -1)
            return;

        // interrupt if cheats not allowed in this game
        if ((validCode != 12 || !ggMgr.IsHostGame()) && !ggMgr.game.options.allowCheats)
        {
            ggMgr.AddNotification("cheat codes not allowed in this game.");
            validCode = -1; // initialized
            return;
        }

        // perform valid cheat code
        PerformValidCode();
    }

    void ResetCurrentCode()
    {
        currentCode = "";
        codeTimer = 0f;
    }

    void DetectCode()
    {
        validCode = -1;
        for (int i=0; i<codes.Length; i++)
        {
            if (currentCode == codes[i].name)
            {
                validCode = i;
                Debug.Log("--- CheatManager [DetectCode] : cheat code '" + codes[i].name +"' ["+validCode+"] detected.");
                ggMgr.AddNotification("cheat code '" + codes[i].name + "' detected.");
                ResetCurrentCode();
                break;
            }
        }
    }

    void GetCodeInfo(int codeIndex, out string n, out string d)
    {
        // cheat code list
        switch (codeIndex)
        {
            case 0:
                n = "logcheat";
                d = "toggles the log display of current cheat code as typed";
                break;
            case 1:
                n = "timereset";
                d = "Resets time rate to 100% (default time rate)";
                break;
            case 2:
                n = "timesten";
                d = "Sets time rate to 10x default";
                break;
            case 3:
                n = "timeshundred";
                d = "Sets time rate to 100x default";
                break;
            case 4:
                n = "timesthousand";
                d = "Sets time rate to 1000x default";
                break;
            case 5:
                n = "playerlight";
                d = "Toggles a light over the player";
                break;
            case 6:
                n = "gimmiepoo";
                d = "Drops fertilizer in front of the player";
                break;
            case 7:
                n = "gimmieseed";
                d = "Drops corn seed in front of the player";
                break;
            case 8:
                n = "gimmiegold";
                d = "Gives 10 gold to the player";
                break;
            case 9:
                n = "gimmiespells";
                d = "Enters two spells in the player's grimoire";
                break;
            case 10:
                n = "discosky";
                d = "Toggles a very colorful sky display";
                break;
            case 11:
                n = "bumpmaxplayers";
                d = "Adds one to the max players of this game (up to 8)";
                break;
            case 12:
                n = "togglecheats";
                d = "Toggles the setting to allow cheats in this game";
                break;
            case 13:
                n = "mediumrare";
                d = "Drops potentially rare seed in front of the player";
                break;
            case 14:
                n = "listcheats";
                d = "Temporarily displays a list of cheat codes";
                break;
            case 15:
                n = "revealalmanac";
                d = "Reveals all entries in the Biomancer's Almanac";
                break;
            default:
                n = "-";
                d = "--";
                break;
        }
    }

    void PerformValidCode()
    {
        //Debug.Log("--- CheatManager [PerformValidCode] : performing cheat code "+validCode+" '"+codes[validCode].name+"'.");

        ItemSpawnManager ism;

        // time bump codes
        switch (validCode)
        {
            case 0:
                debugLogCodes = !debugLogCodes;
                break;
            case 1:
                GameObject.FindAnyObjectByType<TimeManager>().SetCheatTimeScale(1f);
                break;
            case 2:
                GameObject.FindAnyObjectByType<TimeManager>().SetCheatTimeScale(10f);
                break;
            case 3:
                GameObject.FindAnyObjectByType<TimeManager>().SetCheatTimeScale(100f);
                break;
            case 4:
                GameObject.FindAnyObjectByType<TimeManager>().SetCheatTimeScale(1000f);
                break;
            case 5:
                if (GameObject.Find("player light") == null)
                {
                    GameObject player = GameObject.FindFirstObjectByType<PlayerControlManager>().gameObject;
                    GameObject light = new GameObject();
                    light.name = "player light";
                    light.transform.position = player.transform.position + Vector3.up;
                    light.transform.parent = player.transform;
                    Light l = light.AddComponent<Light>();
                    l.range = 3.81f;
                    l.intensity = .618f;
                    l.bounceIntensity = 0f;
                    l.shadows = LightShadows.Hard;
                }
                else
                    Destroy(GameObject.Find("player light").gameObject);
                break;
            case 6:
                // drop fertilizer
                ism = GameObject.FindFirstObjectByType<ItemSpawnManager>();
                if (ism != null)
                {
                    PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
                    float facing = pcm.gameObject.transform.GetChild(0).GetComponent<Renderer>().material.GetTextureScale("_MainTex").x;
                    Vector3 pos = GameObject.FindFirstObjectByType<PlayerControlManager>().gameObject.transform.position;
                    Vector3 targ = pos + (facing * Vector3.right);
                    ism.SpawnNewItem(ItemType.Fertilizer, pos, targ, true);
                }
                break;
            case 7:
                // drop corn seed
                ism = GameObject.FindFirstObjectByType<ItemSpawnManager>();
                if (ism != null)
                {
                    PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
                    float facing = pcm.gameObject.transform.GetChild(0).GetComponent<Renderer>().material.GetTextureScale("_MainTex").x;
                    Vector3 pos = GameObject.FindFirstObjectByType<PlayerControlManager>().gameObject.transform.position;
                    Vector3 targ = pos + (facing*Vector3.right);
                    LooseItemData seed = InventorySystem.CreateItem(ItemType.Seed);
                    seed.inv.items[0].name = "Seed (Corn)";
                    seed.inv.items[0].plant = PlantType.Corn;
                    ism.SpawnItem(seed, pos, targ, true);
                }
                break;
            case 8:
                PlayerControlManager playerControlMgr = GameObject.FindFirstObjectByType<PlayerControlManager>();
                playerControlMgr.playerData.gold += 10;
                break;
            case 9:
                PlayerControlManager plcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
                PlayerData pData = plcm.playerData;
                pData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.FastGrowI, pData.magic.library);
                pData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.SummonWaterI, pData.magic.library);
                pData.magic.library.grimiore[0].name = "Fast Grow I";
                pData.magic.library.grimiore[0].type = SpellType.FastGrowI; // REVIEW: why this not already in?
                pData.magic.library.grimiore[0].description = "Plants grow faster for one day. (5%)";
                pData.magic.library.grimiore[0].ingredients = new ItemType[2];
                pData.magic.library.grimiore[0].ingredients[0] = ItemType.Fertilizer;
                pData.magic.library.grimiore[0].ingredients[1] = ItemType.Stalk;
                pData.magic.library.grimiore[1].name = "Summon Water I";
                pData.magic.library.grimiore[1].description = "Waters a 2x2 area that stays hydrated for one day.";
                pData.magic.library.grimiore[1].ingredients = new ItemType[2];
                pData.magic.library.grimiore[1].ingredients[0] = ItemType.Seed;
                pData.magic.library.grimiore[1].ingredients[1] = ItemType.Fruit;
                break;
            case 10:
                BackgroundManager bm = GameObject.FindFirstObjectByType<BackgroundManager>();
                if (bm != null)
                    bm.DiscoSky();
                break;
            case 11:
                // only allow this cheat if host
                if (ggMgr.IsHostGame())
                    ggMgr.game.options.maxPlayers = Mathf.Clamp(ggMgr.game.options.maxPlayers + 1, 1, 8);
                break;
            case 12:
                // only allow this cheat if host
                // (always allow if host)
                if (ggMgr.IsHostGame())
                    ggMgr.game.options.allowCheats = !ggMgr.game.options.allowCheats;
                break;
            case 13:
                // drop potentially rare seed
                ism = GameObject.FindFirstObjectByType<ItemSpawnManager>();
                if (ism != null)
                {
                    PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
                    float facing = pcm.gameObject.transform.GetChild(0).GetComponent<Renderer>().material.GetTextureScale("_MainTex").x;
                    Vector3 pos = GameObject.FindFirstObjectByType<PlayerControlManager>().gameObject.transform.position;
                    Vector3 targ = pos + (facing * Vector3.right);
                    float maybeRarePlantType = RandomSystem.WeightedRandom01() * 41f;
                    PlantType pt = (PlantType)(((int)maybeRarePlantType+1));
                    PlantData p = PlantSystem.InitializePlant(pt);
                    LooseItemData seed = InventorySystem.CreateItem(ItemType.Seed);
                    seed.inv.items[0] = InventorySystem.SetItemAsPlant(seed.inv.items[0],p);
                    seed.inv.items[0].name = "Seed ("+p.plantName+")";
                    seed.inv.items[0].plant = pt;
                    ism.SpawnItem(seed, pos, targ, true);
                }
                break;
            case 14:
                Debug.Log("CHEAT CODE LIST");
                string s = "";
                for (int i = 0; i < codes.Length; i++)
                {
                    s += codes[i].name + "\n   " + codes[i].description + "\n";
                }
                Debug.Log(s);
                cheatDisplayString = s;
                cheatDisplayTimer = CHEATDISPLAYTIME;
                break;
            case 15:
                // unlock all almanac entries
                InGameAlmanac iga = GameObject.FindFirstObjectByType<InGameAlmanac>();
                if (iga != null)
                {
                    for (int i = 0; i < iga.almanac.entries.Length; i++)
                    {
                        iga.almanac.entries[i].revealed = true;
                    }
                }
                break;
            default:
                Debug.LogWarning("--- CheatManager [PerformValidCode] : code index "+validCode+" undefined. will ignore.");
                break;
        }

        validCode = -1; // initialized
    }

    void OnGUI()
    {
        if (cheatDisplayTimer == 0f)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        r.x = 0.25f * w;
        r.y = 0.2f * h;
        r.width = 0.5f * w;
        r.height = 0.8f * h;

        GUIStyle g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt(12f * (w/1024f));
        g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleLeft;
        // drop shadow
        r.x += 0.001f * w;
        r.y += 0.002f * h;
        Color c = Color.black;
        //c.a = Mathf.Clamp01(cheatDisplayTimer + (CHEATDISPLAYTIME - 1f) / CHEATDISPLAYTIME);
        g.normal.textColor = c;
        g.hover.textColor = c;
        g.active.textColor = c;
        GUI.Label(r, cheatDisplayString, g);
        r.x -= 0.001f * w;
        r.y -= 0.002f * h;
        c = Color.white;
        //c.a = Mathf.Clamp01(cheatDisplayTimer + (CHEATDISPLAYTIME - 1f) / CHEATDISPLAYTIME);
        g.normal.textColor = c;
        g.hover.textColor = c;
        g.active.textColor = c;
        GUI.Label(r, cheatDisplayString, g);
    }
}
