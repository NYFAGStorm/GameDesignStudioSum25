using UnityEngine;

public class IslandUpgradeMenu : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the island upgrade menu

    public enum  UpgradeType
    {
        TeleportNode,
        Mailbox,
        CompostBin,
        BushA,
        BushB,
        BushC,
        RockA,
        RockB,
        RockC,
        LampPostA,
        LampPostB,
        BannerA,
        BannerB
    }

    [System.Serializable]
    public struct MenuItem
    {
        public string name;
        public UpgradeType type;
        public string description;
        public int price;
        public Texture2D icon;
        public GameObject prefab;
        public float itemPadding;
    }
    public MenuItem[] items;

    private PlayerControlManager pcm;
    private GreenerGameManager ggm;
    private IslandManager im;

    private bool cursorMode;
    private GameObject cursor;
    private float cursorSpeed = 1f;
    private Vector3 cursorMove;
    private bool gridLockCursor;

    private Vector3 islandCenter;
    private float islandRange = 7f;

    private GameObject currentConfigObject;
    private Color currentObjectColor;
    private float currentObjectPadding; // amount effecting bounds

    private bool configPulse;
    private float configObjectPulse;

    private bool validConfig;
    private string configFeedback;
    private float feedbackTimer;

    private MultiGamepad padMgr;
    private bool usingPad;
    private int padButtonSelection = -1;
    private int padMaxButton = 0;
    private int padClickButton = -1;
    private int padMove = -1;

    private AudioManager sfxAudio;

    private SalesVisitManager salesVisit;
    private bool menuOpen;

    private bool confirmAccepted; // TODO: do something with this
    private bool confirmPopup;
    private string popupMessage;
    private float popupTimer;
    private bool popUpMoveDown;

    const float FEEDBACKTIME = 3f;
    const float PULSETIME = 2f;
    const float POPTIME = 1f;


    void Start()
    {
        // validate
        pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        if (pcm == null)
        {
            Debug.LogError("--- IslandUpgradeMenu [Start] : no player control manager found in scene. aborting.");
            enabled = false;
        }
        ggm = GameObject.FindFirstObjectByType<GreenerGameManager>();
        if (ggm == null)
        {
            Debug.LogError("--- IslandUpgradeMenu [Start] : no game manager manager found in scene. aborting.");
            enabled = false;
        }
        im = GameObject.FindFirstObjectByType<IslandManager>();
        if (im == null)
        {
            Debug.LogError("--- IslandUpgradeMenu [Start] : no island manager manager found in scene. aborting.");
            enabled = false;
        }
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
        {
            Debug.LogError("--- IslandUpgradeMenu [Start] : no multigamepad manager manager found in scene. aborting.");
            enabled = false;
        }
        GameObject sfxObj = GameObject.Find("AudioMgr SFX");
        if (sfxObj != null)
            sfxAudio = sfxObj.GetComponent<AudioManager>();
        // initialize
        if (enabled)
        {
            cursor = GameObject.Instantiate((GameObject)Resources.Load("Cast Cursor"));
            SetCursorVisible(false);

            validConfig = true; // begin with valid island configuration

            ConfigureMenuItems();
        }
        else if (salesVisit != null)
            salesVisit.IslandMenuClosed(); // abort menu operation, free the salesman
    }

    void ConfigureMenuItems()
    {
        if (items == null || items.Length == 0)
        {
            Debug.LogWarning("--- IslandUpgradeMenu [ConfigureMenuItemDescriptions] : no items. will ignore.");
            return;
        }

        for (int i =0; i < items.Length; i++)
        {
            switch (items[i].type)
            {
                case UpgradeType.TeleportNode:
                    items[i].name = "Teleporter";
                    items[i].description = "Everyone needs a teleport node ... literally a must have.";
                    items[i].price = 0;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.Mailbox:
                    items[i].name = "Mail Box";
                    items[i].description = "Everyone needs a mailbox ... literally a must have.";
                    items[i].price = 0;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.CompostBin:
                    items[i].name = "Compost Bin";
                    items[i].description = "A convenience for every farm. Simple, elegant. So useful! A compost bin is the best friend a Biomancer has on the farm.";
                    items[i].price = 1;
                    items[i].itemPadding = 1f;
                    break;
                case UpgradeType.BushA:
                    items[i].name = "Bush (Style A)";
                    items[i].description = "Bushy! When you need a bush on your farm, look no further! It has everything you want in a bush, and it is maintenance free!";
                    items[i].price = 100;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.BushB:
                    items[i].name = "Bush (Style B)";
                    items[i].description = "An alternate bush. Sometimes the standard bush just isn't right. That's when you turn to this; a standout bush.";
                    items[i].price = 100;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.BushC:
                    items[i].name = "Bush (Style C)";
                    items[i].description = "A different kind of bush. This bush sets itself apart from all others. It definitely is distinctive in it's own way.";
                    items[i].price = 100;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.RockA:
                    items[i].name = "Rock (Style A)";
                    items[i].description = "Rock of Ages; truly a classic rock. Very solid. Guaranteed to hold up under any weather. This rock will never let you down.";
                    items[i].price = 200;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.RockB:
                    items[i].name = "Rock (Style B)";
                    items[i].description = "Rocks come and go, but this one is here to stay. Owning this rock is owning a piece of history; literally age old!";
                    items[i].price = 200;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.RockC:
                    items[i].name = "Rock (Style C)";
                    items[i].description = "A rock without the roll. A no-nonsense rock when you just need a rock you can count on to be there - every time.";
                    items[i].price = 200;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.LampPostA:
                    items[i].name = "Lamp Post (Style A)";
                    items[i].description = "A fancy lamp post, for the discerning Biomancer who fancies themselves cultured and modern.";
                    items[i].price = 1000;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.LampPostB:
                    items[i].name = "Lamp Post (Style B)";
                    items[i].description = "A modest lamp post with purpose. A lamp post that says, 'I can be a light for all and not be fancy. Watch me.'";
                    items[i].price = 1000;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.BannerA:
                    items[i].name = "Banner (Style A)";
                    items[i].description = "Declare your island clearly and proudly with this broad banner. It automagically conforms to your personal colors!";
                    items[i].price = 2500;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.BannerB:
                    items[i].name = "Banner (Style B)";
                    items[i].description = "A sturdy banner that displays your personal colors automagically! Tell all who see this banner whose farm this is.";
                    items[i].price = 2500;
                    items[i].itemPadding = 0.5f;
                    break;
            }
        }
    }

    public void ConfigSalesVisit( SalesVisitManager sales )
    {
        salesVisit = sales;
        menuOpen = true;
    }

    void SignalMenuClose()
    {
        menuOpen = false;
        salesVisit.IslandMenuClosed();
        Destroy(gameObject, 1f);
    }

    void SetCursorVisible( bool visible )
    {
        cursor.GetComponent<Renderer>().enabled = visible;
        cursor.GetComponentInChildren<Renderer>().enabled = visible;
    }

    void MoveCursor()
    {
        Vector3 pos = cursor.transform.position;
        if (gridLockCursor)
        {
            pos += cursorMove.normalized;
            cursorMove = Vector3.zero;
        }
        else
            pos += cursorMove * cursorSpeed * Time.deltaTime;
        cursor.transform.position = pos;

        // cursor bounds (island center and range)
        float dist = Vector3.Distance(cursor.transform.position, islandCenter);
        // account for 'padding' of current object
        if (dist + currentObjectPadding > islandRange)
        {
            pos += (islandCenter - cursor.transform.position) *
                ((dist + currentObjectPadding) - islandRange);
            cursor.transform.position = pos;
        }
    }

    void ConfigFeedback( string feedbackString )
    {
        configFeedback = feedbackString;
        feedbackTimer = FEEDBACKTIME;
    }

    void ConfirmPopup( string message )
    {
        confirmAccepted = false;
        popupMessage = message;
        popUpMoveDown = false;
        popupTimer = POPTIME;
        confirmPopup = true;
        if (usingPad)
        {
            padMaxButton = 1;
            padButtonSelection = -1;
        }
    }

    void SetCurrentConfigObject( bool set, GameObject obj, float objectPadding )
    {
        // validate
        if (currentConfigObject != null)
        {
            Debug.LogWarning("--- IslandUpgradeMenu [SetCurrentConfigObject] : already current config object set. will ignore.");
            return;
        }
        if (obj == null)
        {
            Debug.LogWarning("--- IslandUpgradeMenu [SetCurrentConfigObject] : no object given to set. will ignore.");
            return;
        }
        if (objectPadding <= 0f)
        {
            Debug.LogWarning("--- IslandUpgradeMenu [SetCurrentConfigObject] : object padding is "+objectPadding+". will ignore.");
            return;
        }

        currentConfigObject = obj;
        Renderer r = currentConfigObject.GetComponentInChildren<Renderer>();
        if (r != null)
            currentObjectColor = r.material.color;
        currentObjectPadding = objectPadding;
    }

    void ReleaseCurrentConfigObject()
    {
        if (currentConfigObject == null)
        {
            Debug.LogWarning("--- IslandUpgradeMenu [ReleaseCurrentConfigObject] : no object found to release. will ignore.");
            return;
        }

        Renderer r = currentConfigObject.GetComponentInChildren<Renderer>();
        if (r != null)
            r.material.color = currentObjectColor;
        currentConfigObject = null;
        currentObjectColor = Color.white;
        currentObjectPadding = 0f;
    }

    void Update()
    {
        // run timers
        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer < 0f)
            {
                feedbackTimer = 0f;
                configFeedback = "";
            }
        }
        if (popupTimer > 0f)
        {
            popupTimer -= Time.deltaTime;
            if (popupTimer < 0f)
            {
                popupTimer = 0f;
                if (!popUpMoveDown)
                    popUpMoveDown = true;
                else
                    confirmPopup = false;
            }
        }

        // run config pulse
        if (configPulse)
        {
            configObjectPulse = Mathf.Sin(Time.time * PULSETIME);
            configObjectPulse += 1f;
            configObjectPulse *= 0.5f;
            // handle various config object renderers
            if (currentConfigObject != null)
            {
                Renderer r = currentConfigObject.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    Color c = Color.white;
                    c *= configObjectPulse;
                    r.material.color = c;
                }
            }
        }
        else
            configObjectPulse = 1f;

        // cursor move
        if (cursorMode)
        {
            if (!cursor.GetComponent<Renderer>().enabled)
                SetCursorVisible(true);
            MoveCursor();
            if (currentConfigObject != null)
                currentConfigObject.transform.position = cursor.transform.position;
        }
        else if (cursor.GetComponent<Renderer>().enabled)
            SetCursorVisible(false);

        // gamepad control
        usingPad = false;
        if (padMgr != null && padMgr.gamepads[0].isActive)
        {
            usingPad = true;   
            padClickButton = -1;
            padMove = -1;
            // selection movement
            if (padMgr.gPadDown[0].YaxisL > 0f)
                padMove = 0; // up
            if (padMgr.gPadDown[0].YaxisL < 0f)
                padMove = 1; // down
            if (padMgr.gPadDown[0].XaxisL < 0f)
                padMove = 2; // left
            if (padMgr.gPadDown[0].XaxisL > 0f)
                padMove = 3; // right
            // cursor mode
            if (cursorMode)
            {
                cursorMove.x = padMgr.gPadDown[0].XaxisL;
                cursorMove.z = padMgr.gPadDown[0].YaxisL;
            }
            // consume input (for some reason)
            padMgr.gPadDown[0].YaxisL = 0f;
            padMgr.gPadDown[0].XaxisL = 0f;
            // general button navigation
            if (!cursorMode)
            {
                if (padMove == 0)
                    padButtonSelection--;
                if (padMove == 1)
                    padButtonSelection++;
                padButtonSelection = Mathf.Clamp(padButtonSelection, 0, padMaxButton);
            }
            // button press
            if (padMgr.gPadDown[0].aButton)
            {
                padClickButton = padButtonSelection;
                // consume input (for some reason)
                padMgr.gPadDown[0].aButton = false;
            }
        }

        // keyboard controls
        int keyMove = -1;
        if (gridLockCursor)
        {
            if (Input.GetKeyDown(pcm.upKey))
                keyMove = 0; // up
            if (Input.GetKeyDown(pcm.downKey))
                keyMove = 1; // down
            if (Input.GetKeyDown(pcm.leftKey))
                keyMove = 2; // left
            if (Input.GetKeyDown(pcm.rightKey))
                keyMove = 3; // right
        }
        else
        {
            if (Input.GetKey(pcm.upKey))
                keyMove = 0; // up
            if (Input.GetKey(pcm.downKey))
                keyMove = 1; // down
            if (Input.GetKey(pcm.leftKey))
                keyMove = 2; // left
            if (Input.GetKey(pcm.rightKey))
                keyMove = 3; // right
        }            
        // cursor mode
        if (cursorMode)
        {
            if (keyMove == 0)
                cursorMove.z = 1f;
            if (keyMove == 1)
                cursorMove.z = -1f;
            if (keyMove == 2)
                cursorMove.x = -1f;
            if (keyMove == 3)
                cursorMove.x = 1f;
        }
    }

    void OnGUI()
    {
        if (!menuOpen)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        r.x = 0.05f * w;
        r.y = 0.1f * h;
        r.width = 0.9f * w;
        r.height = 0.875f * h;

        GUIStyle g = new GUIStyle(GUI.skin.box);
        g.padding = new RectOffset(0, 0, 30, 0);
        g.fontSize = Mathf.RoundToInt(24 * (w/1024f));
        g.fontStyle = FontStyle.Bold;
        //g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;

        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.white;
        string s = "";

        // menu box
        s = "ISLAND UPGRADES";
        GUI.Box(r, s, g);

        GUI.enabled = !confirmPopup;

        //

        GUI.enabled = true;
        // confirm popup
        if (confirmPopup)
        {
            // box message
            r.x = 0.3f * w;
            r.y = 0.35f * h;
            r.width = 0.4f * w;
            r.height = 0.3f * h;
            g = new GUIStyle(GUI.skin.box);
            g.padding = new RectOffset(0, 0, 30, 0);
            g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            s = popupMessage;
            GUI.Box(r, s, g);
            // accept button
            r.x = 0.35f * w;
            r.y = 0.35f * h;
            r.width = 0.1f * w;
            r.height = 0.3f * h;
            g = new GUIStyle(GUI.skin.box);
            g.padding = new RectOffset(0, 0, 30, 0);
            g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.normal.textColor = Color.white;
            if (usingPad && padClickButton == 0)
                g.normal.textColor = Color.yellow;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            s = "ACCEPT";
            if (GUI.Button(r,s,g) ||
                (usingPad && padClickButton == 0))
            {
                confirmAccepted = true;
                popupTimer = POPTIME;
            }
            // cancel button
            r.x = 0.55f * w;
            g.normal.textColor = Color.white;
            if (usingPad && padClickButton == 1)
                g.normal.textColor = Color.yellow;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            s = "CANCEL";
            if (GUI.Button(r, s, g) ||
                (usingPad && padClickButton == 1))
            {
                popupTimer = POPTIME;
            }
        }

        GUI.enabled = true;
        // config feedback label
        if (feedbackTimer > 0f)
        {
            r.x = 0.05f * w;
            r.y = 0.85f * h;
            r.width = 0.9f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
            g.fontStyle = FontStyle.BoldAndItalic;
            g.alignment = TextAnchor.MiddleCenter;
            g.wordWrap = true;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            s = configFeedback;
        }

        GUI.enabled = !confirmPopup;
        // close menu button
        r.x = 0.4f * w;
        r.y = 0.9f * h;
        r.width = 0.2f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        //g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = Color.white;
        if (usingPad && padButtonSelection == padMaxButton)
            g.normal.textColor = Color.yellow;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;
        s = "End Island Upgrades";
        if (validConfig && (GUI.Button(r, s, g) || 
            (usingPad && padClickButton == padMaxButton)))
        {
            SignalMenuClose();
        }
    }
}
