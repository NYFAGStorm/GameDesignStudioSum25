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
        public int buyItemValue;
        public int sellItemValue;
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

    private int[] maxMenuListPerLevel = new int[11];
    
    int generalItems = 1; // fertilizer
    int plantItemTypes = 2;

    int commonPlants = 10;
    int uncommonPlants = 11;
    int rarePlants = 10;
    int specialPlants = 10;
    //int uniquePlants = 9;

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
            InitializeMaxMenuList();
            playerCheckTimer = PLAYERCHECKTIME;
            marketInstructions = "MARKET [Welcome]\nE=BUY F=SELL";
            if (padMgr != null && padMgr.gamepads[0].isActive)
                marketInstructions = "MARKET [Welcome]\nA=BUY B=SELL";
        }
    }

    void InitializeMaxMenuList()
    {
        for (int i = 0; i < maxMenuListPerLevel.Length; i++)
        {
            switch (i)
            {
                case 0:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes);
                    break;
                case 1:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes);
                    break;
                case 2:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes);
                    break;
                case 3:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes);
                    break;
                case 4:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes);
                    break;
                case 5:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes) +
                        (rarePlants * plantItemTypes);
                    break;
                case 6:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes) +
                        (rarePlants * plantItemTypes);
                    break;
                case 7:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes) +
                        (rarePlants * plantItemTypes);
                    break;
                case 8:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes) +
                        (rarePlants * plantItemTypes) +
                        (specialPlants * plantItemTypes);
                    break;
                case 9:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes) +
                        (rarePlants * plantItemTypes) +
                        (specialPlants * plantItemTypes);
                    break;
                case >9: // TODO:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes) +
                        (rarePlants * plantItemTypes) +
                        (specialPlants * plantItemTypes);
                    break;
            }
            maxMenuListPerLevel[i]--;
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
            //if (currentCustomer.playerData.level < 2)
                maxMenuList = maxMenuListPerLevel[Mathf.Clamp(currentCustomer.playerData.level,0,9)]; // 20
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
                if (menuItems[menuItemSelection].buyItemValue <= currentCustomer.playerData.gold)
                {
                    currentCustomer.playerData.gold -= menuItems[menuItemSelection].buyItemValue;
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
            // NOTE: this is here until leveling tweaked
            int maxMenuList = maxMenuListPerLevel[Mathf.Clamp(currentCustomer.playerData.level,0,9)];
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
                            value = menuItems[i].sellItemValue;
                            found = true;
                            menuItemSelection = i;
                            topOfMenuList = i;
                            if (topOfMenuList > menuItems.Length - MENUITEMSINLIST)
                                topOfMenuList = menuItems.Length - MENUITEMSINLIST;
                            
                            if (topOfMenuList > maxMenuList - MENUITEMSINLIST)
                                topOfMenuList = maxMenuList - MENUITEMSINLIST;
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

    /// <summary>
    /// Returns the market buy price of given item type and give plant type
    /// </summary>
    /// <param name="iType">item type</param>
    /// <param name="pType">plant type</param>
    /// <returns>market price</returns>
    public int GetMarketBuyPrice( ItemType iType, PlantType pType )
    {
        int retInt = 0;

        if (iType == ItemType.Default)
            return retInt;

        bool found = false;

        for (int i = 0; i < menuItems.Length; i++)
        {
            if (menuItems[i].itemType == iType && menuItems[i].plantIndex == (int)pType )
            {
                retInt = menuItems[i].buyItemValue;
                found = true;
                break;
            }
        }

        if (!found)
            Debug.LogWarning("--- MarketManager [GetMarketBuyPrice] : sell price not found for item '" + iType + "' and plant type '" + pType + "'. will return value of zero.");

        return retInt;
    }

    /// <summary>
    /// Returns market sell value of given item type and given plant type (does not factor in quality, see GetFinalMarketSellPrice).
    /// </summary>
    /// <param name="iType">item type</param>
    /// <param name="pType">plant type</param>
    /// <returns>market sell value</returns>
    public int GetMarketSellValue( ItemType iType, PlantType pType )
    {
        int retInt = 0;

        if (iType == ItemType.Default)
            return retInt;

        bool found = false;

        for (int i = 0; i < menuItems.Length; i++)
        {
            if (menuItems[i].itemType == iType && menuItems[i].plantIndex == (int)pType)
            {
                retInt = menuItems[i].sellItemValue;
                found = true;
                break;
            }
        }

        if (!found)
            Debug.LogWarning("--- MarketManager [GetMarketSellValue] : sell price not found for item '" + iType + "' and plant type '" + pType + "'. will return value of zero.");

        return retInt;
    }

    /// <summary>
    /// Returns final sell value of given specific item data taking factors like quality into account
    /// </summary>
    /// <param name="item">specific item data</param>
    /// <returns>final sell value, or zero if not found on menu</returns>
    public int GetFinalMarketSellValue( ItemData item )
    {
        if (item == null || item.type == ItemType.Default)
            return 0;

        return GetAdjustedSellValue(item, GetMarketSellValue(item.type, item.plant));
    }

    int GetAdjustedSellValue( ItemData item, int baseValue )
    {
        int retInt = baseValue;

        if (item == null || item.type == ItemType.Default)
            return retInt;

        // REVIEW: amount of quality ding
        float qualityReduction = 1f - item.quality;
        // quality factor
        int qualityDing = Mathf.RoundToInt(baseValue * qualityReduction);
        retInt -= qualityDing;
        // TODO: other factors

        return retInt;
    }

    void InitializeMenu()
    {
        menuItems = new MenuItem[generalItems + (plantItemTypes * 
            (commonPlants + uncommonPlants + rarePlants + specialPlants))]; // + uniquePlants];

        int idx = 0;

        // -- GENERAL ITEMS --

        menuItems[idx].itemName = "Fertilizer";
        menuItems[idx].itemType = ItemType.Fertilizer;
        menuItems[idx].plantIndex = -1;
        menuItems[idx].buyItemValue = 1;
        menuItems[idx].sellItemValue = 0;
        idx++;

        // -- COMMON PLANTS --

        menuItems[idx].itemName = "Seed (Corn)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Corn;
        menuItems[idx].buyItemValue = 3;
        menuItems[idx].sellItemValue = 2;
        idx++;

        menuItems[idx].itemName = "Fruit (Corn)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Corn;
        menuItems[idx].buyItemValue = 7;
        menuItems[idx].sellItemValue = 6;
        idx++;

        menuItems[idx].itemName = "Seed (Tomato)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Tomato;
        menuItems[idx].buyItemValue = 4;
        menuItems[idx].sellItemValue = 3;
        idx++;

        menuItems[idx].itemName = "Fruit (Tomato)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Tomato;
        menuItems[idx].buyItemValue = 9;
        menuItems[idx].sellItemValue = 8;
        idx++;

        menuItems[idx].itemName = "Seed (Carrot)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Carrot;
        menuItems[idx].buyItemValue = 5;
        menuItems[idx].sellItemValue = 4;
        idx++;

        menuItems[idx].itemName = "Fruit (Carrot)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Carrot;
        menuItems[idx].buyItemValue = 11;
        menuItems[idx].sellItemValue = 10;
        idx++;

        menuItems[idx].itemName = "Seed (Poppy)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Poppy;
        menuItems[idx].buyItemValue = 5;
        menuItems[idx].sellItemValue = 4;
        idx++;

        menuItems[idx].itemName = "Fruit (Poppy)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Poppy;
        menuItems[idx].buyItemValue = 11;
        menuItems[idx].sellItemValue = 10;
        idx++;

        menuItems[idx].itemName = "Seed (Rose)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Rose;
        menuItems[idx].buyItemValue = 5;
        menuItems[idx].sellItemValue = 4;
        idx++;

        menuItems[idx].itemName = "Fruit (Rose)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Rose;
        menuItems[idx].buyItemValue = 11;
        menuItems[idx].sellItemValue = 10;
        idx++;

        menuItems[idx].itemName = "Seed (Sunflower)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Sunflower;
        menuItems[idx].buyItemValue = 5;
        menuItems[idx].sellItemValue = 4;
        idx++;

        menuItems[idx].itemName = "Fruit (Sunflower)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Sunflower;
        menuItems[idx].buyItemValue = 11;
        menuItems[idx].sellItemValue = 10;
        idx++;

        menuItems[idx].itemName = "Seed (Moonflower)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Moonflower;
        menuItems[idx].buyItemValue = 5;
        menuItems[idx].sellItemValue = 4;
        idx++;

        menuItems[idx].itemName = "Fruit (Moonflower)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Moonflower;
        menuItems[idx].buyItemValue = 11;
        menuItems[idx].sellItemValue = 10;
        idx++;

        menuItems[idx].itemName = "Seed (Apple)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Apple;
        menuItems[idx].buyItemValue = 5;
        menuItems[idx].sellItemValue = 4;
        idx++;

        menuItems[idx].itemName = "Fruit (Apple)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Apple;
        menuItems[idx].buyItemValue = 11;
        menuItems[idx].sellItemValue = 10;
        idx++;

        menuItems[idx].itemName = "Seed (Orange)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Orange;
        menuItems[idx].buyItemValue = 5;
        menuItems[idx].sellItemValue = 4;
        idx++;

        menuItems[idx].itemName = "Fruit (Orange)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Orange;
        menuItems[idx].buyItemValue = 11;
        menuItems[idx].sellItemValue = 10;
        idx++;

        menuItems[idx].itemName = "Seed (Lemon)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Lemon;
        menuItems[idx].buyItemValue = 5;
        menuItems[idx].sellItemValue = 4;
        idx++;

        menuItems[idx].itemName = "Fruit (Lemon)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Lemon;
        menuItems[idx].buyItemValue = 11;
        menuItems[idx].sellItemValue = 10;
        idx++;

        // -- UNCOMMON PLANTS --

        menuItems[idx].itemName = "Seed (Lotus)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Lotus;
        menuItems[idx].buyItemValue = 7;
        menuItems[idx].sellItemValue = 6;
        idx++;

        menuItems[idx].itemName = "Fruit (Lotus)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Lotus;
        menuItems[idx].buyItemValue = 16;
        menuItems[idx].sellItemValue = 15;
        idx++;

        menuItems[idx].itemName = "Seed (Marigold)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Marigold;
        menuItems[idx].buyItemValue = 6;
        menuItems[idx].sellItemValue = 5;
        idx++;

        menuItems[idx].itemName = "Fruit (Marigold)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Marigold;
        menuItems[idx].buyItemValue = 15;
        menuItems[idx].sellItemValue = 14;
        idx++;

        menuItems[idx].itemName = "Seed (Magnolia)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Magnolia;
        menuItems[idx].buyItemValue = 5;
        menuItems[idx].sellItemValue = 4;
        idx++;

        menuItems[idx].itemName = "Fruit (Magnolia)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Magnolia;
        menuItems[idx].buyItemValue = 11;
        menuItems[idx].sellItemValue = 10;
        idx++;

        menuItems[idx].itemName = "Seed (Myosotis)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Myosotis;
        menuItems[idx].buyItemValue = 8;
        menuItems[idx].sellItemValue = 6;
        idx++;

        menuItems[idx].itemName = "Fruit (Myosotis)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Myosotis;
        menuItems[idx].buyItemValue = 17;
        menuItems[idx].sellItemValue = 15;
        idx++;

        menuItems[idx].itemName = "Seed (Chrystalia)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Chrystalia;
        menuItems[idx].buyItemValue = 7;
        menuItems[idx].sellItemValue = 5;
        idx++;

        menuItems[idx].itemName = "Fruit (Chrystalia)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Chrystalia;
        menuItems[idx].buyItemValue = 17;
        menuItems[idx].sellItemValue = 15;
        idx++;

        menuItems[idx].itemName = "Seed (Pumpkin)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Pumpkin;
        menuItems[idx].buyItemValue = 7;
        menuItems[idx].sellItemValue = 6;
        idx++;

        menuItems[idx].itemName = "Fruit (Pumpkin)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Pumpkin;
        menuItems[idx].buyItemValue = 16;
        menuItems[idx].sellItemValue = 15;
        idx++;

        menuItems[idx].itemName = "Seed (Underbloom)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Underbloom;
        menuItems[idx].buyItemValue = 7;
        menuItems[idx].sellItemValue = 6;
        idx++;

        menuItems[idx].itemName = "Fruit (Underbloom)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Underbloom;
        menuItems[idx].buyItemValue = 15;
        menuItems[idx].sellItemValue = 14;
        idx++;

        menuItems[idx].itemName = "Seed (Water Lily)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.WaterLily;
        menuItems[idx].buyItemValue = 8;
        menuItems[idx].sellItemValue = 5;
        idx++;

        menuItems[idx].itemName = "Fruit (Water Lily)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.WaterLily;
        menuItems[idx].buyItemValue = 15;
        menuItems[idx].sellItemValue = 12;
        idx++;

        menuItems[idx].itemName = "Seed (Snowgrace)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Snowgrace;
        menuItems[idx].buyItemValue = 7;
        menuItems[idx].sellItemValue = 6;
        idx++;

        menuItems[idx].itemName = "Fruit (Snowgrace)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Snowgrace;
        menuItems[idx].buyItemValue = 15;
        menuItems[idx].sellItemValue = 14;
        idx++;

        menuItems[idx].itemName = "Seed (Popcorn)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Popcorn;
        menuItems[idx].buyItemValue = 7;
        menuItems[idx].sellItemValue = 6;
        idx++;

        menuItems[idx].itemName = "Fruit (Popcorn)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Popcorn;
        menuItems[idx].buyItemValue = 15;
        menuItems[idx].sellItemValue = 14;
        idx++;

        menuItems[idx].itemName = "Seed (Esclipse Flower)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.EclipseFlower;
        menuItems[idx].buyItemValue = 6;
        menuItems[idx].sellItemValue = 4;
        idx++;

        menuItems[idx].itemName = "Fruit (Esclipse Flower)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.EclipseFlower;
        menuItems[idx].buyItemValue = 17;
        menuItems[idx].sellItemValue = 16;
        idx++;

        // -- RARE PLANTS --

        menuItems[idx].itemName = "Seed (Golden Apple)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.GoldenApple;
        menuItems[idx].buyItemValue = 9;
        menuItems[idx].sellItemValue = 8;
        idx++;

        menuItems[idx].itemName = "Fruit (Golden Apple)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.GoldenApple;
        menuItems[idx].buyItemValue = 21;
        menuItems[idx].sellItemValue = 19;
        idx++;

        menuItems[idx].itemName = "Seed (Hollowbloom)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Hollowbloom;
        menuItems[idx].buyItemValue = 6;
        menuItems[idx].sellItemValue = 5;
        idx++;

        menuItems[idx].itemName = "Fruit (Hollowbloom)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Hollowbloom;
        menuItems[idx].buyItemValue = 15;
        menuItems[idx].sellItemValue = 14;
        idx++;

        menuItems[idx].itemName = "Seed (Mandrake)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Mandrake;
        menuItems[idx].buyItemValue = 10;
        menuItems[idx].sellItemValue = 8;
        idx++;

        menuItems[idx].itemName = "Fruit (Mandrake)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Mandrake;
        menuItems[idx].buyItemValue = 21;
        menuItems[idx].sellItemValue = 18;
        idx++;

        menuItems[idx].itemName = "Seed (Frost Lily)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.FrostLily;
        menuItems[idx].buyItemValue = 10;
        menuItems[idx].sellItemValue = 9;
        idx++;

        menuItems[idx].itemName = "Fruit (Frost Lily)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.FrostLily;
        menuItems[idx].buyItemValue = 21;
        menuItems[idx].sellItemValue = 20;
        idx++;

        menuItems[idx].itemName = "Seed (Banana)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Banana;
        menuItems[idx].buyItemValue = 9;
        menuItems[idx].sellItemValue = 8;
        idx++;

        menuItems[idx].itemName = "Fruit (Banana)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Banana;
        menuItems[idx].buyItemValue = 23;
        menuItems[idx].sellItemValue = 19;
        idx++;

        menuItems[idx].itemName = "Seed (Coconut)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Coconut;
        menuItems[idx].buyItemValue = 9;
        menuItems[idx].sellItemValue = 8;
        idx++;

        menuItems[idx].itemName = "Fruit (Coconut)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Coconut;
        menuItems[idx].buyItemValue = 22;
        menuItems[idx].sellItemValue = 21;
        idx++;

        menuItems[idx].itemName = "Seed (Mysteria)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Mysteria;
        menuItems[idx].buyItemValue = 9;
        menuItems[idx].sellItemValue = 8;
        idx++;

        menuItems[idx].itemName = "Fruit (Mysteria)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Mysteria;
        menuItems[idx].buyItemValue = 23;
        menuItems[idx].sellItemValue = 21;
        idx++;

        menuItems[idx].itemName = "Seed (Nightshade)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Nightshade;
        menuItems[idx].buyItemValue = 10;
        menuItems[idx].sellItemValue = 9;
        idx++;

        menuItems[idx].itemName = "Fruit (Nightshade)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Nightshade;
        menuItems[idx].buyItemValue = 23;
        menuItems[idx].sellItemValue = 21;
        idx++;

        menuItems[idx].itemName = "Seed (Crystal Rose)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.CrystalRose;
        menuItems[idx].buyItemValue = 10;
        menuItems[idx].sellItemValue = 9;
        idx++;

        menuItems[idx].itemName = "Fruit (Crystal Rose)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.CrystalRose;
        menuItems[idx].buyItemValue = 22;
        menuItems[idx].sellItemValue = 21;
        idx++;

        menuItems[idx].itemName = "Seed (Yarrow)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Yarrow;
        menuItems[idx].buyItemValue = 9;
        menuItems[idx].sellItemValue = 8;
        idx++;

        menuItems[idx].itemName = "Fruit (Yarrow)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Yarrow;
        menuItems[idx].buyItemValue = 23;
        menuItems[idx].sellItemValue = 19;
        idx++;

        // -- SPECIAL PLANTS --

        menuItems[idx].itemName = "Seed (Dragonroot)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Dragonroot;
        menuItems[idx].buyItemValue = 15;
        menuItems[idx].sellItemValue = 12;
        idx++;

        menuItems[idx].itemName = "Fruit (Dragonroot)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Dragonroot;
        menuItems[idx].buyItemValue = 32;
        menuItems[idx].sellItemValue = 30;
        idx++;

        menuItems[idx].itemName = "Seed (Winter Rose)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.WinterRose;
        menuItems[idx].buyItemValue = 15;
        menuItems[idx].sellItemValue = 12;
        idx++;

        menuItems[idx].itemName = "Fruit (Winter Rose)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.WinterRose;
        menuItems[idx].buyItemValue = 32;
        menuItems[idx].sellItemValue = 30;
        idx++;

        menuItems[idx].itemName = "Seed (Fleur-De-Lis)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.FleurDeLis;
        menuItems[idx].buyItemValue = 14;
        menuItems[idx].sellItemValue = 11;
        idx++;

        menuItems[idx].itemName = "Fruit (Fleur-De-Lis)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.FleurDeLis;
        menuItems[idx].buyItemValue = 32;
        menuItems[idx].sellItemValue = 31;
        idx++;

        menuItems[idx].itemName = "Seed (Tropicus)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.Tropicus;
        menuItems[idx].buyItemValue = 14;
        menuItems[idx].sellItemValue = 11;
        idx++;

        menuItems[idx].itemName = "Fruit (Tropicus)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.Tropicus;
        menuItems[idx].buyItemValue = 32;
        menuItems[idx].sellItemValue = 31;
        idx++;

        menuItems[idx].itemName = "Seed (Mourning Nyx)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.MourningNyx;
        menuItems[idx].buyItemValue = 14;
        menuItems[idx].sellItemValue = 11;
        idx++;

        menuItems[idx].itemName = "Fruit (Mourning Nyx)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.MourningNyx;
        menuItems[idx].buyItemValue = 34;
        menuItems[idx].sellItemValue = 31;
        idx++;

        menuItems[idx].itemName = "Seed (Blast Apple)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.BlastApple;
        menuItems[idx].buyItemValue = 16;
        menuItems[idx].sellItemValue = 11;
        idx++;

        menuItems[idx].itemName = "Fruit (Blast Apple)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.BlastApple;
        menuItems[idx].buyItemValue = 33;
        menuItems[idx].sellItemValue = 30;
        idx++;

        menuItems[idx].itemName = "Seed (Pixie Plumeria)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.PixiePlumeria;
        menuItems[idx].buyItemValue = 16;
        menuItems[idx].sellItemValue = 13;
        idx++;

        menuItems[idx].itemName = "Fruit (Pixie Plumeria)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.PixiePlumeria;
        menuItems[idx].buyItemValue = 33;
        menuItems[idx].sellItemValue = 31;
        idx++;

        menuItems[idx].itemName = "Seed (Fae Foxglove)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.FaeFoxglove;
        menuItems[idx].buyItemValue = 15;
        menuItems[idx].sellItemValue = 11;
        idx++;

        menuItems[idx].itemName = "Fruit (Fae Foxglove)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.FaeFoxglove;
        menuItems[idx].buyItemValue = 32;
        menuItems[idx].sellItemValue = 31;
        idx++;

        menuItems[idx].itemName = "Seed (Druid's Lotus)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.DruidsLotus;
        menuItems[idx].buyItemValue = 15;
        menuItems[idx].sellItemValue = 13;
        idx++;

        menuItems[idx].itemName = "Fruit (Druid's Lotus)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.DruidsLotus;
        menuItems[idx].buyItemValue = 33;
        menuItems[idx].sellItemValue = 31;
        idx++;

        menuItems[idx].itemName = "Seed (Splat Berry)";
        menuItems[idx].itemType = ItemType.Seed;
        menuItems[idx].plantIndex = (int)PlantType.SplatBerry;
        menuItems[idx].buyItemValue = 18;
        menuItems[idx].sellItemValue = 16;
        idx++;

        menuItems[idx].itemName = "Fruit (Splat Berry)";
        menuItems[idx].itemType = ItemType.Fruit;
        menuItems[idx].plantIndex = (int)PlantType.SplatBerry;
        menuItems[idx].buyItemValue = 40;
        menuItems[idx].sellItemValue = 35;
        idx++;

        // -- UNIQUE PLANTS --
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
                s = (menuItems[i].buyItemValue-1).ToString(); // (less 1 gold as profit margin)
            else
                s = menuItems[i].buyItemValue.ToString();
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
