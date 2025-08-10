using UnityEngine;

public class IslandUpgradeMenu : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the island upgrade menu

    public enum StageOfTransaction
    {
        None,
        Purchases,
        Composition,
        Completion
    }

    public enum UpgradeCategory
    {
        Islands,
        Farms,
        Towers,
        OutdoorProps,
        IndoorProps
    }

    public enum  UpgradeType
    {
        IslandSmall,
        IslandMedium,
        IslandLarge,
        IslandVeryLarge,
        FarmTiny,
        FarmModest,
        FarmSizable,
        FarmHuge,
        FarmVast,
        HermitTower,
        WizardTower,
        SorcererTower,
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
        BannerB,
        IntCandleStickA,
        IntCandleStickB,
        IntFireplace,
        IntBookshelf,
        IntWritingDesk,
        IntTapestryA,
        IntTapestryB
    }

    public enum ConfirmType
    {
        None,
        ReplaceExisting,
        Purchase,
        ClearPurchases,
        ComposeIsland,
        CompleteIsland
    }

    [System.Serializable]
    public class MenuItem
    {
        public string name;
        public UpgradeCategory category;
        public UpgradeType type;
        public string description;
        public int price;
        public Texture2D icon;
        public GameObject prefab;
        public float itemPadding;
        public bool playerNowHas;
    }
    public MenuItem[] items;

    private PlayerControlManager pcm;
    private GreenerGameManager ggm;
    private IslandManager im;

    private bool hasSalesmanDiscount;

    private bool cursorMode;
    private GameObject cursor;
    private float cursorSpeed = 1f;
    private Vector3 cursorMove;
    private bool gridLockCursor;

    private GameObject islandObj;
    private Vector3 islandSavedPosition;
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

    private StageOfTransaction stage;

    private UpgradeCategory currentCategory;
    private MenuItem[] displayItems;
    private int menuItemSelection = -1;
    private int topOfMenuList = 0;
    private int maxDisplayItems = 5;

    private Texture2D purchasedBoxIcon;

    private MenuItem currentPurchaseItem;
    private int currentPurchsePrice;
    private MenuItem[] purchaseItemsHeld = new MenuItem[0];
    private int purchaseGoldHeld;
    private MenuItem currentTradeInItem;
    private MenuItem[] tradeInItems = new MenuItem[0];

    private float invalidPulseTimer;

    private AudioManager sfxAudio;

    private SalesVisitManager salesVisit;
    private bool menuOpen;

    private bool confirmPopup;
    private ConfirmType confirmType;
    private string popupMessage;
    private float popupTimer;
    private bool popUpMoveDown;
    private float popupProgress;
    private AnimationCurve popupCurve;

    private AnimationCurve greenerAnimCurve;
    private float greenerPasturesTimer;
    private bool greenerMoveUp;

    private int compositionBeat;
    private float compositionTimer;

    private GameObject[] compositionObjects = new GameObject[0];
    private int currentCompositionObject;
    private int compositionPropDataIndex;

    const float FEEDBACKTIME = 60f;
    const float PULSETIME = 2f;
    const float INVALIDPULSETIME = 1f;
    const float POPTIME = 2f;
    const float GREENERPASTURESTIME = 8f;
    const float GREENERDEPTH = 55f;
    const float COMPOSITIONPAUSETIME = 1f;


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
            validConfig = true; // begin with valid island configuration
            CreateCursor();

            // set up menu
            ConfigureMenuItems();

            // scan player's current configuration (matched to menu items)
            ScanPlayerIslandConfig();

            // lock down player
            pcm.characterFrozen = true;
            pcm.freezeCharacterActions = true;
            pcm.hidePlayerNameTag = true;
            // detect salesman discount
            hasSalesmanDiscount = PlayerSystem.PlayerHasEffect(pcm.playerData, PlayerEffect.SkillFriendsSalesman);

            // disable normal player menus (will turn themselves back on when this is removed)
            InGameControls igc = GameObject.FindFirstObjectByType<InGameControls>();
            if (igc != null)
                igc.SetIslandUpgrades();
            InGameAlmanac iga = GameObject.FindFirstObjectByType<InGameAlmanac>();
            if (iga != null)
                iga.SetIslandUpgrades();
            
            // start menu on islands
            currentCategory = UpgradeCategory.Islands;
            displayItems = GetDisplayItems(currentCategory);

            // set stage to purchasing
            stage = StageOfTransaction.Purchases;

            // popup curve
            popupCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            // greenerAnimCurve for 'greener pastures' moment
            greenerAnimCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            // set purchased box icon
            purchasedBoxIcon = (Texture2D)Resources.Load("Plot_Cursor");
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
                // islands
                case UpgradeType.IslandSmall:
                    items[i].name = "Small Island";
                    items[i].description = "Our smallest island measuring 14 meters in diameter. It comes with a flat grassy surface and enough fertile ground to grow upon.";
                    items[i].price = 7500;
                    items[i].itemPadding = 7f;
                    break;
                case UpgradeType.IslandMedium:
                    items[i].name = "Medium Island";
                    items[i].description = "If you like a little more room, this 16 meter diameter island is priced to fit your budget. Make the move to this great island.";
                    items[i].price = 10000;
                    items[i].itemPadding = 8f;
                    break;
                case UpgradeType.IslandLarge:
                    items[i].name = "Large Island";
                    items[i].description = "This island boasts a 18 meter diameter scale and can accomodate most of our larger upgrades. Keep this at the top of your wish list.";
                    items[i].price = 15000;
                    items[i].itemPadding = 9f;
                    break;
                case UpgradeType.IslandVeryLarge:
                    items[i].name = "Very Large Island";
                    items[i].description = "Our largest island available is 20 meters in diameter and demands a fair amount of accessories to fill it. A very impressive island.";
                    items[i].price = 20000;
                    items[i].itemPadding = 10f;
                    break;
                // farms
                case UpgradeType.FarmTiny:
                    items[i].name = "Tiny Farm";
                    items[i].description = "10 plots of land make up the smallest of farms. Manageable for any Biomancer. They say a garden is all we really need to grow.";
                    items[i].price = 1000;
                    items[i].itemPadding = 2f;
                    break;
                case UpgradeType.FarmModest:
                    items[i].name = "Modest Farm";
                    items[i].description = "When you're ready to handle more crops and more harvests, this upgrade is for you. A 4x4 farm with 16 plots in total.";
                    items[i].price = 2000;
                    items[i].itemPadding = 2.5f;
                    break;
                case UpgradeType.FarmSizable:
                    items[i].name = "Sizable Farm";
                    items[i].description = "A 5x5 farm with 25 plots, brings in a large harvest while maintainable for the casual Biomancer. A good mid-sized farm.";
                    items[i].price = 3000;
                    items[i].itemPadding = 3f;
                    break;
                case UpgradeType.FarmHuge:
                    items[i].name = "Huge Farm";
                    items[i].description = "To impress your guests and the market at the same time, look no further than this glorioud 6x6 farm with a total of 36 plots!";
                    items[i].price = 4000;
                    items[i].itemPadding = 3.5f;
                    break;
                case UpgradeType.FarmVast:
                    items[i].name = "Vast Farm";
                    items[i].description = "Only for the advanced Biomancer, we offer this incredible 7x7 farm with an amazing 49 plots! Only for the larger islands.";
                    items[i].price = 5000;
                    items[i].itemPadding = 4f;
                    break;
                // towers
                case UpgradeType.HermitTower:
                    items[i].name = "Hermit Tower";
                    items[i].description = "A humble tower for a humble Biomancer. This tower need only be an indoor space to craft magic or retreat from dramatic weather.";
                    items[i].price = 5000;
                    items[i].itemPadding = 2f;
                    break;
                case UpgradeType.WizardTower:
                    items[i].name = "Wizard Tower";
                    items[i].description = "Impressive tower and a marvelous upgrade. The classic roofline and taller stature appears as sturdy as it is welcoming to guests.";
                    items[i].price = 10000;
                    items[i].itemPadding = 2.5f;
                    break;
                case UpgradeType.SorcererTower:
                    items[i].name = "Sorcerer Tower";
                    items[i].description = "There is no finer residence for Biomancers. The sorcerer tower is on a grand scale; standing proud and tall to be seen from afar.";
                    items[i].price = 20000;
                    items[i].itemPadding = 3f;
                    break;
                // outdoor props
                case UpgradeType.BushA:
                    items[i].name = "Bush (Style A)";
                    items[i].description = "Bushy! When you need a bush on your farm, look no further! It has everything you want in a bush, and it is maintenance free!";
                    items[i].price = 50;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.BushB:
                    items[i].name = "Bush (Style B)";
                    items[i].description = "An alternate bush. Sometimes the standard bush just isn't right. That's when you turn to this; a standout bush.";
                    items[i].price = 50;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.BushC:
                    items[i].name = "Bush (Style C)";
                    items[i].description = "A different kind of bush. This bush sets itself apart from all others. It definitely is distinctive in it's own way.";
                    items[i].price = 50;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.RockA:
                    items[i].name = "Rock (Style A)";
                    items[i].description = "Rock of Ages; truly a classic rock. Very solid. Guaranteed to hold up under any weather. This rock will never let you down.";
                    items[i].price = 100;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.RockB:
                    items[i].name = "Rock (Style B)";
                    items[i].description = "Rocks come and go, but this one is here to stay. Owning this rock is owning a piece of history; literally very old!";
                    items[i].price = 100;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.RockC:
                    items[i].name = "Rock (Style C)";
                    items[i].description = "A rock without the roll. A no-nonsense rock when you just need a rock you can count on to be there - every time.";
                    items[i].price = 100;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.LampPostA:
                    items[i].name = "Lamp Post (Style A)";
                    items[i].description = "A fancy lamp post, for the discerning Biomancer who fancies themselves cultured and modern.";
                    items[i].price = 500;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.LampPostB:
                    items[i].name = "Lamp Post (Style B)";
                    items[i].description = "A modest lamp post with purpose. A lamp post that says, 'I can be a light for all and not be fancy. Watch me.'";
                    items[i].price = 500;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.BannerA:
                    items[i].name = "Banner (Style A)";
                    items[i].description = "Declare your island clearly and proudly with this broad banner. It automagically conforms to your personal colors!";
                    items[i].price = 1250;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.BannerB:
                    items[i].name = "Banner (Style B)";
                    items[i].description = "A sturdy banner that displays your personal colors automagically! Tell all who see this banner whose farm this is.";
                    items[i].price = 1250;
                    items[i].itemPadding = 0.5f;
                    break;
                // indoor props
                case UpgradeType.IntCandleStickA:
                    items[i].name = "Candle Stick (Style A)";
                    items[i].description = "Everyone needs some light. These candle sticks stand tall on study wooden posts to cast the warm light across your interior.";
                    items[i].price = 750;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.IntCandleStickB:
                    items[i].name = "Candle Stick (Style B)";
                    items[i].description = "Everyone could use more light. This one stands closer to the doorway to help guests remove shoes and coats. Always lit!";
                    items[i].price = 750;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.IntFireplace:
                    items[i].name = "Fireplace";
                    items[i].description = "A warm hearth for cold nights. This is a center piece to any tower interior, and the warm glow and crackling fire is a treat!";
                    items[i].price = 2500;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.IntBookshelf:
                    items[i].name = "Bookshelf";
                    items[i].description = "A bookshelf tells your guests you value knowledge. A must for every well-read Biomancer. Comes with assorted books!";
                    items[i].price = 1750;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.IntWritingDesk:
                    items[i].name = "Writing Desk";
                    items[i].description = "This valuable addition to your tower interior is as useful as it is stylish. Compose letters or write that great novel!";
                    items[i].price = 1500;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.IntTapestryA:
                    items[i].name = "Tapestry (Style A)";
                    items[i].description = "Colorful banner in your style. Hangs on most any wall. Always a fine decor choice for the Biomancer with taste.";
                    items[i].price = 1000;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.IntTapestryB:
                    items[i].name = "Tapestry (Style B)";
                    items[i].description = "Truly a magnificent tapestry, made from the finest material. Automagically configures to your personal colors.";
                    items[i].price = 1000;
                    items[i].itemPadding = 0.5f;
                    break;
                default:
                    break;
            }
        }
    }

    void ScanPlayerIslandConfig()
    {
        PlayerData pData = pcm.playerData;
        IslandData iData = im.islands[pData.playerIsland];

        // for any menu item, determine if player has it now (or lesser variant)
        for (int i = 0; i < items.Length; i++)
        {
            bool found = false;
            switch (items[i].type)
            {
                // islands
                case UpgradeType.IslandSmall:
                    items[i].playerNowHas = (pData.island.w >= 7f);
                    break;
                case UpgradeType.IslandMedium:
                    items[i].playerNowHas = (pData.island.w >= 8f);
                    break;
                case UpgradeType.IslandLarge:
                    items[i].playerNowHas = (pData.island.w >= 9f);
                    break;
                case UpgradeType.IslandVeryLarge:
                    items[i].playerNowHas = (pData.island.w >= 10f);
                    break;
                // farms
                case UpgradeType.FarmTiny:
                    items[i].playerNowHas = (pData.farm.plots.Length >= 10);
                    break;
                case UpgradeType.FarmModest:
                    items[i].playerNowHas = (pData.farm.plots.Length >= 16);
                    break;
                case UpgradeType.FarmSizable:
                    items[i].playerNowHas = (pData.farm.plots.Length >= 25);
                    break;
                case UpgradeType.FarmHuge:
                    items[i].playerNowHas = (pData.farm.plots.Length >= 36);
                    break;
                case UpgradeType.FarmVast:
                    items[i].playerNowHas = (pData.farm.plots.Length >= 49);
                    break;
                // towers
                case UpgradeType.HermitTower:
                    for (int n = 0; n < iData.structures.Length; n++)
                    {
                        if (iData.structures[n].type == StructureType.HermitTower ||
                            iData.structures[n].type == StructureType.WizardTower ||
                            iData.structures[n].type == StructureType.SorcererTower)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.WizardTower:
                    for (int n = 0; n < iData.structures.Length; n++)
                    {
                        if (iData.structures[n].type == StructureType.WizardTower ||
                            iData.structures[n].type == StructureType.SorcererTower)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.SorcererTower:
                    for (int n = 0; n < iData.structures.Length; n++)
                    {
                        if (iData.structures[n].type == StructureType.SorcererTower)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                // outdoor props
                case UpgradeType.BushA:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.BushA)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.BushB:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.BushB)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.BushC:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.BushC)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.RockA:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.RockA)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.RockB:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.RockB)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.RockC:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.RockC)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.LampPostA:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.LampPostA)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.LampPostB:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.LampPostB)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.BannerA:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.BannerA)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.BannerB:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.BannerB)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                // indoor props
                case UpgradeType.IntCandleStickA:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.IntCandleA)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.IntCandleStickB:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.IntCandleB)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.IntFireplace:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.IntFireplace)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.IntBookshelf:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.IntBookshelf)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.IntWritingDesk:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.IntWritingDesk)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.IntTapestryA:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.IntTapestryA)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                case UpgradeType.IntTapestryB:
                    for (int n = 0; n < iData.props.Length; n++)
                    {
                        if (iData.props[n].type == PropType.IntTapestryB)
                        {
                            found = true;
                            break;
                        }
                    }
                    items[i].playerNowHas = found;
                    break;
                default:
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
        pcm.hidePlayerNameTag = false;
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

    void SetCursorToObject( GameObject obj )
    {
        cursor.transform.position = obj.transform.position;
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
        cursorMove = Vector3.zero;

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
        bool found = false;
        for (int i = 0; i < im.islands[pcm.playerData.playerIsland].props.Length; i++)
        {
            if (compositionPropDataIndex == i)
                continue; // ignore prop being moved

            float dist = Vector3.Distance(cursor.transform.position, GameSystem.GetVector(im.islands[pcm.playerData.playerIsland].props[i].location));
            float minDist = 1f;
            if (dist < minDist)
            {
                found = true;
                break;
            }
        }
        if (found)
            retBool = false;
        found = false;
        for (int i = 0; i < im.islands[pcm.playerData.playerIsland].structures.Length; i++)
        {
            float dist = Vector3.Distance(cursor.transform.position, GameSystem.GetVector(im.islands[pcm.playerData.playerIsland].structures[i].location));
            float minDist = 1f;
            if (im.islands[pcm.playerData.playerIsland].structures[i].type == StructureType.HermitTower)
                minDist = 1f;
            if (im.islands[pcm.playerData.playerIsland].structures[i].type == StructureType.WizardTower)
                minDist = 1.5f;
            if (im.islands[pcm.playerData.playerIsland].structures[i].type == StructureType.SorcererTower)
                minDist = 2f;
            if (dist < minDist)
            {
                found = true;
                break;
            }
        }
        if (found)
            retBool = false;

        return retBool;
    }

    void ConfigFeedback( string feedbackString )
    {
        configFeedback = feedbackString;
        feedbackTimer = FEEDBACKTIME;
    }

    void ConfirmPopup( string message, ConfirmType type )
    {
        if (confirmPopup)
            return;

        confirmType = type;
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
        compositionPropDataIndex = GetPropDataIndexByName(obj.name);
        Renderer r = currentConfigObject.GetComponentInChildren<Renderer>();
        if (r != null)
            currentObjectColor = r.material.color;
        currentObjectPadding = objectPadding;
    }

    int GetPropDataIndexByName( string propName )
    {
        int retInt = 0;
        bool found = false;
        string pName = propName.Replace("Prop ", "");
        for (int i = 0; i < im.islands[pcm.playerData.playerIsland].props.Length; i++)
        {
            if (im.islands[pcm.playerData.playerIsland].props[i].name == pName)
            {
                found = true;
                retInt = i;
                break;
            }
        }
        if (!found)
            Debug.LogWarning("--- IslandUpgradeMenu [GetPropDataIndexByLocation] : no prop of name '"+propName+"'. will ignore.");

        return retInt;
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
            popupProgress = popupCurve.Evaluate(popupTimer);
            if (popUpMoveDown)
            {
                if (popupTimer > 0f)
                    popupProgress = 1f - popupProgress;
                else
                    popupProgress = 0f;
            }
        }
        if (invalidPulseTimer > 0f)
        {
            invalidPulseTimer -= Time.deltaTime;
            if (invalidPulseTimer < 0f)
                invalidPulseTimer = 0f;
        }
        if (greenerPasturesTimer > 0f)
        {
            greenerPasturesTimer -= Time.deltaTime;
            if (greenerPasturesTimer < 0f)
            {
                greenerPasturesTimer = 0f;
                if (!greenerMoveUp)
                    PerformReplacementSwaps();
            }
            // island move progress
            float progress = greenerAnimCurve.Evaluate(greenerPasturesTimer / GREENERPASTURESTIME);
            if (!greenerMoveUp)
                progress = 1f - progress;
            if (islandObj != null)
            {
                Vector3 pos = islandSavedPosition;
                pos.y -= progress * GREENERDEPTH;
                islandObj.transform.position = pos;
            }
        }
        if (compositionTimer > 0f)
        {
            compositionTimer -= Time.deltaTime;
            if (compositionTimer < 0f)
            {
                compositionBeat++;
                compositionTimer = 0f;
            }
        }

        // run config pulse
        if (configPulse)
        {
            configObjectPulse = Mathf.Sin(Time.time * PULSETIME * 3f);
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
                    if (!CheckValidCursorLocation())
                        c.r = 1f;
                    r.material.color = c;
                }
            }
        }
        else
            configObjectPulse = 1f;

        // handle composition beats
        if (stage == StageOfTransaction.Composition)
            HandleCompositionBeats();

        // cursor mode
        if (cursorMode)
        {
            if (!cursor.GetComponent<Renderer>().enabled)
                SetCursorVisible(true);
            MoveCursor();
            if (currentConfigObject != null)
            {
                configPulse = true;
                currentConfigObject.transform.position = cursor.transform.position;
                // change data on island manager for this prop
                if (compositionPropDataIndex > -1 && compositionPropDataIndex < im.islands[pcm.playerData.playerIsland].props.Length)
                    im.islands[pcm.playerData.playerIsland].props[compositionPropDataIndex].location = GameSystem.GetPositionData(currentConfigObject.transform.position);
            }
            else
                configPulse = false;
        }
        else if (cursor != null &&
            cursor.GetComponent<Renderer>().enabled)
        {
            SetCursorVisible(false);
            configPulse = false;
        }

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

            if (CheckValidCursorLocation())
            {
                if (Input.GetKeyDown(pcm.actionAKey) || usingPad && padMgr.gPadDown[0].aButton)
                {
                    // next composition object
                    ReleaseCurrentConfigObject();
                    currentCompositionObject++;
                    if (currentCompositionObject > compositionObjects.Length - 1)
                        currentCompositionObject = 0;
                    SetCursorToObject(compositionObjects[currentCompositionObject]);
                    SetCurrentConfigObject(true, compositionObjects[currentCompositionObject], 1f);
                    CameraManager cm = GameObject.FindFirstObjectByType<CameraManager>();
                    if (cm != null)
                        cm.ConfigurePlayerObject(compositionObjects[currentCompositionObject]);
                }
                if (Input.GetKeyDown(pcm.actionBKey) || usingPad && padMgr.gPadDown[0].bButton)
                {
                    // previous composition object
                    ReleaseCurrentConfigObject();
                    currentCompositionObject--;
                    if (currentCompositionObject < 0)
                        currentCompositionObject = compositionObjects.Length - 1;
                    SetCursorToObject(compositionObjects[currentCompositionObject]);
                    SetCurrentConfigObject(true, compositionObjects[currentCompositionObject], 1f);
                    CameraManager cm = GameObject.FindFirstObjectByType<CameraManager>();
                    if (cm != null)
                        cm.ConfigurePlayerObject(compositionObjects[currentCompositionObject]);
                }
                if (Input.GetKeyDown(pcm.actionDKey) || usingPad && padMgr.gPadDown[0].yButton)
                {
                    // step out of composition mode
                    ReleaseCurrentConfigObject();
                    currentCompositionObject = 0;
                    cursorMode = false;
                    CameraManager cm = GameObject.FindFirstObjectByType<CameraManager>();
                    if (cm != null)
                        cm.ConfigurePlayerObject(pcm.gameObject);
                    pcm.characterFrozen = false;
                    pcm.freezeCharacterActions = false;
                    pcm.hidePlayerHUD = false;
                    pcm.hidePlayerNameTag = false;
                    stage = StageOfTransaction.Completion;
                    validConfig = true;
                }
            }
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

        // purchase selection
        if (stage == StageOfTransaction.Purchases && 
            (Input.GetKeyDown(pcm.actionAKey) || usingPad && padMgr.gPadDown[0].aButton))
        {
            if (CanPurchase(displayItems[menuItemSelection]))
            {
                string confirmString = "Are you sure you want to buy\n"
                    + displayItems[menuItemSelection].name;
                MenuItem tradeIn = GetTradeInItemForUpgrade(menuItemSelection);
                if (tradeIn != null)
                    confirmString += "\nand trade in " + tradeIn.name;
                confirmString += "\nfor " + GetPurchasePrice(displayItems[menuItemSelection]) + " gold?";
                ConfirmPopup(confirmString, ConfirmType.Purchase);
                currentPurchaseItem = displayItems[menuItemSelection];
                currentPurchsePrice = GetPurchasePrice(displayItems[menuItemSelection]);
                if (tradeIn != null)
                    currentTradeInItem = tradeIn;
            }
            else
            {
                float rnd = RandomSystem.FlatRandom01();
                if (rnd < .25f)
                    salesVisit.MenuDialogBeat("It looks like that is a little more than you have, friend.");
                else if (rnd >= .25f && rnd < .5f)
                    salesVisit.MenuDialogBeat("Some things are worth waiting for. Just save up your gold.");
                else if (rnd >= .5f && rnd < .75f)
                    salesVisit.MenuDialogBeat("You've got good taste, friend. That item will have to wait.");
                else
                    salesVisit.MenuDialogBeat("It seems you're a little short on gold to buy that item.");
                invalidPulseTimer = INVALIDPULSETIME;
            }
        }
    }

    MenuItem GetTradeInItemForUpgrade( int displayIndex )
    {
        // return existing item being replaced with this upgrade purchase (or null)
        MenuItem retItem = null;

        if (displayItems[displayIndex].category >= UpgradeCategory.OutdoorProps)
            return retItem; // only islands, farms and towers

        for (int i = displayIndex-1; i >= 0; i--)
        {
            if (displayItems[i].playerNowHas)
            {
                retItem = displayItems[i]; // trade-in item
                break;
            }
        }

        return retItem;
    }

    bool IsItemHeldForPurchase( int displayIndex )
    {
        bool retBool = false;

        for (int i = 0; i < purchaseItemsHeld.Length; i++)
        {
            if (!displayItems[displayIndex].playerNowHas &&
                displayItems[displayIndex].type == purchaseItemsHeld[i].type)
            {
                retBool = true;
                break;
            }
        }


        return retBool;
    }

    int GetPurchasePrice( MenuItem item )
    {
        int thisPrice = item.price;
        
        if (hasSalesmanDiscount)
            thisPrice = Mathf.RoundToInt(thisPrice * 0.75f);

        return thisPrice;
    }

    // TODO: do not allow purchases in the same category (island, farm, tower), exchange instead

    bool CanPurchase( MenuItem item )
    {
        bool retBool = false;

        int thisPrice = item.price;
        if (hasSalesmanDiscount)
            thisPrice = Mathf.RoundToInt(thisPrice * 0.75f);
        retBool = ((pcm.playerData.gold - purchaseGoldHeld) >= thisPrice);

        return retBool;
    }

    void MakePurchase( MenuItem item )
    {
        int thisPrice = item.price;
        if (hasSalesmanDiscount)
            thisPrice = Mathf.RoundToInt(thisPrice * 0.75f);
        pcm.playerData.gold -= thisPrice;
    }

    void AddToPurchaseItemsHeld( MenuItem item, MenuItem tradeIn )
    {
        MenuItem[] tmp = new MenuItem[purchaseItemsHeld.Length + 1];
        for (int i = 0; i < purchaseItemsHeld.Length; i++)
        {
            tmp[i] = purchaseItemsHeld[i];
        }
        tmp[purchaseItemsHeld.Length] = item;
        purchaseItemsHeld = tmp;
        // if trade-in store trade-in item
        if (tradeIn == null)
            return;
        tmp = new MenuItem[tradeInItems.Length + 1];
        for (int i = 0; i < tradeInItems.Length; i++)
        {
            tmp[i] = tradeInItems[i];
        }
        tmp[tradeInItems.Length] = tradeIn;
        tradeInItems = tmp;
    }

    void ClearPurchaseItemsHeld()
    {
        purchaseItemsHeld = new MenuItem[0];
        purchaseGoldHeld = 0;
        tradeInItems = new MenuItem[0];
    }

    void GetIslandObject()
    {
        PositionData islandPos = im.islands[pcm.playerData.playerIsland].location;
        Vector3 islandVec = GameSystem.GetVector(islandPos);
        GameObject islandsFolder = GameObject.Find("Islands");
        if (islandsFolder != null)
        {
            bool found = false;
            for (int i = 0; i < islandsFolder.transform.childCount; i++)
            {
                if (Vector3.Distance(islandVec, islandsFolder.transform.GetChild(i).gameObject.transform.position) < 1f)
                {
                    islandObj = islandsFolder.transform.GetChild(i).gameObject;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Debug.LogWarning("--- IslandUpgradeMenu [GetIslandObject] : no island object matching data location "+islandVec+". will ignore.");
                return;
            }
        }

        islandSavedPosition = islandObj.transform.position;
        islandCenter = islandObj.transform.position; // TODO: cleanup
        islandRange = im.islands[pcm.playerData.playerIsland].location.w * 7f;
    }

    void ParentLooseItemsToIsland()
    {
        LooseItemManager[] islandItems = GameObject.FindObjectsByType<LooseItemManager>(FindObjectsSortMode.None);
        for (int i = 0; i < islandItems.Length; i++)
        {
            if (Vector3.Distance(islandItems[i].transform.position, islandCenter) <= islandRange)
                islandItems[i].transform.parent = islandObj.transform;
        }
        // also parent wild plot to island
        GameObject wildPlot = GameObject.Find("Mystic Forager's wild plot");
        if (wildPlot != null)
        {
            wildPlot.transform.parent = islandObj.transform;
        }
    }

    void PrepareForGreenerPastures()
    {
        // if we are swapping islands, perform the lowering of island, rising of new island
        // get current island, prepare to revise
        GetIslandObject();
        // un-parent this island from the islands folder
        islandObj.transform.parent = null;
        // parent loose items within range of island to island for the moment (lowering island)
        ParentLooseItemsToIsland();
    }

    void LaunchGreenerPastures()
    {
        greenerMoveUp = false;
        greenerPasturesTimer = GREENERPASTURESTIME;
    }

    // REVIEW: no
    void AddToCompositionObjects( GameObject obj )
    {
        if (obj == null)
        {
            Debug.LogWarning("--- IslandUpgradeMenu [AddToCompositionObject] : object transform not found. will ignore.");
            return;
        }

        GameObject[] tmp = new GameObject[compositionObjects.Length + 1];
        for (int i = 0; i < compositionObjects.Length; i++)
        {
            tmp[i] = compositionObjects[i];
        }
        tmp[compositionObjects.Length] = obj;
        compositionObjects = tmp;
    }

    void PerformReplacementSwaps()
    {
        // set island object to whole number y position
        Vector3 iPos = islandObj.transform.position;
        iPos.y = -GREENERDEPTH;
        islandObj.transform.position = iPos;

        islandRange = 7;

        im.SetCheckProps(false); // suspend checking props

        // -- ISLAND --
        float radiusDelta = im.islands[pcm.playerData.playerIsland].location.w * 7f; // TODO: find whole number scale
        Vector3 newIslandScale = Vector3.one;
        float radiusIncrement = (1f / 7f);
        for (int i = 0; i < purchaseItemsHeld.Length; i++)
        {
            if (purchaseItemsHeld[i].type == UpgradeType.IslandSmall)
            {
                newIslandScale = Vector3.one;
                radiusDelta = 7f - radiusDelta;
                islandRange = 7;
            }
            if (purchaseItemsHeld[i].type == UpgradeType.IslandMedium)
            {
                newIslandScale = Vector3.one * (1f + (radiusIncrement * 1f));
                radiusDelta = 8f - radiusDelta;
                islandRange = 8;
            }
            if (purchaseItemsHeld[i].type == UpgradeType.IslandLarge)
            {
                newIslandScale = Vector3.one * (1f + (radiusIncrement * 2f));
                radiusDelta = 9f - radiusDelta;
                islandRange = 9;
            }
            if (purchaseItemsHeld[i].type == UpgradeType.IslandVeryLarge)
            {
                newIslandScale = Vector3.one * (1f + (radiusIncrement * 3f));
                radiusDelta = 10f - radiusDelta;
                islandRange = 10;
            }
        }
        // explain to island manager new island scale
        im.islands[pcm.playerData.playerIsland].location.w = newIslandScale.x;

        // find and remove ground and under children from island object
        islandObj.transform.Find("Ground").gameObject.name = "Old Ground";
        islandObj.transform.Find("Old Ground").parent = null;
        islandObj.transform.Find("Under").gameObject.name = "Old Under";
        islandObj.transform.Find("Old Under").parent = null;
        Destroy(GameObject.Find("Old Ground"));
        Destroy(GameObject.Find("Old Under"));

        // spawn island
        GameObject newIsland = GameObject.Instantiate((GameObject)Resources.Load("Test Island"), islandCenter + (Vector3.down * GREENERDEPTH), Quaternion.identity);
        newIsland.name = islandObj.name;
        GameObject oldIsland = islandObj;
        // scale new island
        newIsland.transform.localScale = newIslandScale;
        // reset island scale to one while retaining ground and under scale
        GameObject ground = newIsland.transform.Find("Ground").gameObject;
        GameObject under = newIsland.transform.Find("Under").gameObject;
        ground.transform.parent = null;
        under.transform.parent = null;
        newIsland.transform.localScale = Vector3.one;
        ground.transform.parent = newIsland.transform;
        under.transform.parent = newIsland.transform;
        // replace island
        islandObj = newIsland;
        GameObject[] islandChildren = new GameObject[oldIsland.transform.childCount];
        int plotCount = 0;
        for (int i = 0; i < islandChildren.Length; i++)
        {
            islandChildren[i] = oldIsland.transform.GetChild(i).gameObject;
        }
        for (int i = 0; i < islandChildren.Length; i++)
        {
            if (islandChildren[i].GetComponent<TeleportManager>() != null)
            {
                // configure island property on teleport nodes
                islandChildren[i].GetComponent<TeleportManager>().islandObj = newIsland;
                islandChildren[i].GetComponent<TeleportManager>().islandRadius = islandRange;
            }
            if (islandChildren[i].GetComponent<PlotManager>() != null)
            {
                if (islandChildren[i].name != "Mystic Forager's wild plot")
                    plotCount++; // do not count wild plot
            }
            islandChildren[i].transform.parent = newIsland.transform;
        }
        Destroy(oldIsland);

        // -- FARM --
        // farm plot spawn
        int newPlotCount = 0;
        for (int i = 0; i < purchaseItemsHeld.Length; i++)
        {
            if (purchaseItemsHeld[i].type == UpgradeType.FarmTiny)
                newPlotCount = 10;
            if (purchaseItemsHeld[i].type == UpgradeType.FarmModest)
                newPlotCount = 16;
            if (purchaseItemsHeld[i].type == UpgradeType.FarmSizable)
                newPlotCount = 25;
            if (purchaseItemsHeld[i].type == UpgradeType.FarmHuge)
                newPlotCount = 36;
            if (purchaseItemsHeld[i].type == UpgradeType.FarmVast)
                newPlotCount = 49;
        }
        if (newPlotCount > plotCount)
        {
            int side = (int)Mathf.Sqrt((int)newPlotCount);
            newPlotCount -= plotCount;
            // use corner of farm
            Vector3 farmCorner = new Vector3(-1f, 0f, -1f);
            farmCorner += newIsland.transform.position;
            if (side >= 5)
                farmCorner += Vector3.left;
            if (side >= 6)
                farmCorner += Vector3.left;
            if (side >= 7)
                farmCorner += Vector3.forward; // probably too close to tower (but fits small island)
            int count = newPlotCount;
            // add to player's farm data plot array
            PlotData[] tmp = new PlotData[pcm.playerData.farm.plots.Length + count];
            for (int q = 0; q < pcm.playerData.farm.plots.Length; q++)
            {
                tmp[q] = pcm.playerData.farm.plots[q];
            }
            for (int q = pcm.playerData.farm.plots.Length; q < pcm.playerData.farm.plots.Length + count; q++)
            {
                tmp[q] = new PlotData();
                tmp[q].condition = PlotCondition.Wild;
            }
            count = pcm.playerData.farm.plots.Length; // REVIEW: this use in data location below
            pcm.playerData.farm.plots = tmp;
            // place farm plot-by-plot, ignore existing
            PlotManager[] plots = GameObject.FindObjectsByType<PlotManager>(FindObjectsSortMode.None);
            for (int n = 0; n < side; n++)
            {
                for (int t = 0; t < side; t++)
                {
                    Vector3 pos = farmCorner;
                    pos.x += t;
                    pos.z -= n;
                    bool found = false;
                    foreach ( PlotManager plot in plots )
                    {
                        if (Vector3.Distance(plot.transform.position,pos) <= .5f)
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        GameObject newPlot = GameObject.Instantiate((GameObject)Resources.Load("Plot"));
                        newPlot.name = "Plot";
                        newPlot.transform.position = pos;
                        newPlot.transform.parent = islandObj.transform;
                        newPlot.GetComponent<PlotManager>().data.condition = PlotCondition.Wild;
                        pcm.playerData.farm.plots[count].location.x = pos.x;
                        pcm.playerData.farm.plots[count].location.z = pos.z;
                        newPlot.GetComponent<PlotManager>().data.location = pcm.playerData.farm.plots[count].location;
                        count++;
                    }
                }
            }
        }

        // -- TOWER --
        GameObject newTowerObject = null;
        Vector3 towerOffset = new Vector3(1f, 1f, 2f);
        // offset 'forward' for each size up in island radius
        towerOffset += Vector3.forward * radiusDelta;
        StructureType towerType = StructureType.Default;
        // tower spawn
        for (int i = 0; i < purchaseItemsHeld.Length; i++)
        {
            if (purchaseItemsHeld[i].type == UpgradeType.HermitTower)
            {
                towerType = StructureType.HermitTower;
                newTowerObject = GameObject.Instantiate((GameObject)Resources.Load("Hermit Tower"));
            }
            if (purchaseItemsHeld[i].type == UpgradeType.WizardTower)
            {
                towerType = StructureType.WizardTower;
                newTowerObject = GameObject.Instantiate((GameObject)Resources.Load("Wizard Tower"));
            }
            if (purchaseItemsHeld[i].type == UpgradeType.SorcererTower)
            {
                towerType = StructureType.SorcererTower;
                newTowerObject = GameObject.Instantiate((GameObject)Resources.Load("Sorcerer Tower"));
            }
        }
        GameObject towerInterior = null;
        if (newTowerObject != null)
        {
            // find old tower
            GameObject oldTowerObject = islandObj.transform.Find("Structure wiz tower").gameObject;
            // position and name new tower
            newTowerObject.transform.position = islandObj.transform.position + towerOffset;
            newTowerObject.transform.parent = islandObj.transform;
            newTowerObject.name = oldTowerObject.name;
            // set tower data
            for (int i = 0; i < im.islands[pcm.playerData.playerIsland].structures.Length; i++)
            {
                if (im.islands[pcm.playerData.playerIsland].structures[i].type == StructureType.HermitTower ||
                    im.islands[pcm.playerData.playerIsland].structures[i].type == StructureType.WizardTower ||
                    im.islands[pcm.playerData.playerIsland].structures[i].type == StructureType.SorcererTower)
                {
                    im.islands[pcm.playerData.playerIsland].structures[i].type = towerType;
                    im.islands[pcm.playerData.playerIsland].structures[i].location.z = towerOffset.z;
                }
            }
            // delete old tower
            Destroy(oldTowerObject);
            // move tower interior same offset
            towerInterior = islandObj.transform.Find("Structure tower interior").gameObject;
            Vector3 iTPos = towerInterior.transform.position;
            iTPos.z += towerOffset.z - 2f; // TODO: cannot use +, need solid numbers
            towerInterior.transform.position = iTPos;
            // move tower teleporters same offset
            for (int i =0; i < islandChildren.Length; i++)
            {
                if (islandChildren[i].GetComponent<TeleportManager>() != null)
                {
                    TeleportManager tm = islandChildren[i].GetComponent<TeleportManager>();
                    if (tm.teleporterTag == "tower")
                    {
                        Vector3 tPos = tm.gameObject.transform.position;
                        tPos.z += towerOffset.z - 2f;
                        tm.gameObject.transform.position = tPos;
                        if (tm.cameraMode == CameraManager.CameraMode.PanFollow)
                        {            
                            // move camera settings on teleport node same offset
                            tm.cameraPanModePosition.z += towerOffset.z - 2f;
                        }
                    }
                }
            }
            // adjust data on island manager for these teleporters
            for (int i = 0; i < im.islands[pcm.playerData.playerIsland].tports.Length; i++)
            {
                if (im.islands[pcm.playerData.playerIsland].tports[i].tag == "tower")
                {
                    im.islands[pcm.playerData.playerIsland].tports[i].location.z += towerOffset.z - 2f;
                    if (im.islands[pcm.playerData.playerIsland].tports[i].cameraMode == CameraManager.CameraMode.PanFollow)
                    {
                        im.islands[pcm.playerData.playerIsland].tports[i].cameraPosition.z += towerOffset.z - 2f;
                    }
                }
            }
        }

        // adjust data on island manager for central teleporter
        PositionData centralPos = new PositionData();
        for (int i = 0; i < im.islands[pcm.playerData.playerIsland].tports.Length; i++)
        {
            if (im.islands[pcm.playerData.playerIsland].tports[i].tag == "centralTport")
            {
                Vector3 tv = GameSystem.GetVector(im.islands[pcm.playerData.playerIsland].tports[i].location);
                tv += (tv - islandCenter).normalized * radiusDelta;
                tv.y = 0f;
                im.islands[pcm.playerData.playerIsland].tports[i].location = GameSystem.GetPositionData(tv);
                centralPos = GameSystem.GetPositionData(tv);
            }
        }        
        // move central teleporter
        for (int i = 0; i < islandChildren.Length; i++)
        {
            if (islandChildren[i].GetComponent<TeleportManager>() != null)
            {
                TeleportManager tm = islandChildren[i].GetComponent<TeleportManager>();
                if (tm.teleporterTag == "centralTport")
                {
                    tm.gameObject.transform.localPosition = GameSystem.GetVector(centralPos);
                    salesVisit.SetNewTeleporterSpot(GameSystem.GetVector(centralPos));
                }
            }
        }

        // -- OUTDOOR PROPS --
        for (int n = 0; n < im.islands[pcm.playerData.playerIsland].props.Length; n++)
        {
            Vector3 pPos = GameSystem.GetVector(im.islands[pcm.playerData.playerIsland].props[n].location);
            if (im.islands[pcm.playerData.playerIsland].props[n].type < PropType.IntCandleA)
            {
                GameObject childProp = null;
                for (int i = 0; i < islandChildren.Length; i++)
                {
                    if (islandChildren[i].transform.localPosition == pPos)
                        childProp = islandChildren[i];
                }
                // push out from center by radiusDelta
                pPos += (pPos - islandCenter).normalized * radiusDelta;
                im.islands[pcm.playerData.playerIsland].props[n].location = GameSystem.GetPositionData(pPos);
                Vector3 cPropPos = childProp.transform.localPosition;
                cPropPos += (pPos - islandCenter).normalized * radiusDelta;
                // set to whole number (grid lock)
                cPropPos.x = Mathf.RoundToInt(cPropPos.x);
                cPropPos.y = Mathf.RoundToInt(cPropPos.y);
                cPropPos.z = Mathf.RoundToInt(cPropPos.z);
                childProp.transform.localPosition = cPropPos;
                //if (im.islands[pcm.playerData.playerIsland].props[n].type != PropType.Mailbox)
                if (childProp != null)
                    AddToCompositionObjects(childProp);
            }
        }

        // REVIEW: and center of farm movable?

        // outdoor prop spawn
        int propMove = 0;
        for (int i = 0; i < purchaseItemsHeld.Length; i++)
        {
            if (purchaseItemsHeld[i].category == UpgradeCategory.OutdoorProps)
            {
                string pName = "";
                PropType pType = PropType.Default;
                Vector3 pPos = Vector3.zero;
                string prefabName = "";
                switch (purchaseItemsHeld[i].type)
                {
                    case UpgradeType.BushA:
                        pName = "bush A";
                        pType = PropType.BushA;
                        prefabName = "Bush A";
                        break;
                    case UpgradeType.BushB:
                        pName = "bush B";
                        pType = PropType.BushB;
                        prefabName = "Bush B";
                        break;
                    case UpgradeType.BushC:
                        pName = "bush C";
                        pType = PropType.BushC;
                        prefabName = "Bush C";
                        break;
                    case UpgradeType.RockA:
                        pName = "rock A";
                        pType = PropType.RockA;
                        prefabName = "Rock A";
                        break;
                    case UpgradeType.RockB:
                        pName = "rock B";
                        pType = PropType.RockB;
                        prefabName = "Rock B";
                        break;
                    case UpgradeType.RockC:
                        pName = "rock C";
                        pType = PropType.RockC;
                        prefabName = "Rock C";
                        break;
                    case UpgradeType.LampPostA:
                        pName = "lamp post A";
                        pType = PropType.LampPostA;
                        prefabName = "Lamp Post A";
                        break;
                    case UpgradeType.LampPostB:
                        pName = "lamp post B";
                        pType = PropType.LampPostB;
                        prefabName = "Lamp Post B";
                        break;
                    case UpgradeType.BannerA:
                        pName = "banner A";
                        pType = PropType.BannerA;
                        prefabName = "Banner A";
                        break;
                    case UpgradeType.BannerB:
                        pName = "banner B";
                        pType = PropType.BannerB;
                        prefabName = "Banner B";
                        break;
                }
                // spawn prop
                GameObject newProp = GameObject.Instantiate((GameObject)Resources.Load(prefabName));
                newProp.name = "Prop " + pName;
                // rando position
                pPos.x = propMove++;
                newProp.transform.position = islandObj.transform.position + pPos;
                newProp.transform.parent = islandObj.transform;
                // store outdoor props as composition objects
                AddToCompositionObjects(newProp);
                // add island data for exterior prop
                PropData[] tmp = new PropData[im.islands[pcm.playerData.playerIsland].props.Length + 1];
                for (int n = 0; n < im.islands[pcm.playerData.playerIsland].props.Length; n++)
                {
                    tmp[n] = im.islands[pcm.playerData.playerIsland].props[n];
                }
                tmp[im.islands[pcm.playerData.playerIsland].props.Length] = new PropData();
                tmp[im.islands[pcm.playerData.playerIsland].props.Length].name = pName;
                tmp[im.islands[pcm.playerData.playerIsland].props.Length].type = pType;
                tmp[im.islands[pcm.playerData.playerIsland].props.Length].location = GameSystem.GetPositionData(pPos);
                im.islands[pcm.playerData.playerIsland].props = tmp;
            }
        }

        // indoor prop activation
        for (int i =0; i < purchaseItemsHeld.Length; i++)
        {
            if (purchaseItemsHeld[i].category == UpgradeCategory.IndoorProps)
            {
                if (towerInterior == null)
                {
                    towerInterior = islandObj.transform.Find("Structure tower interior").gameObject;
                    if (towerInterior == null)
                    {
                        Debug.LogWarning("--- IslandUpgradeMenu [swap] : indoor prop held but no tower interior available. will ignore.");
                        continue;
                    }
                }
                GameObject decoObj = towerInterior.transform.Find("Deco").gameObject;
                if (decoObj == null)
                {
                    Debug.LogWarning("--- IslandUpgradeMenu [swap] : indoor prop held but no tower interior deco object found. will ignore.");
                    continue;
                }
                string pName = "";
                PropType pType = PropType.Default;
                switch (purchaseItemsHeld[i].type)
                {
                    case UpgradeType.IntCandleStickA:
                        decoObj.transform.Find("Candle Prop A").gameObject.SetActive(true);
                        pName = "int candle stick A";
                        pType = PropType.IntCandleA;
                        break;
                    case UpgradeType.IntCandleStickB:
                        decoObj.transform.Find("Candle Prop B").gameObject.SetActive(true);
                        pName = "int candle stick B";
                        pType = PropType.IntCandleB;
                        break;
                    case UpgradeType.IntFireplace:
                        decoObj.transform.Find("Fireplace Prop").gameObject.SetActive(true);
                        pName = "int fireplace";
                        pType = PropType.IntFireplace;
                        break;
                    case UpgradeType.IntBookshelf:
                        decoObj.transform.Find("Bookshelf").gameObject.SetActive(true);
                        pName = "int bookshelf";
                        pType = PropType.IntBookshelf;
                        break;
                    case UpgradeType.IntWritingDesk:
                        decoObj.transform.Find("Writing Desk").gameObject.SetActive(true);
                        pName = "int writing desk";
                        pType = PropType.IntWritingDesk;
                        break;
                    case UpgradeType.IntTapestryA:
                        decoObj.transform.Find("Tapestry A").gameObject.SetActive(true);
                        pName = "int tapestry A";
                        pType = PropType.IntTapestryA;
                        break;
                    case UpgradeType.IntTapestryB:
                        decoObj.transform.Find("Tapestry B").gameObject.SetActive(true);
                        pName = "int tapestry B";
                        pType = PropType.IntTapestryB;
                        break;
                }
                // add island data for interior prop
                PropData[] tmp = new PropData[im.islands[pcm.playerData.playerIsland].props.Length + 1];
                for (int n = 0; n < im.islands[pcm.playerData.playerIsland].props.Length; n++)
                {
                    tmp[n] = im.islands[pcm.playerData.playerIsland].props[n];
                }
                tmp[im.islands[pcm.playerData.playerIsland].props.Length] = new PropData();
                tmp[im.islands[pcm.playerData.playerIsland].props.Length].name = pName;
                tmp[im.islands[pcm.playerData.playerIsland].props.Length].type = pType;
                im.islands[pcm.playerData.playerIsland].props = tmp;
            }
        }

        // re-parent new island to islands folder
        GameObject islandFolderObj = GameObject.Find("Islands");
        islandObj.transform.parent = islandFolderObj.transform;

        // get island manger to re-acquire props for period check
        im.ForceReConfigurePropRenderers(im.islands[pcm.playerData.playerIsland], islandObj);
        im.SetCheckProps(true);

        greenerMoveUp = true;
        greenerPasturesTimer = GREENERPASTURESTIME;
    }

    void LaunchCompositionStage()
    {
        stage = StageOfTransaction.Composition;
        validConfig = false; // suspend transaction until valid

        // step off island, greener pastures event, and compose upgrades
        float rnd = RandomSystem.FlatRandom01();
        if (rnd < .25f)
            salesVisit.MenuDialogBeat("All right then. Just step off the island for a moment with me.");
        else if (rnd >= .25f && rnd < .5f)
            salesVisit.MenuDialogBeat("We're ready to compose your island upgrades. Just stand with me.");
        else if (rnd >= .5f && rnd < .75f)
            salesVisit.MenuDialogBeat("It's that time I love, when we compose your upgrades. Follow me.");
        else
            salesVisit.MenuDialogBeat("If you'll just stand here with me, we'll begin to compose now.");

        // composition beats started
        compositionTimer = COMPOSITIONPAUSETIME;
    }

    void HandleCompositionBeats()
    {
        if (compositionTimer > 0f)
            return;

        // TODO: fast-forward in composition beats if we are not performing greener pastures

        switch (compositionBeat)
        {
            case 1:
                if (salesVisit.menuPlayerResponse)
                {
                    salesVisit.menuPlayerResponse = false;
                    salesVisit.MenuVFXBeat(-.5f, 0f);
                    salesVisit.currentBeat.beatPosition.w = -0.5f;
                    compositionTimer = .1f;
                }
                break;
            case 2:
                salesVisit.menuBeatTimeUp = false;
                salesVisit.ToggleSalesmanPlatform();
                compositionTimer = 1f;
                break;
            case 3:
                salesVisit.MenuMarkBeat(new Vector3(6.18f,0f,-10f));
                compositionTimer = 1f;
                break;
            case 4:
                if (salesVisit.menuNpcCallback)
                {
                    salesVisit.menuNpcCallback = false;
                    salesVisit.MenuDialogBeat("Now we ask for the grace and assistance of the Genesis Tree.");
                    compositionTimer = .1f;
                }
                break;
            case 5:
                if (salesVisit.menuPlayerResponse)
                {
                    salesVisit.menuNpcCallback = false;
                    salesVisit.MenuDialogBeat("This part never gets old. I love my job.");
                    compositionTimer = .1f;
                }
                break;
            case 6:
                if (salesVisit.menuPlayerResponse)
                {
                    salesVisit.menuPlayerResponse = false;
                    salesVisit.MenuVFXBeat(-.5f, 1f);
                    compositionTimer = 0.9f;
                    //
                    PrepareForGreenerPastures();
                }
                break;
            case 7:
                if (salesVisit.menuBeatTimeUp)
                {
                    salesVisit.menuBeatTimeUp = false;
                    compositionTimer = 4f;
                    //
                    LaunchGreenerPastures();
                }
                break;
            case 8:
                salesVisit.MenuDialogBeat("Wait for it...");
                compositionTimer = 4f;
                break;
            case 9:
                if (salesVisit.menuPlayerResponse && greenerPasturesTimer == 0f)
                {
                    salesVisit.menuPlayerResponse = false;
                    salesVisit.MenuDialogBeat("There you are, my friend, a refreshed island and greener pastures!");
                    compositionTimer = 4f;
                }
                break;
            case 10:
                if (salesVisit.menuPlayerResponse)
                {
                    salesVisit.menuPlayerResponse = false;
                    salesVisit.MenuDialogBeat("Let's go take a look");
                    compositionTimer = 1f;
                }
                break;
            case 11:
                if (salesVisit.menuPlayerResponse)
                {
                    salesVisit.menuPlayerResponse = false;
                    salesVisit.MenuMarkBeat(new Vector3(1f, 0f, -6.5f));
                    compositionTimer = 6.18f;
                }
                break;
            case 12:
                if (salesVisit.menuNpcCallback)
                {
                    salesVisit.menuNpcCallback = false;
                    salesVisit.ToggleSalesmanPlatform();
                    salesVisit.MenuVFXBeat(-.5f, 1f);
                    pcm.characterFrozen = false;
                    pcm.freezeCharacterActions = false;
                    pcm.hidePlayerNameTag = false;
                    pcm.playerData.island = im.islands[pcm.playerData.playerIsland].location;
                    pcm.playerData.island.w = islandRange;
                    compositionTimer = 4f;
                }
                break;
            case 13:
                if (salesVisit.menuBeatTimeUp)
                {
                    salesVisit.menuBeatTimeUp = false;
                    salesVisit.MenuDialogBeat("Let me help you move items around.");
                    compositionTimer = 1f;
                    string s = "Move items, press E and F to switch items, press V to complete composition.";
                    if (usingPad)
                        s = "Move items, press A and B to switch items, press Y to complete composition.";
                    ConfigFeedback(s);
                }
                break;
            case 14:
                if (salesVisit.menuPlayerResponse)
                {
                    SetCursorToObject(compositionObjects[0]);
                    SetCurrentConfigObject(true, compositionObjects[0], 1f);
                    cursorMode = true;
                    gridLockCursor = true;
                    compositionTimer = 3f;
                    pcm.freezeCharacterActions = true;
                    pcm.characterFrozen = true;
                    pcm.hidePlayerNameTag = true;
                    pcm.hidePlayerHUD = true;
                    CameraManager cm = GameObject.FindFirstObjectByType<CameraManager>();
                    if (cm != null)
                        cm.ConfigurePlayerObject(compositionObjects[0]);
                }
                break;
            case 15:
                if (salesVisit.menuBeatTimeUp)
                {
                    salesVisit.menuBeatTimeUp = false;
                    salesVisit.MenuDialogBeat("Go ahead an move this around your island.");
                    compositionTimer = 1f;
                }
                break;
            case 16:
                if (salesVisit.menuPlayerResponse)
                {
                    salesVisit.menuPlayerResponse = false;
                    salesVisit.MenuDialogBeat("When you like where it is, press your action button.");
                    compositionTimer = 3f;
                }
                break;
            case 17:
                if (salesVisit.menuBeatTimeUp)
                {
                    salesVisit.menuBeatTimeUp = false;
                    salesVisit.MenuDialogBeat("Take your time.");
                    compositionTimer = 1f;
                }
                break;
            case 18:
                if (salesVisit.menuPlayerResponse)
                {
                    stage = StageOfTransaction.Completion; // temp
                    validConfig = true; // temp;
                }
                break;
        }
    }

    string FormCategoryLabel( UpgradeCategory category )
    {
        string retString = "";

        switch (category)
        {
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

        if (!confirmPopup && 
            stage != StageOfTransaction.Composition && stage != StageOfTransaction.Completion)
        {
            // menu box
            s = "ISLAND UPGRADES";
            GUI.Box(r, s, g);
        }

        if (!confirmPopup && stage == StageOfTransaction.Purchases)
        {
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
            g.hover.textColor = Color.yellow;
            g.active.textColor = Color.yellow;
            s = "<<";
            if (usingPad)
                s += "\nL Bump";
            if (GUI.Button(r, s, g) ||
                (usingPad && padMgr.gPadDown[0].LBump))
            {
                currentCategory--;
                if (currentCategory < UpgradeCategory.Islands)
                    currentCategory = UpgradeCategory.IndoorProps;
                displayItems = GetDisplayItems(currentCategory);
                topOfMenuList = 0;
                menuItemSelection = 0;

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
                    currentCategory = UpgradeCategory.Islands;
                displayItems = GetDisplayItems(currentCategory);
                topOfMenuList = 0;
                menuItemSelection = 0;

                // consuming input, but why?
                if (usingPad)
                    padMgr.gPadDown[0].RBump = false;
            }
        }

        GUI.enabled = !confirmPopup;

        if (!confirmPopup && stage == StageOfTransaction.Purchases)
        {
            // menu item list
            r.y = 0.25f * h;
            for (int i = topOfMenuList; i < Mathf.Min((topOfMenuList + maxDisplayItems), displayItems.Length); i++)
            {
                if (displayItems == null || displayItems.Length == 0 ||
                    i < 0 || i > displayItems.Length)
                    continue;

                // if already owned, gray out display and place 'already owned' banner above
                GUI.enabled = (!displayItems[i].playerNowHas);

                // icon
                r.x = 0.1f * w;
                r.width = 0.05f * w;
                r.height = r.width; // square
                t = displayItems[i].icon;
                c = Color.white;
                GUI.color = c;
                GUI.DrawTexture(r, t);
                if (IsItemHeldForPurchase(i) && purchasedBoxIcon)
                {
                    t = purchasedBoxIcon;
                    GUI.DrawTexture(r, t);
                }
                // name
                r.x = 0.175f * w;
                r.width = 0.5f * w;
                r.height = 0.05f * h;
                g = new GUIStyle(GUI.skin.label);
                g.alignment = TextAnchor.MiddleLeft;
                g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
                g.fontStyle = FontStyle.Bold;
                s = displayItems[i].name;
                if (displayItems[i].playerNowHas)
                    s += " (Currently Owned or Upgraded)";
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
                if (menuItemSelection == (i + 0) ||
                    (usingPad && padButtonSelection == (i + 0)))
                {
                    g.normal.textColor = Color.yellow;
                    g.hover.textColor = Color.yellow;
                    g.active.textColor = Color.yellow;
                }
                GUI.Label(r, s, g);
                r.x += 0.0008f * w;
                r.y += 0.001f * w;
                // price
                r.x = 0.8f * w;
                r.width = 0.1f * w;
                r.height = 0.05f * h;
                g = new GUIStyle(GUI.skin.label);
                g.padding = new RectOffset(0, 20, 0, 0);
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
                c = Color.white;
                if (menuItemSelection == (i + 0) ||
                    (usingPad && padButtonSelection == (i + 0)))
                    c = Color.yellow;
                if (invalidPulseTimer > 0f && menuItemSelection == (i + 0))
                {
                    c = Color.red;
                    c *= (invalidPulseTimer * 5f) % 1f;
                }
                g.normal.textColor = c;
                g.hover.textColor = c;
                g.active.textColor = c;
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
        }

        GUI.enabled = true;
        // confirm popup
        if (confirmPopup)
        {
            // box message
            r.x = 0.3f * w;
            r.y = 0.35f * h + (popupProgress * .8f * h);
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
            r.y = 0.55f * h + (popupProgress * .8f * h);
            r.width = 0.1f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.box);
            g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.normal.textColor = Color.white;
            if (usingPad && padClickButton == 0)
                g.normal.textColor = Color.yellow;
            g.hover.textColor = Color.yellow;
            g.active.textColor = Color.white;
            s = "ACCEPT";
            if (GUI.Button(r,s,g) ||
                (usingPad && padClickButton == 0))
            {
                if (confirmType == ConfirmType.Purchase)
                {
                    if (currentPurchaseItem != null)
                    {
                        AddToPurchaseItemsHeld(currentPurchaseItem, currentTradeInItem);
                        purchaseGoldHeld += currentPurchsePrice;
                        // clear current purchase item consideration
                        currentPurchaseItem = null;
                        currentPurchsePrice = 0;
                        currentTradeInItem = null;
                        float rnd = RandomSystem.FlatRandom01();
                        if (rnd < .25f)
                            salesVisit.MenuDialogBeat("An excellent choice. You certainly know your island upgrades.");
                        else if (rnd >= .25f && rnd < .5f)
                            salesVisit.MenuDialogBeat("Clearly, you're a Biomancer who knows what they want.");
                        else if (rnd >= .5f && rnd < .75f)
                            salesVisit.MenuDialogBeat("I see you value quality. You won't be disappointed.");
                        else
                            salesVisit.MenuDialogBeat("A very good choice, and I'm happy to provide this for you.");
                    }
                }
                if (confirmType == ConfirmType.ClearPurchases)
                {
                    ClearPurchaseItemsHeld();
                    float rnd = RandomSystem.FlatRandom01();
                    if (rnd < .25f)
                        salesVisit.MenuDialogBeat("We can start again, that's no problem at all.");
                    else if (rnd >= .25f && rnd < .5f)
                        salesVisit.MenuDialogBeat("Of course, that's fine. I want to make sure you're happy today.");
                    else if (rnd >= .5f && rnd < .75f)
                        salesVisit.MenuDialogBeat("You're a careful Biomancer. I like that in a customer.");
                    else
                        salesVisit.MenuDialogBeat("Take your time. These decisions can be difficult. I understand.");
                }
                if (confirmType == ConfirmType.ComposeIsland)
                {
                    // step off island, etc.
                    LaunchCompositionStage();
                }
                if (confirmType == ConfirmType.CompleteIsland)
                {
                    // temp
                    float rnd = RandomSystem.FlatRandom01();
                    if (rnd < .25f)
                        salesVisit.MenuDialogBeat("It is truly a pleasure to do business with you.");
                    else if (rnd >= .25f && rnd < .5f)
                        salesVisit.MenuDialogBeat("Very well, we shall conclude our business then.");
                    else if (rnd >= .5f && rnd < .75f)
                        salesVisit.MenuDialogBeat("I am happy to have served you well today.");
                    else
                        salesVisit.MenuDialogBeat("Then it's a deal, and a geniune pleasure as well.");
                    pcm.playerData.gold -= purchaseGoldHeld;
                    //
                    ClearPurchaseItemsHeld();
                    // 
                    SignalMenuClose();
                }
                confirmType = ConfirmType.None;
                popupTimer = POPTIME;
            }
            // cancel button
            r.x = 0.55f * w;
            g.normal.textColor = Color.white;
            if (usingPad && padClickButton == 1)
                g.normal.textColor = Color.yellow;
            g.hover.textColor = Color.yellow;
            g.active.textColor = Color.white;
            s = "CANCEL";
            if (GUI.Button(r, s, g) ||
                (usingPad && padClickButton == 1))
            {

                if (confirmType == ConfirmType.Purchase)
                {                
                    // clear current purchase item consideration
                    currentPurchaseItem = null;
                    currentPurchsePrice = 0;
                    currentTradeInItem = null;
                }
                confirmType = ConfirmType.None;
                popupTimer = POPTIME;
            }
        }

        GUI.enabled = true;
        // total purchases cost display
        r.x = 0.075f * w;
        r.y = 0.85f * h;
        r.width = 0.2f * w;
        r.height = 0.1f * h;
        g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt(14 * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleLeft;
        g.normal.textColor = Color.yellow;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.yellow;
        s = "TOTAL GOLD TO COMPLETE OUR BUSINESS: " + purchaseGoldHeld;
        if (purchaseItemsHeld.Length > 0)
            GUI.Label(r, s, g);

        GUI.enabled = !confirmPopup;
        if (!confirmPopup)
        {
            // compose purchases, complete transaction button
            if ((stage == StageOfTransaction.Purchases && purchaseItemsHeld.Length > 0) ||
                (stage == StageOfTransaction.Completion && validConfig))
            {
                r.x = 0.225f * w;
                r.y = 0.9f * h;
                r.width = 0.15f * w;
                r.height = 0.05f * h;
                g = new GUIStyle(GUI.skin.button);
                g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
                //g.fontStyle = FontStyle.Bold;
                g.alignment = TextAnchor.MiddleCenter;
                g.normal.textColor = Color.white;
                if (usingPad && padButtonSelection == padMaxButton)
                    g.normal.textColor = Color.yellow;
                g.hover.textColor = Color.yellow;
                g.active.textColor = Color.white;
                s = "Compose Puchases";
                if (stage == StageOfTransaction.Completion)
                    s = "End Transaction";
                if ((GUI.Button(r, s, g) || (usingPad && padClickButton == padMaxButton)))
                {
                    if (stage == StageOfTransaction.Purchases)
                        ConfirmPopup("Are you sure you're done purchasing\neverything you want today?", ConfirmType.ComposeIsland);
                    else if (stage == StageOfTransaction.Completion)
                        ConfirmPopup("Are you satisfied with our transaction?\nMay we close our business for today?", ConfirmType.CompleteIsland);
                }
            }
        }

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
            GUI.Label(r, s, g);
        }

        GUI.enabled = !confirmPopup;
        if (!confirmPopup && stage == StageOfTransaction.Purchases && purchaseItemsHeld.Length > 0)
        {
            // clear purchase items button
            r.x = 0.775f * w;
            r.y = 0.9f * h;
            r.width = 0.15f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.button);
            g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
            //g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            g.normal.textColor = Color.white;
            if (usingPad && padButtonSelection == padMaxButton)
                g.normal.textColor = Color.yellow;
            g.hover.textColor = Color.yellow;
            g.active.textColor = Color.white;
            s = "Clear All Purchases";
            if (GUI.Button(r, s, g) || (usingPad && padClickButton == padMaxButton))
            {
                ConfirmPopup("Are you sure you want to clear\nall purchases you've considered so far?", ConfirmType.ClearPurchases);
            }
        }

        GUI.enabled = !confirmPopup;
        if (!confirmPopup && stage == StageOfTransaction.Purchases && purchaseItemsHeld.Length == 0)
        {
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
            if (usingPad && padButtonSelection == padMaxButton - 1)
                g.normal.textColor = Color.yellow;
            g.hover.textColor = Color.yellow;
            g.active.textColor = Color.white;
            s = "End Island Upgrades";
            if (GUI.Button(r, s, g) || (usingPad && padClickButton == padMaxButton - 1))
            {
                SignalMenuClose();
            }
        }
    }
}
