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

    const int TOTALCHEATCODES = 8;
    const float CHEATCODEWINDOW = 1f;


    void Start()
    {
        // validate
        if ( codes != null && codes.Length > 0 )
            Debug.LogWarning("--- CheatManager [Start] : cheat code data detected in "+gameObject.name+", but data set via script. will ignore.");
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
                    GameObject player = GameObject.Find("Player Character");
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
                ism = GameObject.FindFirstObjectByType<ItemSpawnManager>();
                if (ism != null)
                {
                    PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
                    float facing = pcm.gameObject.transform.GetChild(0).GetComponent<Renderer>().material.GetTextureScale("_MainTex").x;
                    Vector3 pos = GameObject.FindFirstObjectByType<PlayerControlManager>().gameObject.transform.position;
                    Vector3 targ = pos + (facing * Vector3.right);
                    ism.SpawnNewItem(ItemType.Fertilizer, pos, targ);
                }
                break;
            case 7:
                ism = GameObject.FindFirstObjectByType<ItemSpawnManager>();
                if (ism != null)
                {
                    PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
                    float facing = pcm.gameObject.transform.GetChild(0).GetComponent<Renderer>().material.GetTextureScale("_MainTex").x;
                    Vector3 pos = GameObject.FindFirstObjectByType<PlayerControlManager>().gameObject.transform.position;
                    Vector3 targ = pos + (facing*Vector3.right);
                    LooseItemData seed = InventorySystem.CreateItem(ItemType.Seed);
                    seed.inv.items[0].name = "Seed (Corn)";
                    seed.inv.items[0].plantIndex = 1;
                    ism.SpawnItem(seed, pos, targ);
                }
                break;
            default:
                Debug.LogWarning("--- CheatManager [PerformValidCode] : code index "+validCode+" undefined. will ignore.");
                break;
        }

        validCode = -1; // initialized
    }
}
