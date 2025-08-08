using UnityEngine;

public class IslandUpgradeMenu : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the island upgrade menu

    public enum UpgradeCategory
    {
        Services,
        Islands,
        Farms,
        Towers,
        OutdoorProps,
        IndoorProps
    }

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
        public UpgradeCategory category;
        public UpgradeType type;
        public string description;
        public int price;
        public Texture2D icon;
        public GameObject prefab;
        public float itemPadding;
    }
    public MenuItem[] items;

    public bool test;

    private PlayerControlManager pcm;
    private GreenerGameManager ggm;
    private IslandManager im;

    private bool hasSalesmanDiscount;

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

    private UpgradeCategory currentCategory;
    private MenuItem[] displayItems;
    private int menuItemSelection = -1;
    private int topOfMenuList = 0;
    private int maxDisplayItems = 5;

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
        if (items == null || items.Length == 0)
        {
            Debug.LogError("--- IslandUpgradeMenu [Start] : no items configured on this tool (need icons and prefabs). aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            CreateCursor();

            validConfig = true; // begin with valid island configuration

            ConfigureMenuItems();

            // lock down player
            pcm.characterFrozen = true;
            pcm.freezeCharacterActions = true;
            // detect salesman discount
            hasSalesmanDiscount = PlayerSystem.PlayerHasEffect(pcm.playerData, PlayerEffect.SkillFriendsSalesman);

            // temp
            currentCategory = UpgradeCategory.OutdoorProps;
            displayItems = GetDisplayItems(currentCategory);
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
            if (items[i].icon == null)
                Debug.LogWarning("--- IslandUpgradeMenu [ConfigureMenuItemDescriptions] : item #" + i + " is missing an icon. will ignore.");
            if (items[i].prefab == null)
                Debug.LogWarning("--- IslandUpgradeMenu [ConfigureMenuItemDescriptions] : item #" + i + " is missing a prefab. will ignore.");

            switch (items[i].type)
            {
                case UpgradeType.TeleportNode:
                    items[i].name = "Teleporter";
                    items[i].description = "Everyone needs a teleport node ... literally a must have.";
                    items[i].price = 0;
                    items[i].itemPadding = 1f;
                    break;
                case UpgradeType.Mailbox:
                    items[i].name = "Mail Box";
                    items[i].description = "Everyone needs a mailbox ... literally a must have.";
                    items[i].price = 0;
                    items[i].itemPadding = 1f;
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
                    items[i].description = "Rocks come and go, but this one is here to stay. Owning this rock is owning a piece of history; literally very old!";
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

    MenuItem[] GetDisplayItems( UpgradeCategory category )
    {
        // find number of items
        int num = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].category == category)
                num++;
        }
        MenuItem[] retItems = new MenuItem[num];

        int count = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].category == category)
            {
                retItems[count] = items[i];
                count++;
            }
        }

        topOfMenuList = 0;

        return retItems;
    }

    public void ConfigSalesVisit( SalesVisitManager sales )
    {
        salesVisit = sales;
        menuOpen = true;
    }

    void SignalMenuClose()
    {
        // remove cursor
        Destroy(cursor);
        // release player
        pcm.characterFrozen = false;
        pcm.freezeCharacterActions = false;
        // close menu
        menuOpen = false;
        salesVisit.IslandMenuClosed();
        // remove this tool
        Destroy(gameObject, 1f);
    }

    void CreateCursor()
    {
        cursor = GameObject.Instantiate((GameObject)Resources.Load("Cast Cursor"));
        cursor.name = "Island Upgrade Cursor";
        SetCursorVisible(false);
    }

    void SetCursorVisible( bool visible )
    {
        cursor.GetComponent<Renderer>().enabled = visible;
        cursor.transform.GetChild(0).GetComponent<Renderer>().enabled = visible;
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

    bool CheckValidCursorLocation()
    {
        bool retBool = true;

        // check if overlapping critical elements (structures or other props)

        return retBool;
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
        else if (cursor != null &&
            cursor.GetComponent<Renderer>().enabled)
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
        if (gridLockCursor || !cursorMode)
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
        else
        {
            if (keyMove == 0)
                menuItemSelection--;
            if (keyMove == 1)
                menuItemSelection++;
            if (menuItemSelection < 0)
                menuItemSelection = 0;
            if (menuItemSelection > displayItems.Length-1)
                menuItemSelection = displayItems.Length-1;
        }

        // configure top of menu
        if (menuItemSelection > topOfMenuList + maxDisplayItems - 1)
            topOfMenuList = menuItemSelection - maxDisplayItems + 1;
        if (menuItemSelection < topOfMenuList)
            topOfMenuList = menuItemSelection;
    }



    bool CanPurchase( MenuItem item )
    {
        bool retBool = false;

        int thisPrice = item.price;
        if (hasSalesmanDiscount)
            thisPrice = Mathf.RoundToInt(thisPrice * 0.75f);
        retBool = (pcm.playerData.gold >= thisPrice);

        return retBool;
    }

    void MakePurchase( MenuItem item )
    {
        int thisPrice = item.price;
        if (hasSalesmanDiscount)
            thisPrice = Mathf.RoundToInt(thisPrice * 0.75f);
        pcm.playerData.gold -= thisPrice;
    }

    string FormCategoryLabel( UpgradeCategory category )
    {
        string retString = "";

        switch (category)
        {
            case UpgradeCategory.Services:
                retString = "Services";
                break;
            case UpgradeCategory.Islands:
                retString = "Islands";
                break;
            case UpgradeCategory.Farms:
                retString = "Farms";
                break;
            case UpgradeCategory.Towers:
                retString = "Towers";
                break;
            case UpgradeCategory.OutdoorProps:
                retString = "Outdoor Props";
                break;
            case UpgradeCategory.IndoorProps:
                retString = "Indoor Props";
                break;
        }

        return retString;
    }

    void OnGUI()
    {
        if (!menuOpen)
            return;

        if (test)
        {
            test = false;
            salesVisit.MenuDialogBeat("We are defintely UP selling!");
        }

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

        // category label
        r.x = 0.4f * w;
        r.y = 0.175f * h;
        r.width = 0.2f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.label);
        g.alignment = TextAnchor.MiddleCenter;
        g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        g.fontStyle = FontStyle.BoldAndItalic;
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;
        s = FormCategoryLabel(currentCategory);
        GUI.Label(r, s, g);
        // category nav buttons
        r.x = 0.2f * w;
        r.y = 0.175f * h;
        r.width = 0.1f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.button);
        if (usingPad)
            g.fontSize = Mathf.RoundToInt(12 * (w / 1024f));
        g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.yellow;
        s = "<<";
        if (usingPad)
            s += "\nL Bump";
        if (GUI.Button(r,s,g) || 
            (usingPad && padMgr.gPadDown[0].LBump))
        {
            currentCategory--;
            if (currentCategory < UpgradeCategory.Services)
                currentCategory = UpgradeCategory.IndoorProps;
            displayItems = GetDisplayItems(currentCategory);

            // consuming input, but why?
            if (usingPad)
                padMgr.gPadDown[0].LBump = false;
        }
        r.x = 0.7f * w;
        s = ">>";
        if (usingPad)
            s += "\nR Bump";
        if (GUI.Button(r, s, g) ||
            (usingPad && padMgr.gPadDown[0].RBump))
        {
            currentCategory++;
            if (currentCategory > UpgradeCategory.IndoorProps)
                currentCategory = UpgradeCategory.Services;
            displayItems = GetDisplayItems(currentCategory);

            // consuming input, but why?
            if (usingPad)
                padMgr.gPadDown[0].RBump = false;
        }

        GUI.enabled = !confirmPopup;

        // menu item list
        r.y = 0.25f * h;
        for (int i = topOfMenuList; i < Mathf.Min((topOfMenuList+maxDisplayItems),displayItems.Length); i++)
        {
            if (displayItems == null || displayItems.Length == 0 ||
                i < 0 || i > displayItems.Length)
                continue;

            // icon
            r.x = 0.1f * w;
            r.width = 0.05f * w;
            r.height = r.width; // square
            t = displayItems[i].icon;
            c = Color.white;
            GUI.color = c;
            GUI.DrawTexture(r, t);
            // name
            r.x = 0.175f * w;
            r.width = 0.2f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleLeft;
            g.fontSize = Mathf.RoundToInt(18 * (w/1024f));
            g.fontStyle = FontStyle.Bold;
            s = displayItems[i].name;
            r.x += 0.0008f * w;
            r.y += 0.001f * w;
            g.normal.textColor = Color.black;
            g.hover.textColor = Color.black;
            g.active.textColor = Color.black;
            GUI.Label(r, s, g);
            r.x -= 0.0016f * w;
            r.y -= 0.002f * w;
            g.normal.textColor = Color.white;
            if (menuItemSelection == (i + 0) || 
                (usingPad && padButtonSelection == (i + 0)))
                g.normal.textColor = Color.yellow;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            GUI.Label(r, s, g);
            r.x += 0.0008f * w;
            r.y += 0.001f * w;
            // price
            r.x = 0.7f * w;
            r.width = 0.1f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleRight;
            g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            int thisPrice = displayItems[i].price;
            if (hasSalesmanDiscount)
                thisPrice = Mathf.RoundToInt(thisPrice * 0.75f);
            s = thisPrice.ToString();
            r.x += 0.0008f * w;
            r.y += 0.001f * w;
            g.normal.textColor = Color.black;
            g.hover.textColor = Color.black;
            g.active.textColor = Color.black;
            GUI.Label(r, s, g);
            r.x -= 0.0016f * w;
            r.y -= 0.002f * w;
            g.normal.textColor = Color.white;
            if (menuItemSelection == (i + 0) ||
                (usingPad && padButtonSelection == (i + 0)))
                g.normal.textColor = Color.yellow;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            GUI.Label(r, s, g);
            r.x += 0.0008f * w;
            r.y += 0.001f * w;
            // description
            r.x = 0.2f * w;
            r.y += 0.025f * h;
            r.width = 0.6f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleLeft;
            g.wordWrap = true;
            g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
            g.fontStyle = FontStyle.Italic;
            if (menuItemSelection == (i + 0) ||
                (usingPad && padButtonSelection == (i + 0)))
                g.fontStyle = FontStyle.BoldAndItalic;
            s = displayItems[i].description;
            r.x += 0.0008f * w;
            r.y += 0.001f * w;
            g.normal.textColor = Color.black;
            g.hover.textColor = Color.black;
            g.active.textColor = Color.black;
            GUI.Label(r, s, g);
            r.x -= 0.0016f * w;
            r.y -= 0.002f * w;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            GUI.Label(r, s, g);
            r.x += 0.0008f * w;
            r.y += 0.001f * w;

            r.y += 0.1f * h;
        }

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
