using UnityEngine;

public class MarketManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the market transactions with players

    public enum CustomerMode
    {
        Default,
        Buy,
        Sell
    }
    
    [System.Serializable]
    public struct MenuItem
    {
        public string itemName;
        public ItemType itemType;
        public int plantIndex;
        public int itemValue;
    }

    public MenuItem[] menuItems;

    private MultiGamepad padMgr;

    private string marketInstructions;
    private float playerCheckTimer;
    private PlayerControlManager currentCustomer;
    private int menuItemSelection = -1;
    private CustomerMode customerMode;
    private float rejectFlashTimer;
    private Vector3 purchaseOffset = new Vector3(-1f,0f,0f);
    // REVIEW: apply a profit margin difference when player sells an item (less value)
    // temp - for now, just minus one gold when selling items to market

    const float PLAYERCHECKTIME = 1f;
    const float MARKETPROXIMITYRANGE = .5f;
    const float REJECTFLASHTIME = 1f;


    void Start()
    {
        // validate
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
            Debug.LogWarning("--- MarketManager [Start] : no multi gamepad found. will ignore.");
        // initialize
        if ( enabled )
        {
            InitializeMenu();
            playerCheckTimer = PLAYERCHECKTIME;
            marketInstructions = "MARKET [Welcome]\nE=BUY F=SELL";
            if (padMgr != null && padMgr.gamepads[0].isActive)
                marketInstructions = "MARKET [Welcome]\nA=BUY B=SELL";
        }
    }

    void Update()
    {
        // run reject flash timer
        if ( rejectFlashTimer > 0f )
        {
            rejectFlashTimer -= Time.deltaTime;
            if (rejectFlashTimer < 0f)
                rejectFlashTimer = 0f;
        }

        // run player check timer
        if ( playerCheckTimer > 0f )
        {
            playerCheckTimer -= Time.deltaTime;
            if ( playerCheckTimer < 0f )
            {
                playerCheckTimer = PLAYERCHECKTIME;

                // allow player to 'enter' and 'exit' (proximity)
                PlayerControlManager[] pcms = GameObject.FindObjectsByType<PlayerControlManager>(FindObjectsSortMode.None);
                int found = -1;
                for ( int i=0; i<pcms.Length; i++ )
                {
                    float dist = Vector3.Distance(gameObject.transform.position, pcms[i].gameObject.transform.position);
                    if ( dist < MARKETPROXIMITYRANGE )
                    {
                        found = i;
                        break;
                    }
                }
                if (found > -1)
                    currentCustomer = pcms[found];
                else
                {
                    if (currentCustomer != null )
                    {
                        currentCustomer.characterFrozen = false;
                        currentCustomer.hidePlayerNameTag = false;
                    }
                    currentCustomer = null;
                }
            }
        }

        if (currentCustomer == null)
            return;

        PlayerControlManager.PlayerActions pa = currentCustomer.GetPlayerActions();

        // allow player to enter buy mode
        CheckBuyMode( pa );

        // allow player to sell inventory item
        CheckSellMode( pa );
    }

    void CheckBuyMode( PlayerControlManager.PlayerActions pa )
    {
        if (customerMode == CustomerMode.Default && pa.actionADown)
        {
            customerMode = CustomerMode.Buy;
            currentCustomer.characterFrozen = true;
            menuItemSelection = 0;
            marketInstructions = "MARKET [BUY MODE]\nE=BUY V=EXIT";
            if (padMgr != null && padMgr.gamepads[0].isActive)
                marketInstructions = "MARKET [BUY MODE]\nA=BUY Y=EXIT";
            return; // consume input, do not allow purchase with current actionA signal
        }
        if (customerMode == CustomerMode.Buy)
        {
            // allow player to select item on menu
            if (Input.GetKeyDown(currentCustomer.upKey) || 
                (padMgr != null && padMgr.gamepads[0].isActive &&
                    padMgr.gPadDown[0].YaxisL > 0f))
            {
                menuItemSelection--;
                rejectFlashTimer = 0f;
            }
            if (Input.GetKeyDown(currentCustomer.downKey) || 
                (padMgr != null && padMgr.gamepads[0].isActive && 
                    padMgr.gPadDown[0].YaxisL < 0f))
            {
                menuItemSelection++;
                rejectFlashTimer = 0f;
            }
            menuItemSelection = Mathf.Clamp(menuItemSelection, 0, menuItems.Length - 1);
            // allow player to buy item
            if (Input.GetKeyDown(currentCustomer.actionAKey) || 
                (padMgr != null && padMgr.gamepads[0].isActive && 
                    padMgr.gPadDown[0].aButton))
            {
                if (menuItems[menuItemSelection].itemValue <= currentCustomer.playerData.gold)
                {
                    currentCustomer.playerData.gold -= menuItems[menuItemSelection].itemValue;
                    // try place in inventory, spawn to the side if fail
                    if (InventorySystem.InvHasSlot(currentCustomer.playerData.inventory))
                    {
                        ItemData iData = InventorySystem.InitializeItem(menuItems[menuItemSelection].itemType);
                        if (iData == null)
                            Debug.LogWarning("--- MarketManager [CheckBuyMode] : unable to initialize item. will ignore.");
                        else
                        {
                            iData.plantIndex = menuItems[menuItemSelection].plantIndex;
                            if (menuItems[menuItemSelection].itemType == ItemType.Seed ||
                                menuItems[menuItemSelection].itemType == ItemType.Fruit)
                            {
                                PlantType p = (PlantType)iData.plantIndex;
                                iData.name += " (" + p.ToString() + ")";
                            }
                        }
                        currentCustomer.playerData.inventory = InventorySystem.AddToInventory(currentCustomer.playerData.inventory, iData);
                    }
                    else
                    {
                        Vector3 pos = gameObject.transform.position;
                        pos += purchaseOffset;
                        Vector3 targ = (currentCustomer.transform.position - gameObject.transform.position) * 4f;
                        targ += pos;
                        ItemSpawnManager ism = GameObject.FindFirstObjectByType<ItemSpawnManager>();
                        LooseItemData loose = InventorySystem.CreateItem(menuItems[menuItemSelection].itemType);
                        loose.inv.items[0].plantIndex = menuItems[menuItemSelection].plantIndex;
                        if (menuItems[menuItemSelection].itemType == ItemType.Seed ||
                            menuItems[menuItemSelection].itemType == ItemType.Fruit)
                        {
                            PlantType p = (PlantType)loose.inv.items[0].plantIndex;
                            loose.inv.items[0].name += " (" + p.ToString() + ")";
                        }
                        ism.SpawnItem(loose, pos, targ);
                    }
                }
                else
                    rejectFlashTimer = REJECTFLASHTIME;
            }
            // allow customer to exit buy mode
            if (Input.GetKeyDown(currentCustomer.actionDKey) || 
                (padMgr != null && padMgr.gamepads[0].isActive && 
                    padMgr.gPadDown[0].yButton))
            {
                customerMode = CustomerMode.Default;
                currentCustomer.characterFrozen = false;
                menuItemSelection = -1;
                marketInstructions = "MARKET [Welcome]\nE=BUY F=SELL";
                if (padMgr != null && padMgr.gamepads[0].isActive)
                    marketInstructions = "MARKET [Welcome]\nA=BUY B=SELL";
            }
        }
    }

    void CheckSellMode( PlayerControlManager.PlayerActions pa )
    {
        if (customerMode == CustomerMode.Default && pa.actionBDown)
        {
            customerMode = CustomerMode.Sell;
            currentCustomer.characterFrozen = true;
            menuItemSelection = -1;
            marketInstructions = "MARKET [SELL MODE]\nE=SELL V=EXIT";
            if (padMgr != null && padMgr.gamepads[0].isActive)
                marketInstructions = "MARKET [SELL MODE]\nA=SELL Y=EXIT";
            return; // consume input of actionB signal
        }
        if (customerMode == CustomerMode.Sell)
        {
            // allow customer to sell selected item
            if (Input.GetKeyDown(currentCustomer.actionAKey) || 
                (padMgr != null && padMgr.gamepads[0].isActive && 
                    padMgr.gPadDown[0].aButton))
            {
                ItemData iData = currentCustomer.GetPlayerCurrentItemSelection();
                // cannot sell fertilizer (or 'default' type item)
                if (iData != null && (int)iData.type > 1)
                {
                    bool found = false;
                    int value = 0;
                    for (int i = 0; i < menuItems.Length; i++)
                    {
                        if (menuItems[i].itemType == iData.type)
                        {
                            value = menuItems[i].itemValue;
                            // TODO: apply profit margin difference (less value)
                            // temp just minus one gold for now
                            value--;
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        currentCustomer.playerData.gold += value;
                        currentCustomer.DeleteCurrentItemSelection();
                    }
                }
            }
            // allow customer to exit sell mode
            if (Input.GetKeyDown(currentCustomer.actionDKey) || 
                (padMgr != null && padMgr.gamepads[0].isActive && 
                    padMgr.gPadDown[0].yButton))
            {
                customerMode = CustomerMode.Default;
                currentCustomer.characterFrozen = false;
                menuItemSelection = -1;
                marketInstructions = "MARKET [Welcome]\nE=BUY F=SELL";
                if (padMgr != null && padMgr.gamepads[0].isActive)
                    marketInstructions = "MARKET [Welcome]\nA=BUY B=SELL";
            }
        }
    }

    void InitializeMenu()
    {
        menuItems = new MenuItem[7];

        menuItems[0].itemName = "Fertilizer";
        menuItems[0].itemType = ItemType.Fertilizer;
        menuItems[0].plantIndex = -1;
        menuItems[0].itemValue = 1;

        menuItems[1].itemName = "Seed (Corn)";
        menuItems[1].itemType = ItemType.Seed;
        menuItems[1].plantIndex = 1; // NOTE: plant index 0 = Default
        menuItems[1].itemValue = 3;

        menuItems[2].itemName = "Seed (Tomato)";
        menuItems[2].itemType = ItemType.Seed;
        menuItems[2].plantIndex = 2;
        menuItems[2].itemValue = 4;

        menuItems[3].itemName = "Seed (Carrot)";
        menuItems[3].itemType = ItemType.Seed;
        menuItems[3].plantIndex = 3;
        menuItems[3].itemValue = 5;

        menuItems[4].itemName = "Fruit (Corn)";
        menuItems[4].itemType = ItemType.Fruit;
        menuItems[4].plantIndex = 1;
        menuItems[4].itemValue = 7;

        menuItems[5].itemName = "Fruit (Tomato)";
        menuItems[5].itemType = ItemType.Fruit;
        menuItems[5].plantIndex = 2;
        menuItems[5].itemValue = 9;

        menuItems[6].itemName = "Fruit (Carrot)";
        menuItems[6].itemType = ItemType.Fruit;
        menuItems[6].plantIndex = 3;
        menuItems[6].itemValue = 11;
    }

    void OnGUI()
    {
        if (currentCustomer == null)
            return;

        // handle player tag display
        currentCustomer.hidePlayerNameTag = true;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        r.x = 0.35f * w;
        r.y = 0.25f * h;
        r.width = 0.3f * w;
        r.height = 0.5f * h;

        // draw bg
        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.black;
        c.g = 0.618f;
        c.a = 0.381f;
        GUI.color = c;
        GUI.DrawTexture(r, t);
        
        GUIStyle g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt( 20 * (w/1024f) );
        g.fontStyle = FontStyle.Bold;

        string s = "";

        r.x = 0.35f * w;
        r.y = 0.275f * h;
        r.width = 0.3f * w;
        r.height = 0.1f * h;
        g.alignment = TextAnchor.MiddleCenter;
        s = marketInstructions;
        GUI.color = Color.white;
        GUI.Label(r, s, g);

        r.x = 0.375f * w;
        r.y = 0.4f * h;
        r.width = 0.2f * w;
        r.height = 0.05f * h;
        g.alignment = TextAnchor.MiddleLeft;
        for (int i = 0; i < menuItems.Length; i++)
        {
            s = menuItems[i].itemName;
            c = Color.white;
            if (i == menuItemSelection)
            {
                c = Color.yellow;
                if (rejectFlashTimer > 0f)
                    c.g = (rejectFlashTimer * 5f) % 1f;
            }
            GUI.color = c;
            GUI.Label(r, s, g);
            r.y += 0.04f * h;
        }
        r.x = 0.525f * w;
        r.y = 0.4f * h;
        r.width = 0.1f * w;
        r.height = 0.05f * h;
        g.alignment = TextAnchor.MiddleRight;
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (customerMode == CustomerMode.Sell)
                s = (menuItems[i].itemValue-1).ToString(); // (less 1 gold as profit margin)
            else
                s = menuItems[i].itemValue.ToString();
            c = Color.white;
            if (i == menuItemSelection)
            {
                c = Color.yellow;
                if (rejectFlashTimer > 0f)
                    c.g = (rejectFlashTimer * 5f) % 1f;
            }
            GUI.color = c;
            GUI.Label(r, s, g);
            r.y += 0.04f * h;
        }
    }
}
