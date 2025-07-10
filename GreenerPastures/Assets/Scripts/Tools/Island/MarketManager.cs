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
    private int topOfMenuList = 0;
    private CustomerMode customerMode;
    private float rejectFlashTimer;
    private Vector3 purchaseOffset = new Vector3(-1f,0f,0f);
    // REVIEW: apply a profit margin difference when player sells an item (less value)
    // temp - for now, just minus one gold when selling items to market

    const float PLAYERCHECKTIME = 1f;
    const float MARKETPROXIMITYRANGE = .5f;
    const float REJECTFLASHTIME = 1f;
    const int MENUITEMSINLIST = 7;


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
            int maxMenuList = menuItems.Length - 1;
            if (currentCustomer.playerData.level < 2)
                maxMenuList = 20;
            menuItemSelection = Mathf.Clamp(menuItemSelection, 0, maxMenuList);
            // set top of menu list
            if (menuItemSelection < topOfMenuList)
                topOfMenuList = menuItemSelection;
            if (menuItemSelection > topOfMenuList + MENUITEMSINLIST)
                topOfMenuList = menuItemSelection - MENUITEMSINLIST;
            // allow player to buy item
            if (Input.GetKeyDown(currentCustomer.actionAKey) || 
                (padMgr != null && padMgr.gamepads[0].isActive && 
                    padMgr.gPadDown[0].aButton))
            {
                if (menuItems[menuItemSelection].itemValue <= currentCustomer.playerData.gold)
                {
                    currentCustomer.playerData.gold -= menuItems[menuItemSelection].itemValue;
                    currentCustomer.AwardXP(PlayerData.XP_BUYFROMSHOP);

                    // try place in inventory, spawn to the side if fail
                    if (InventorySystem.InvHasSlot(currentCustomer.playerData.inventory))
                    {
                        ItemData iData = InventorySystem.InitializeItem(menuItems[menuItemSelection].itemType);
                        if (iData == null)
                            Debug.LogWarning("--- MarketManager [CheckBuyMode] : unable to initialize item. will ignore.");
                        else
                        {
                            iData.plant = (PlantType)menuItems[menuItemSelection].plantIndex;
                            if (menuItems[menuItemSelection].itemType == ItemType.Seed ||
                                menuItems[menuItemSelection].itemType == ItemType.Fruit)
                            {
                                PlantType p = iData.plant;
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
                        loose.inv.items[0].plant = (PlantType)menuItems[menuItemSelection].plantIndex;
                        if (menuItems[menuItemSelection].itemType == ItemType.Seed ||
                            menuItems[menuItemSelection].itemType == ItemType.Fruit)
                        {
                            PlantType p = loose.inv.items[0].plant;
                            loose.inv.items[0].name += " (" + p.ToString() + ")";
                        }
                        ism.SpawnItem(loose, pos, targ, true);
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
            // find item on menu matching player selection and display sell value
            ItemData iData = currentCustomer.GetPlayerCurrentItemSelection();
            bool found = false;
            int value = 0;
            topOfMenuList = 0;
            menuItemSelection = -1;
            if (iData != null)
            {
                // cannot sell fertilizer (or 'default' type item)
                if ( (int)iData.type > 1 )
                {
                    for (int i = 0; i < menuItems.Length; i++)
                    {
                        if (menuItems[i].itemType == iData.type &&
                            menuItems[i].plantIndex == (int)iData.plant)
                        {
                            value = menuItems[i].itemValue;
                            // TODO: apply profit margin difference (less value)
                            // temp just minus one gold for now
                            value--;
                            found = true;
                            menuItemSelection = i;
                            topOfMenuList = i;
                            if (topOfMenuList > menuItems.Length - MENUITEMSINLIST)
                                topOfMenuList = menuItems.Length - MENUITEMSINLIST;
                            if (currentCustomer.playerData.level < 2 && topOfMenuList > 20 - MENUITEMSINLIST)
                                topOfMenuList = 20 - MENUITEMSINLIST;
                            break;
                        }
                    }
                }
            }
            // allow customer to sell selected item
            if (Input.GetKeyDown(currentCustomer.actionAKey) || 
                (padMgr != null && padMgr.gamepads[0].isActive && 
                    padMgr.gPadDown[0].aButton))
            {
                // cannot sell fertilizer (or 'default' type item)
                if (iData != null && (int)iData.type > 1)
                {
                    if (found)
                    {
                        currentCustomer.playerData.gold += value;
                        currentCustomer.DeleteCurrentItemSelection();
                        // PLAYER STATS:
                        currentCustomer.playerData.stats.totalGoldEarned += value;
                        currentCustomer.AwardXP(PlayerData.XP_SELLTOSHOP);
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
        menuItems = new MenuItem[43];

        menuItems[0].itemName = "Fertilizer";
        menuItems[0].itemType = ItemType.Fertilizer;
        menuItems[0].plantIndex = -1;
        menuItems[0].itemValue = 1;

        menuItems[1].itemName = "Seed (Corn)";
        menuItems[1].itemType = ItemType.Seed;
        menuItems[1].plantIndex = (int)PlantType.Corn;
        menuItems[1].itemValue = 3;

        menuItems[2].itemName = "Seed (Tomato)";
        menuItems[2].itemType = ItemType.Seed;
        menuItems[2].plantIndex = (int)PlantType.Tomato;
        menuItems[2].itemValue = 4;

        menuItems[3].itemName = "Seed (Carrot)";
        menuItems[3].itemType = ItemType.Seed;
        menuItems[3].plantIndex = (int)PlantType.Carrot;
        menuItems[3].itemValue = 5;

        menuItems[4].itemName = "Seed (Poppy)";
        menuItems[4].itemType = ItemType.Seed;
        menuItems[4].plantIndex = (int)PlantType.Poppy;
        menuItems[4].itemValue = 6;

        menuItems[5].itemName = "Seed (Rose)";
        menuItems[5].itemType = ItemType.Seed;
        menuItems[5].plantIndex = (int)PlantType.Rose;
        menuItems[5].itemValue = 7;

        menuItems[6].itemName = "Seed (Sunflower)";
        menuItems[6].itemType = ItemType.Seed;
        menuItems[6].plantIndex = (int)PlantType.Sunflower;
        menuItems[6].itemValue = 8;

        menuItems[7].itemName = "Seed (Moonflower)";
        menuItems[7].itemType = ItemType.Seed;
        menuItems[7].plantIndex = (int)PlantType.Moonflower;
        menuItems[7].itemValue = 9;

        menuItems[8].itemName = "Seed (Apple)";
        menuItems[8].itemType = ItemType.Seed;
        menuItems[8].plantIndex = (int)PlantType.Apple;
        menuItems[8].itemValue = 10;

        menuItems[9].itemName = "Seed (Orange)";
        menuItems[9].itemType = ItemType.Seed;
        menuItems[9].plantIndex = (int)PlantType.Orange;
        menuItems[9].itemValue = 11;

        menuItems[10].itemName = "Seed (Lemon)";
        menuItems[10].itemType = ItemType.Seed;
        menuItems[10].plantIndex = (int)PlantType.Lemon;
        menuItems[10].itemValue = 12;

        menuItems[11].itemName = "Fruit (Corn)";
        menuItems[11].itemType = ItemType.Fruit;
        menuItems[11].plantIndex = (int)PlantType.Corn;
        menuItems[11].itemValue = 6;

        menuItems[12].itemName = "Fruit (Tomato)";
        menuItems[12].itemType = ItemType.Fruit;
        menuItems[12].plantIndex = (int)PlantType.Tomato;
        menuItems[12].itemValue = 7;

        menuItems[13].itemName = "Fruit (Carrot)";
        menuItems[13].itemType = ItemType.Fruit;
        menuItems[13].plantIndex = (int)PlantType.Carrot;
        menuItems[13].itemValue = 8;

        menuItems[14].itemName = "Fruit (Poppy)";
        menuItems[14].itemType = ItemType.Fruit;
        menuItems[14].plantIndex = (int)PlantType.Poppy;
        menuItems[14].itemValue = 9;

        menuItems[15].itemName = "Fruit (Rose)";
        menuItems[15].itemType = ItemType.Fruit;
        menuItems[15].plantIndex = (int)PlantType.Rose;
        menuItems[15].itemValue = 10;

        menuItems[16].itemName = "Fruit (Sunflower)";
        menuItems[16].itemType = ItemType.Fruit;
        menuItems[16].plantIndex = (int)PlantType.Sunflower;
        menuItems[16].itemValue = 11;

        menuItems[17].itemName = "Fruit (Moonflower)";
        menuItems[17].itemType = ItemType.Fruit;
        menuItems[17].plantIndex = (int)PlantType.Moonflower;
        menuItems[17].itemValue = 12;

        menuItems[18].itemName = "Fruit (Apple)";
        menuItems[18].itemType = ItemType.Fruit;
        menuItems[18].plantIndex = (int)PlantType.Apple;
        menuItems[18].itemValue = 13;

        menuItems[19].itemName = "Fruit (Orange)";
        menuItems[19].itemType = ItemType.Fruit;
        menuItems[19].plantIndex = (int)PlantType.Orange;
        menuItems[19].itemValue = 14;

        menuItems[20].itemName = "Fruit (Lemon)";
        menuItems[20].itemType = ItemType.Fruit;
        menuItems[20].plantIndex = (int)PlantType.Lemon;
        menuItems[20].itemValue = 15;

        // -- UNCOMMON PLANTS --

        menuItems[21].itemName = "Seed (Lotus)";
        menuItems[21].itemType = ItemType.Seed;
        menuItems[21].plantIndex = (int)PlantType.Lotus;
        menuItems[21].itemValue = 6;

        menuItems[22].itemName = "Seed (Marigold)";
        menuItems[22].itemType = ItemType.Seed;
        menuItems[22].plantIndex = (int)PlantType.Marigold;
        menuItems[22].itemValue = 7;

        menuItems[23].itemName = "Seed (Magnolia)";
        menuItems[23].itemType = ItemType.Seed;
        menuItems[23].plantIndex = (int)PlantType.Magnolia;
        menuItems[23].itemValue = 8;

        menuItems[24].itemName = "Seed (Myosotis)";
        menuItems[24].itemType = ItemType.Seed;
        menuItems[24].plantIndex = (int)PlantType.Myosotis;
        menuItems[24].itemValue = 9;

        menuItems[25].itemName = "Seed (Chrystalia)";
        menuItems[25].itemType = ItemType.Seed;
        menuItems[25].plantIndex = (int)PlantType.Chrystalia;
        menuItems[25].itemValue = 10;
    
        menuItems[26].itemName = "Seed (Pumpkin)";
        menuItems[26].itemType = ItemType.Seed;
        menuItems[26].plantIndex = (int)PlantType.Pumpkin;
        menuItems[26].itemValue = 11;

        menuItems[27].itemName = "Seed (Underbloom)";
        menuItems[27].itemType = ItemType.Seed;
        menuItems[27].plantIndex = (int)PlantType.Underbloom;
        menuItems[27].itemValue = 12;

        menuItems[28].itemName = "Seed (Water Lily)";
        menuItems[28].itemType = ItemType.Seed;
        menuItems[28].plantIndex = (int)PlantType.WaterLily;
        menuItems[28].itemValue = 13;

        menuItems[29].itemName = "Seed (Snowgrace)";
        menuItems[29].itemType = ItemType.Seed;
        menuItems[29].plantIndex = (int)PlantType.Snowgrace;
        menuItems[29].itemValue = 14;

        menuItems[30].itemName = "Seed (Popcorn)";
        menuItems[30].itemType = ItemType.Seed;
        menuItems[30].plantIndex = (int)PlantType.Popcorn;
        menuItems[30].itemValue = 15;

        menuItems[31].itemName = "Seed (Esclipse Flower)";
        menuItems[31].itemType = ItemType.Seed;
        menuItems[31].plantIndex = (int)PlantType.EclipseFlower;
        menuItems[31].itemValue = 16;

        menuItems[32].itemName = "Fruit (Lotus)";
        menuItems[32].itemType = ItemType.Fruit;
        menuItems[32].plantIndex = (int)PlantType.Lotus;
        menuItems[32].itemValue = 12;

        menuItems[33].itemName = "Fruit (Marigold)";
        menuItems[33].itemType = ItemType.Fruit;
        menuItems[33].plantIndex = (int)PlantType.Marigold;
        menuItems[33].itemValue = 13;

        menuItems[34].itemName = "Fruit (Magnolia)";
        menuItems[34].itemType = ItemType.Fruit;
        menuItems[34].plantIndex = (int)PlantType.Magnolia;
        menuItems[34].itemValue = 14;

        menuItems[35].itemName = "Fruit (Myosotis)";
        menuItems[35].itemType = ItemType.Fruit;
        menuItems[35].plantIndex = (int)PlantType.Myosotis;
        menuItems[35].itemValue = 15;

        menuItems[36].itemName = "Fruit (Chrystalia)";
        menuItems[36].itemType = ItemType.Fruit;
        menuItems[36].plantIndex = (int)PlantType.Chrystalia;
        menuItems[36].itemValue = 16;

        menuItems[37].itemName = "Fruit (Pumpkin)";
        menuItems[37].itemType = ItemType.Fruit;
        menuItems[37].plantIndex = (int)PlantType.Pumpkin;
        menuItems[37].itemValue = 17;

        menuItems[38].itemName = "Fruit (Underbloom)";
        menuItems[38].itemType = ItemType.Fruit;
        menuItems[38].plantIndex = (int)PlantType.Underbloom;
        menuItems[38].itemValue = 18;

        menuItems[39].itemName = "Fruit (Water Lily)";
        menuItems[39].itemType = ItemType.Fruit;
        menuItems[39].plantIndex = (int)PlantType.WaterLily;
        menuItems[39].itemValue = 19;

        menuItems[40].itemName = "Fruit (Snowgrace)";
        menuItems[40].itemType = ItemType.Fruit;
        menuItems[40].plantIndex = (int)PlantType.Snowgrace;
        menuItems[40].itemValue = 20;

        menuItems[41].itemName = "Fruit (Popcorn)";
        menuItems[41].itemType = ItemType.Fruit;
        menuItems[41].plantIndex = (int)PlantType.Popcorn;
        menuItems[41].itemValue = 21;

        menuItems[42].itemName = "Fruit (Esclipse Flower)";
        menuItems[42].itemType = ItemType.Fruit;
        menuItems[42].plantIndex = (int)PlantType.EclipseFlower;
        menuItems[42].itemValue = 22;
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

        r.x = 0.3625f * w;
        r.y = 0.4f * h;
        r.width = 0.25f * w;
        r.height = 0.05f * h;
        g.alignment = TextAnchor.MiddleLeft;
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (i < topOfMenuList || i > topOfMenuList + MENUITEMSINLIST)
                continue;
            if (currentCustomer.playerData.level < 2 && i > 20)
                continue;
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
        r.x = 0.5625f * w;
        r.y = 0.4f * h;
        r.width = 0.075f * w;
        r.height = 0.05f * h;
        g.alignment = TextAnchor.MiddleRight;
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (i < topOfMenuList || i > topOfMenuList + MENUITEMSINLIST)
                continue;
            if (currentCustomer.playerData.level < 2 && i > 20)
                continue;
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
