using UnityEngine;

public class PostOfficeManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles delivery of packages and letter to player mailbox props

    private struct DeliveryBox
    {
        public string playerName;
        public Vector3 mailboxSpot;
    }
    private DeliveryBox[] mailboxes;

    private PlayerMessage[] letterMessages; // player messages saved until read

    private GreenerGameManager ggm;
    private bool mailboxesFound;

    private struct ScheduledMessage
    {
        public int day;
        public WorldMonth month;
        public string sender;
        public string message;
        public PlayerEffect requiredPlayerEffect;
    }
    private ScheduledMessage[] timedMessages;
    private struct ScheduledPackage
    {
        public int day;
        public WorldMonth month;
        public string sender;
        public string message; // letter included with every package
        public ItemData[] packedItems;
    }
    private ScheduledPackage[] timedPackages;
    private int latestDailyDelivery;


    void Start()
    {
        // validate
        ggm = GameObject.FindFirstObjectByType<GreenerGameManager>();
        if (ggm == null)
        {
            Debug.LogError("--- PostOfficeManager [Start] : no game manager found in scene. aborting.");
            enabled = false;
        }
        // initialze
        if (enabled)
        {
            InitializeTimedMessages();
            InitializeTimedPackages();
        }
    }

    void FindMailboxes()
    {
        IslandManager im = GameObject.FindFirstObjectByType<IslandManager>();
        if (im == null)
        {
            Debug.LogError("--- PostOfficeManager [FindMailboxes] : no island manager found in scene. aborting.");
            return;
        }
        mailboxes = new DeliveryBox[ggm.game.players.Length];
        for (int i = 0; i < ggm.game.players.Length; i++)
        {
            mailboxes[i].playerName = ggm.game.players[i].playerName;
            int islandIndex = ggm.game.players[i].playerIsland;
            if (im.islands[islandIndex] != null && im.islands[islandIndex].props != null &&
                im.islands[islandIndex].props.Length > 0)
            {
                PositionData position = im.islands[islandIndex].location;
                bool foundBox = false;
                for (int n = 0; n < im.islands[islandIndex].props.Length; n++)
                {
                    if (im.islands[islandIndex].props[n].type == PropType.Mailbox)
                    {
                        mailboxes[i].mailboxSpot = Vector3.zero;
                        mailboxes[i].mailboxSpot += GameSystem.GetVector(position);
                        mailboxes[i].mailboxSpot += GameSystem.GetVector(im.islands[islandIndex].props[n].location);
                        foundBox = true;
                        break;
                    }
                }            
                if (!foundBox)
                    Debug.LogWarning("--- PostOfficeManager [FindMailboxes] : no mailbox found on island for player '" + ggm.game.players[i].playerName + "'. will ignore.");
            }
            else
                Debug.LogWarning("--- PostOfficeManager [FindMailboxes] : invalid island data or no island props found for player '" + ggm.game.players[i].playerName + "'. will ignore.");
        }
    }

    void InitializeTimedMessages()
    {
        timedMessages = new ScheduledMessage[8];
        int idx = 0;

        timedMessages[idx].day = 1;
        timedMessages[idx].month = WorldMonth.Mar;
        timedMessages[idx].sender = "Eden";
        timedMessages[idx].message = "We're so happy you're here!\n\nEveryone enjoys seeing a new Biomancer's island on the horizon.\n\nRemember you can use dark plants like Moonflower to grow more during the night time.\n\nLove, Eden";
        timedMessages[idx].requiredPlayerEffect = PlayerEffect.EdenLetterOne;
        idx++;

        timedMessages[idx].day = 2;
        timedMessages[idx].month = WorldMonth.Mar;
        timedMessages[idx].sender = "Eden";
        timedMessages[idx].message = "Each new day brings us joy, and you're a part of that now.\n\nRemember you can always use spells to water plants or grow them faster.\n\nLevel up and use the magic table in your tower.\n\nLove, Eden";
        timedMessages[idx].requiredPlayerEffect = PlayerEffect.EdenLetterTwo;
        idx++;

        timedMessages[idx].day = 3;
        timedMessages[idx].month = WorldMonth.Mar;
        timedMessages[idx].sender = "Eden";
        timedMessages[idx].message = "I have good advice for you,\n\nYour Biomancer's Almanac is a wealth of valuable information.\n\nFind time each day to read a little more about the world we live in.\n\nAnd watch for new entries to be unlocked as you level up!\n\nLove, Eden";
        timedMessages[idx].requiredPlayerEffect = PlayerEffect.EdenLetterThree;
        idx++;

        timedMessages[idx].day = 4;
        timedMessages[idx].month = WorldMonth.Mar;
        timedMessages[idx].sender = "Eden";
        timedMessages[idx].message = "Hello Good Friend,\n\nIf your trees and other re-fruiting plants are growing slowly, here's a tip:\n\nDig up the whole plant and drop in fertilizer, then drop the plant back in the renourished soil.'Hope that helps!\n\nLove, Eden";
        timedMessages[idx].requiredPlayerEffect = PlayerEffect.EdenLetterFour;
        idx++;

        timedMessages[idx].day = 5;
        timedMessages[idx].month = WorldMonth.Mar;
        timedMessages[idx].sender = "Eden";
        timedMessages[idx].message = "You Are Loved,\n\nYou are appreciated and recognized. You have more to offer the world. Please create and share. Be well, do good, have fun and take care.\n\nLove, Eden\n\nP.S. - Remember to pay the tax man at the end of the month.";
        timedMessages[idx].requiredPlayerEffect = PlayerEffect.EdenLetterFive;
        idx++;

        timedMessages[idx].day = 6;
        timedMessages[idx].month = WorldMonth.Mar;
        timedMessages[idx].sender = "Eden";
        timedMessages[idx].message = "Biomancer Friend,\n\nMay the sun and rain bless your garden and grow your magic for the Genesis Tree.\n\nKeep in mind the our market has new sale items when you level up and their items change every season.\n\nLove, Eden";
        timedMessages[idx].requiredPlayerEffect = PlayerEffect.EdenLetterSix;
        idx++;

        timedMessages[idx].day = 7;
        timedMessages[idx].month = WorldMonth.Mar;
        timedMessages[idx].sender = "Eden";
        timedMessages[idx].message = "Grace of the Genesis Tree be with you,\n\nKeep in mind that you can hold the fruit of one plant, and graft it to the stalk of another.\n\nIf they are compatible, you'll have a new more rare plant. Happy grafting!\n\nLove, Eden";
        timedMessages[idx].requiredPlayerEffect = PlayerEffect.EdenLetterSeven;
        idx++;
    }

    void InitializeTimedPackages()
    {
        timedPackages = new ScheduledPackage[12];
        int idx = 0;

        timedPackages[idx].day = 14;
        timedPackages[idx].month = WorldMonth.Jan;
        timedPackages[idx].sender = "Mr. Sells Alat";
        timedPackages[idx].message = "It's a New Year and New Moon so come visit the market!\n\nEvery Biomancer needs supplies and we've got what you need!\n\nEnjoy this 50% off coupon good for any one item! See you soon!\n\nMr. Sells Alat";
        timedPackages[idx].packedItems = new ItemData[3];
        timedPackages[idx].packedItems[0] = InventorySystem.InitializeItem(ItemType.Coupon);
        timedPackages[idx].packedItems[0].name = "Coupon (50% off)";
        timedPackages[idx].packedItems[0].quality = .5f;
        timedPackages[idx].packedItems[1] = InventorySystem.SetItemAsPlant(InventorySystem.InitializeItem(ItemType.Fruit), PlantSystem.InitializePlant(PlantType.Rose));
        timedPackages[idx].packedItems[2] = InventorySystem.InitializeItem(ItemType.Letter);
        idx++;

        timedPackages[idx].day = 14;
        timedPackages[idx].month = WorldMonth.Feb;
        timedPackages[idx].sender = "Mr. Sells Alat";
        timedPackages[idx].message = "New Moon Specials at the market!\n\nEverything is special for our valued customers!\n\nEnjoy this coupon for 50% off any item! See you soon!\n\nMr. Sells Alat";
        timedPackages[idx].packedItems = new ItemData[3];
        timedPackages[idx].packedItems[0] = InventorySystem.InitializeItem(ItemType.Coupon);
        timedPackages[idx].packedItems[0].name = "Coupon (50% off)";
        timedPackages[idx].packedItems[0].quality = .5f;
        timedPackages[idx].packedItems[1] = InventorySystem.SetItemAsPlant(InventorySystem.InitializeItem(ItemType.Fruit), PlantSystem.InitializePlant(PlantType.Rose));
        timedPackages[idx].packedItems[2] = InventorySystem.InitializeItem(ItemType.Letter);
        idx++;

        timedPackages[idx].day = 14;
        timedPackages[idx].month = WorldMonth.Mar;
        timedPackages[idx].sender = "Mr. Sells Alat";
        timedPackages[idx].message = "Visit the market during the New Moon!\n\nWe've got everything a Biomancer needs to nurture their farm and craft their magic!\n\nEnjoy this complementary coupon worth 50% off any one item! See you soon!\n\nMr. Sells Alat";
        timedPackages[idx].packedItems = new ItemData[3];
        timedPackages[idx].packedItems[0] = InventorySystem.InitializeItem(ItemType.Coupon);
        timedPackages[idx].packedItems[0].name = "Coupon (50% off)";
        timedPackages[idx].packedItems[0].quality = .5f;
        timedPackages[idx].packedItems[1] = InventorySystem.SetItemAsPlant(InventorySystem.InitializeItem(ItemType.Fruit), PlantSystem.InitializePlant(PlantType.Rose));
        timedPackages[idx].packedItems[2] = InventorySystem.InitializeItem(ItemType.Letter);
        idx++;

        timedPackages[idx].day = 14;
        timedPackages[idx].month = WorldMonth.Apr;
        timedPackages[idx].sender = "Mr. Sells Alat";
        timedPackages[idx].message = "New Moon nights are dark, but market is your bright light!\n\nCome one by and get your magic and farming supplies!\n\nPlease accept this 50% discount coupon for any one item! See you soon!\n\nMr. Sells Alat";
        timedPackages[idx].packedItems = new ItemData[3];
        timedPackages[idx].packedItems[0] = InventorySystem.InitializeItem(ItemType.Coupon);
        timedPackages[idx].packedItems[0].name = "Coupon (50% off)";
        timedPackages[idx].packedItems[0].quality = .5f;
        timedPackages[idx].packedItems[1] = InventorySystem.SetItemAsPlant(InventorySystem.InitializeItem(ItemType.Fruit), PlantSystem.InitializePlant(PlantType.Rose));
        timedPackages[idx].packedItems[2] = InventorySystem.InitializeItem(ItemType.Letter);
        idx++;

        timedPackages[idx].day = 14;
        timedPackages[idx].month = WorldMonth.May;
        timedPackages[idx].sender = "Mr. Sells Alat";
        timedPackages[idx].message = "Joyful New Moon to all our happy customers!\n\nEvery Biomancer needs supplies and we've got everything you need at the market!\n\nBring this coupon with you and get 50% off any item! See you soon!\n\nMr. Sells Alat";
        timedPackages[idx].packedItems = new ItemData[3];
        timedPackages[idx].packedItems[0] = InventorySystem.InitializeItem(ItemType.Coupon);
        timedPackages[idx].packedItems[0].name = "Coupon (50% off)";
        timedPackages[idx].packedItems[0].quality = .5f;
        timedPackages[idx].packedItems[1] = InventorySystem.SetItemAsPlant(InventorySystem.InitializeItem(ItemType.Fruit), PlantSystem.InitializePlant(PlantType.Rose));
        timedPackages[idx].packedItems[2] = InventorySystem.InitializeItem(ItemType.Letter);
        idx++;

        timedPackages[idx].day = 14;
        timedPackages[idx].month = WorldMonth.Jun;
        timedPackages[idx].sender = "Mr. Sells Alat";
        timedPackages[idx].message = "New Moon means gifts for all our valued customers!\n\nCome on into the market where we have all you need!\n\nTake this 50% off coupon as our way of saying we value you! See you soon!\n\nMr. Sells Alat";
        timedPackages[idx].packedItems = new ItemData[3];
        timedPackages[idx].packedItems[0] = InventorySystem.InitializeItem(ItemType.Coupon);
        timedPackages[idx].packedItems[0].name = "Coupon (50% off)";
        timedPackages[idx].packedItems[0].quality = .5f;
        timedPackages[idx].packedItems[1] = InventorySystem.SetItemAsPlant(InventorySystem.InitializeItem(ItemType.Fruit), PlantSystem.InitializePlant(PlantType.Rose));
        timedPackages[idx].packedItems[2] = InventorySystem.InitializeItem(ItemType.Letter);
        idx++;

        timedPackages[idx].day = 14;
        timedPackages[idx].month = WorldMonth.Jul;
        timedPackages[idx].sender = "Mr. Sells Alat";
        timedPackages[idx].message = "Happy New Moon!\n\nWhether you need special crafting items or just need to sell you wares, the market is the place to be!\n\nGet 50% off any one item with this coupon! See you soon!\n\nMr. Sells Alat";
        timedPackages[idx].packedItems = new ItemData[3];
        timedPackages[idx].packedItems[0] = InventorySystem.InitializeItem(ItemType.Coupon);
        timedPackages[idx].packedItems[0].name = "Coupon (50% off)";
        timedPackages[idx].packedItems[0].quality = .5f;
        timedPackages[idx].packedItems[1] = InventorySystem.SetItemAsPlant(InventorySystem.InitializeItem(ItemType.Fruit), PlantSystem.InitializePlant(PlantType.Rose));
        timedPackages[idx].packedItems[2] = InventorySystem.InitializeItem(ItemType.Letter);
        idx++;

        timedPackages[idx].day = 14;
        timedPackages[idx].month = WorldMonth.Aug;
        timedPackages[idx].sender = "Mr. Sells Alat";
        timedPackages[idx].message = "A New Moon means savings down at the market!\n\nWe value each of our Biomancer customers, and aim to make sure you get all you need!\n\nUse this 50% off coupon during your next visit!\n\nMr. Sells Alat";
        timedPackages[idx].packedItems = new ItemData[3];
        timedPackages[idx].packedItems[0] = InventorySystem.InitializeItem(ItemType.Coupon);
        timedPackages[idx].packedItems[0].name = "Coupon (50% off)";
        timedPackages[idx].packedItems[0].quality = .5f;
        timedPackages[idx].packedItems[1] = InventorySystem.SetItemAsPlant(InventorySystem.InitializeItem(ItemType.Fruit), PlantSystem.InitializePlant(PlantType.Rose));
        timedPackages[idx].packedItems[2] = InventorySystem.InitializeItem(ItemType.Letter);
        idx++;

        timedPackages[idx].day = 14;
        timedPackages[idx].month = WorldMonth.Sep;
        timedPackages[idx].sender = "Mr. Sells Alat";
        timedPackages[idx].message = "The New Moon is here!\n\nA happy farm and an active magic crafting table come from goods you'll find at the market!\n\nEnjoy this complementary coupon worth 50% off any one item! See you soon!\n\nMr. Sells Alat";
        timedPackages[idx].packedItems = new ItemData[3];
        timedPackages[idx].packedItems[0] = InventorySystem.InitializeItem(ItemType.Coupon);
        timedPackages[idx].packedItems[0].name = "Coupon (50% off)";
        timedPackages[idx].packedItems[0].quality = .5f;
        timedPackages[idx].packedItems[1] = InventorySystem.SetItemAsPlant(InventorySystem.InitializeItem(ItemType.Fruit), PlantSystem.InitializePlant(PlantType.Rose));
        timedPackages[idx].packedItems[2] = InventorySystem.InitializeItem(ItemType.Letter);
        idx++;

        timedPackages[idx].day = 14;
        timedPackages[idx].month = WorldMonth.Oct;
        timedPackages[idx].sender = "Mr. Sells Alat";
        timedPackages[idx].message = "Visit the market during the New Moon!\n\nOur market menu includes all the items a thriving Biomancer needs!\n\nTake this 50% discount coupon as our way of saying 'thank you'! See you soon!\n\nMr. Sells Alat";
        timedPackages[idx].packedItems = new ItemData[3];
        timedPackages[idx].packedItems[0] = InventorySystem.InitializeItem(ItemType.Coupon);
        timedPackages[idx].packedItems[0].name = "Coupon (50% off)";
        timedPackages[idx].packedItems[0].quality = .5f;
        timedPackages[idx].packedItems[1] = InventorySystem.SetItemAsPlant(InventorySystem.InitializeItem(ItemType.Fruit), PlantSystem.InitializePlant(PlantType.Rose));
        timedPackages[idx].packedItems[2] = InventorySystem.InitializeItem(ItemType.Letter);
        idx++;

        timedPackages[idx].day = 14;
        timedPackages[idx].month = WorldMonth.Nov;
        timedPackages[idx].sender = "Mr. Sells Alat";
        timedPackages[idx].message = "We love the New Moon!\n\nThe dark nights are a perfect time to craft magic, and we have all the ingredients you need!\n\nEnjoy this coupon worth 50% off any one item! See you soon!\n\nMr. Sells Alat";
        timedPackages[idx].packedItems = new ItemData[3];
        timedPackages[idx].packedItems[0] = InventorySystem.InitializeItem(ItemType.Coupon);
        timedPackages[idx].packedItems[0].name = "Coupon (50% off)";
        timedPackages[idx].packedItems[0].quality = .5f;
        timedPackages[idx].packedItems[1] = InventorySystem.SetItemAsPlant(InventorySystem.InitializeItem(ItemType.Fruit), PlantSystem.InitializePlant(PlantType.Rose));
        timedPackages[idx].packedItems[2] = InventorySystem.InitializeItem(ItemType.Letter);
        idx++;

        timedPackages[idx].day = 14;
        timedPackages[idx].month = WorldMonth.Dec;
        timedPackages[idx].sender = "Mr. Sells Alat";
        timedPackages[idx].message = "The New Moon means big deals at the market!\n\nCome visit the market and see what we have for you!\n\nBring this complementary coupon that gives you a 50% off any one item! See you soon!\n\nMr. Sells Alat";
        timedPackages[idx].packedItems = new ItemData[3];
        timedPackages[idx].packedItems[0] = InventorySystem.InitializeItem(ItemType.Coupon);
        timedPackages[idx].packedItems[0].name = "Coupon (50% off)";
        timedPackages[idx].packedItems[0].quality = .5f;
        timedPackages[idx].packedItems[1] = InventorySystem.SetItemAsPlant(InventorySystem.InitializeItem(ItemType.Fruit), PlantSystem.InitializePlant(PlantType.Rose));
        timedPackages[idx].packedItems[2] = InventorySystem.InitializeItem(ItemType.Letter);
        idx++;
    }

    void Update()
    {
        if (!mailboxesFound && ggm.isGameDataDistributed())
            FindMailboxes();
    }

    /// <summary>
    /// Triggers post office to revise mailbox data for current player roster
    /// </summary>
    public void UpdateMailboxes()
    {
        mailboxesFound = false;
    }

    /// <summary>
    /// Triggers post office timed messages to be sent to players
    /// </summary>
    public void DailyDelivery( int day, WorldMonth month )
    {
        if (day <= latestDailyDelivery)
            return;

        // send player effect messages to players who have not had them sent yet
        for (int i = 0; i < ggm.game.players.Length; i++)
        {
            bool letterSent = false; // one letter from Eden per day for new players
            for (int n = 0; n < ggm.game.players[i].effects.Length; n++)
            {
                if (letterSent)
                    continue;
                if (ggm.game.players[i].effects[n] <= PlayerEffect.EdenLetterSeven)
                {
                    for (int t = 0; t < timedMessages.Length; t++)
                    {
                        if (letterSent)
                            continue;
                        if (PlayerSystem.PlayerHasEffect(ggm.game.players[i], timedMessages[t].requiredPlayerEffect))
                        {
                            // send letter
                            string pName = ggm.game.players[i].playerName;
                            SendLetter(timedMessages[t].sender, pName, timedMessages[t].message);
                            // remove effect from this player
                            ggm.game.players[i] = PlayerSystem.RemovePlayerEffect(ggm.game.players[i], timedMessages[t].requiredPlayerEffect);
                            // continue to next player (additional letters saved for another day)
                            letterSent = true;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < timedMessages.Length; i++)
        {
            // ignore messages that require player effect (already handled above)
            if (timedMessages[i].requiredPlayerEffect == PlayerEffect.Default && 
                timedMessages[i].day == day && timedMessages[i].month == month)
            {
                // send message in a letter to all players
                for (int n = 0; n < ggm.game.players.Length; n++)
                {
                    string pName = ggm.game.players[n].playerName;
                    SendLetter(timedMessages[i].sender, pName, timedMessages[i].message);
                }
            }
        }

        for (int i = 0; i < timedPackages.Length; i++)
        {
            if (timedPackages[i].day == day && timedPackages[i].month == month)
            {
                // pack letter and send package
                for (int n = 0; n < ggm.game.players.Length; n++)
                {
                    string pName = ggm.game.players[n].playerName;
                    ItemData[] items = timedPackages[i].packedItems;
                    items[2].name = "Letter (from '" + timedPackages[i].sender + "')";
                    items[2].quality = StoreLetterMessage(GameSystem.GetProfileIDOfPlayer(ggm.game, pName), timedPackages[i].sender, pName, timedPackages[i].message);
                    SendPackage(timedPackages[i].sender, pName, items);
                }
            }
        }

        latestDailyDelivery = day;

        return;
    }

    int GetPlayerAddress( string playerName )
    {
        int retInt = -1;

        for (int i = 0; i < mailboxes.Length; i++)
        {
            if (mailboxes[i].playerName == playerName)
            {
                retInt = i;
                break;
            }
        }
        return retInt;
    }

    /// <summary>
    /// Sends a package item to a given player with a given array of items
    /// </summary>
    /// <param name="sender">sender name</param>
    /// <param name="playerName">recipient player name</param>
    /// <param name="items">an array of item data to pack into the package</param>
    public void SendPackage(string sender, string playerName, ItemData[] items)
    {
        LooseItemData package = InventorySystem.CreateItem(ItemType.Package);
        ItemData iData = package.inv.items[0];
        iData.name = "Package (from " + sender + ")";
        package.inv = InventorySystem.InitializeInventory(1 + items.Length);
        package.inv.items = new ItemData[1 + items.Length];
        package.inv.items[0] = iData;
        for (int i = 0; i < items.Length; i++)
        {
            package.inv.items[i + 1] = items[i];
        }
        Deliver(mailboxes[GetPlayerAddress(playerName)], package);
    }

    /// <summary>
    /// Sends a letter to a given player with a given text message
    /// </summary>
    /// <param name="sender">sender name</param>
    /// <param name="playerName">recipient player name</param>
    /// <param name="message">the text message to display in the letter when opened</param>
    public void SendLetter(string sender, string playerName, string message)
    {
        string playerProfileID = GameSystem.GetProfileIDOfPlayer(ggm.game, playerName);
        if (playerProfileID == "")
        {
            Debug.LogWarning("--- PostOfficeManager [SendLetter] : no player ID found for '" + playerName + "'. will ignore.");
            return;
        }
        LooseItemData letter = InventorySystem.CreateItem(ItemType.Letter);
        letter.inv.items[0].name = "Letter (from '" + sender + "')";
        letter.inv.items[0].quality = StoreLetterMessage(playerProfileID, sender, playerName, message);
        Deliver(mailboxes[GetPlayerAddress(playerName)], letter);
    }

    float StoreLetterMessage(string profID, string sender, string recipient, string message)
    {
        float retFloat = 0f;
        bool noUniqueID = true;
        while (noUniqueID)
        {
            retFloat = RandomSystem.FlatRandom01(); // TEST:
            noUniqueID = false;
            for (int i = 0; i < letterMessages.Length; i++)
            {
                if (letterMessages[i].messageID == retFloat)
                    noUniqueID = true;
            }
        }
        PlayerMessage[] tmp = new PlayerMessage[letterMessages.Length + 1];
        for (int i = 0; i < letterMessages.Length; i++)
        {
            tmp[i] = letterMessages[i];
        }
        tmp[letterMessages.Length] = new PlayerMessage();
        tmp[letterMessages.Length].recipientProfileID = profID;
        tmp[letterMessages.Length].messageID = retFloat;
        tmp[letterMessages.Length].sender = sender;
        tmp[letterMessages.Length].recipient = recipient;
        tmp[letterMessages.Length].message = message;
        letterMessages = tmp;

        return retFloat;
    }

    void Deliver(DeliveryBox mailbox, LooseItemData package)
    {
        Vector3 dropSpot = mailbox.mailboxSpot;
        dropSpot.x += RandomSystem.GaussianRandom01() - 0.5f;
        dropSpot.z += (0.5f * RandomSystem.GaussianRandom01()) - 0.25f;
        ItemSpawnManager ism = GameObject.FindFirstObjectByType<ItemSpawnManager>();
        ism.SpawnItem(package, mailbox.mailboxSpot, dropSpot, true);
    }

    /// <summary>
    /// Returns the stored letter message matching the given message ID
    /// </summary>
    /// <param name="messageID">message ID number</param>
    /// <returns>the letter message, or empty string if not found</returns>
    public string GetLetterMessage( float messageID )
    {
        string retString = "";

        bool found = false;
        for (int i = 0; i < letterMessages.Length; i++)
        {
            if (letterMessages[i].messageID == messageID)
            {
                retString = letterMessages[i].message;
                found = true;
                break;
            }

        }
        if (!found)
            Debug.LogWarning("--- PostOfficeManager [GetLetterMessage] : no letter message of message ID '' found. will return empty message.");

        return retString;
    }

    /// <summary>
    /// Gets the letter messages from the post office for data storage
    /// </summary>
    /// <returns>the array of player messages</returns>
    public PlayerMessage[] GetPlayerMessages()
    {
        return letterMessages;
    }

    /// <summary>
    /// Sets the post office letter messages to players
    /// </summary>
    /// <param name="messages">an array of player message data</param>
    public void SetPlayerMessages( PlayerMessage[] messages )
    {
        letterMessages = messages;
    }
}
