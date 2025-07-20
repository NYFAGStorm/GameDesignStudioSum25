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

    private struct ScheduledMessages
    {
        public int day;
        public WorldMonth month;
        public string sender;
        public string message;
    }
    private ScheduledMessages[] timedMessages;
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
        timedMessages = new ScheduledMessages[5];
        int idx = 0;

        timedMessages[idx].day = 1;
        timedMessages[idx].month = WorldMonth.Mar;
        timedMessages[idx].sender = "Eden";
        timedMessages[idx].message = "We're so happy you're here!\n\nEveryone enjoys seeing a new Biomancer's island on the horizon.\n\nRemember you can use dark plants like Moonflower to grow more during the night time.\n\nLove, Eden";
        idx++;

        timedMessages[idx].day = 2;
        timedMessages[idx].month = WorldMonth.Mar;
        timedMessages[idx].sender = "Eden";
        timedMessages[idx].message = "Each new day brings us joy, and you're a part of that now.\n\nRemember you can always use spells to water plants or grow them faster.\n\nLevel up and use the magic table in your tower.\n\nLove, Eden";
        idx++;

        timedMessages[idx].day = 3;
        timedMessages[idx].month = WorldMonth.Mar;
        timedMessages[idx].sender = "Eden";
        timedMessages[idx].message = "Hello Good Friend,\n\nIf your trees and other re-fruiting plants are growing slowly, here's a tip:\n\nDig up the whole plant and drop in fertilizer, then drop the plant back in the renourished soil.'Hope that helps!\n\nLove, Eden";
        idx++;

        timedMessages[idx].day = 4;
        timedMessages[idx].month = WorldMonth.Mar;
        timedMessages[idx].sender = "Eden";
        timedMessages[idx].message = "You Are Loved,\n\nYou are appreciated and recognized. You have more to offer the world. Please create and share. Be well, do good, have fun and take care.\n\nLove, Eden\n\nP.S. - Remember to pay the tax man at the end of the month.";
        idx++;

        timedMessages[idx].day = 5;
        timedMessages[idx].month = WorldMonth.Mar;
        timedMessages[idx].sender = "Eden";
        timedMessages[idx].message = "Biomancer Friend,\n\nMay the sun and rain bless your garden and grow your magic for the Genesis Tree.\n\nKeep in mind the our market has new sale items when you level up and their items change every season.\n\nLove, Eden";
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

        for (int i = 0; i < timedMessages.Length; i++)
        {
            if (timedMessages[i].day == day && timedMessages[i].month == month)
            {
                // send message in a letter to all players
                for (int n = 0; n < ggm.game.players.Length; n++)
                {
                    string pName = ggm.game.players[n].playerName;
                    SendLetter(timedMessages[i].sender, pName, timedMessages[i].message);
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
