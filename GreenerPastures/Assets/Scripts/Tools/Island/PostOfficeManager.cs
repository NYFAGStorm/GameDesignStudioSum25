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
