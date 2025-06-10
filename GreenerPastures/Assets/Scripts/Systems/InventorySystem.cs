// REVIEW: necessary namespaces

public static class InventorySystem
{
    // NOTES:
    // Inventory defines containers for items with a maximum number of item slots
    // Items are distinct types of game elements, only exist in an inventory
    // Items can be transferred from one inventory to another, using StoreItem()
    // Loose Items are those items that exist in the game world, on game objects
    // Loose Items have an inventory of size 1
    // Loose Item data is created using CreateItem()
    // If item is stored from Loose Item, the Loose Item game object will be destroyed
    // An item is dropped from inventory as a Loose Item using DropItem()
    // An item is taken from the game world as a Loose Item using TakeItem()
    // An item in inventory may be destroyed by removing from its inventory
    // A Loose Item may be destroyed by simply destroying the game object
    // SUMMARY:
    // CreateItem() makes data for an item to live in the game world (a loose item)
    // TakeItem() transfers an item from the game world to an inventory
    // StoreItem() transfer an item from one inventory to another
    // DropItem() removes an item from an inventory and creates loose item data
    // several common functions exist to manage items in inventory data
    // an ItemManager tool will be used to handle each loose item game object

    public const int DEFAULTMAXSLOTS = 1;

    /// <summary>
    /// Creates an item as a loose item in the game world, by given type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static LooseItemData CreateItem(ItemType type)
    {
        LooseItemData retLooseItem = new LooseItemData();

        retLooseItem.inv = InitializeInventory(1);
        retLooseItem.inv = AddToInventory(retLooseItem.inv, InitializeItem(type));

        return retLooseItem;
    }

    /// <summary>
    /// Transfer loose item from game world into an inventory, if empty slot available
    /// </summary>
    /// <param name="item">loose item data</param>
    /// <param name="retItem">result loose item data (use same reference)</param>
    /// <param name="inv">inventory data</param>
    /// <returns>inventory data with item added, if loose item and empty slot existed</returns>
    public static InventoryData TakeItem(LooseItemData item, out LooseItemData retItem, InventoryData inv)
    {
        InventoryData retInv = inv;
        retItem = item;

        // validate (item exists)
        if (item == null || item.inv.items[0] == null)
            return retInv;
        // store loose item into inventory
        if (StoreItem(item.inv.items[0], item.inv, inv, out retItem.inv, out retInv))
            retItem.deleteMe = true; // flag empty loose item for deletion
        else
            UnityEngine.Debug.LogWarning("--- InventorySystem [TakeItem] : no empty slot available. will ignore item take.");

        return retInv;
    }

    /// <summary>
    /// Transfers an item from one inventory to another, if exists in from inventory
    /// </summary>
    /// <param name="item">item data</param>
    /// <param name="from">inventory data to transfer from</param>
    /// <param name="to">inventory data to transfer to</param>
    /// <param name="retFrom">result from inventory data (use same reference)</param>
    /// <param name="retTo">result to inventory data (use same reference)</param>  
    /// <returns>true if succeeded, false if failed (no empty slot available in 'to' inv)</returns>
    public static bool StoreItem(ItemData item, InventoryData from, InventoryData to,
        out InventoryData retFrom, out InventoryData retTo)
    {
        bool retBool = InvHasSlot(to);

        retFrom = from;
        retTo = to;

        if (retBool)
        {
            retTo = AddToInventory(to, item);
            retFrom = RemoveItemFromInventory(from, item);
        }

        return retBool;
    }

    /// <summary>
    /// Creates a loose item for an item removed from an inventory to the game world
    /// </summary>
    /// <param name="item">item data</param>
    /// <param name="inv">inventory data</param>
    /// <param name="retInv">result inventory data (use same reference)</param>
    /// <returns>initialized loose item data, configured for this item</returns>
    public static LooseItemData DropItem( ItemData item, InventoryData inv, out InventoryData retInv )
    {
        LooseItemData retLooseItem = new LooseItemData();
        retLooseItem.inv = InitializeInventory(1);
        retInv = inv;

        // transfer item from inventory to game world as new loose item
        StoreItem(item, inv, retLooseItem.inv, out retInv, out retLooseItem.inv);
        // game world properties to be assigned by tool (position, flipped, etc)

        return retLooseItem;
    }

    /// <summary>
    /// Creates a single instance of an item, by given item type (inventory only)
    /// </summary>
    /// <param name="type">item type</param>
    /// <returns>initialized item data</returns>
    public static ItemData InitializeItem(ItemType type)
    {
        ItemData retItem = new ItemData();

        // initialize
        retItem.name = type.ToString();
        retItem.type = type;
        retItem.plantIndex = -1; // no plant by default
        retItem.size = 1f;
        retItem.health = 1f;
        retItem.quality = 1f;
        retItem.effects = new ItemEffects[0];

        return retItem;
    }

    /// <summary>
    /// Configures the properties of a specific plant to this item
    /// </summary>
    /// <param name="item">item data</param>
    /// <param name="plant">plant data</param>
    /// <returns>item data with plant configurations</returns>
    public static ItemData SetItemAsPlant(ItemData item, PlantData plant)
    {
        ItemData retItem = item;

        retItem.name += " (" + plant.plantName + ")";
        retItem.plantIndex = (int)plant.type;
        retItem.size = plant.growth;
        retItem.health = plant.health;
        retItem.quality = plant.quality;
        // TODO: transfer plant effects to item somehow (need mirror item effects?)

        return retItem;
    }

    /// <summary>
    /// Creates an inventory
    /// </summary>
    /// <returns>initialized inventory data</returns>
    public static InventoryData InitializeInventory()
    {
        InventoryData retInv = new InventoryData();

        // initialize
        retInv = new InventoryData();
        retInv.maxSlots = DEFAULTMAXSLOTS;
        retInv.items = new ItemData[0];

        return retInv;
    }

    /// <summary>
    /// Creates an inventory with the given number of item slots
    /// </summary>
    /// <param name="maxSlots">number of maximum item slots (size), minimum 1</param>
    /// <returns>initialized inventory data</returns>
    public static InventoryData InitializeInventory(int maxSlots)
    {
        InventoryData retInv = InitializeInventory();

        // validate max slots more than zero
        if ( maxSlots <= 0 )
            return retInv;
        retInv = SetMaxSlots(retInv, maxSlots);

        return retInv;
    }

    /// <summary>
    /// Get the maximum number of item slots in inventory (size)
    /// </summary>
    /// <param name="inv">inventory data</param>
    /// <returns>the max number of item slots</returns>
    public static int GetMaxSlots(InventoryData inv)
    {
        return inv.maxSlots;
    }

    // REVIEW: handle items prphaned from decreased inventory somehow?
    /// <summary>
    /// Set the maximum number of item slots in inventory (size)
    /// (NOTE: fails if decrease max item slots, use new inventory instead)
    /// </summary>
    /// <param name="inv">inventory data</param>
    /// <param name="max">maximum slots (size)</param>
    /// <returns>inventory data with revised max slots</returns>
    public static InventoryData SetMaxSlots(InventoryData inv, int max)
    {
        InventoryData retInv = inv;

        // validate (max <= current maxSlots)
        if ( inv.maxSlots > max )
            return retInv;
        // set
        inv.maxSlots = max;

        return retInv;
    }

    /// <summary>
    /// Inventory has an empty item slot?
    /// </summary>
    /// <param name="inv">inventory data</param>
    /// <returns>returns true if at least one item slot is empty</returns>
    public static bool InvHasSlot(InventoryData inv)
    {
        return (GetEmptySlotsInInv(inv) > 0);
    }

    /// <summary>
    /// Gets number of available item slots in inventory
    /// </summary>
    /// <param name="inv">inventory data</param>
    /// <returns>number of empty slots</returns>
    public static int GetEmptySlotsInInv(InventoryData inv)
    {
        return (inv.maxSlots - inv.items.Length);
    }

    /// <summary>
    /// Does inventory have at least one item of given type?
    /// </summary>
    /// <param name="inv">inventory data</param>
    /// <param name="type">item type</param>
    /// <returns>returns true if at least one item instance of this type exists</returns>
    public static bool InvHasItemOfType(InventoryData inv, ItemType type)
    {
        bool found = false;

        for (int i=0; i<inv.items.Length; i++)
        {
            if (inv.items[i].type == type)
            {
                found = true;
                break;
            }
        }

        return found;
    }

    /// <summary>
    /// Does inventory have a specific item instance?
    /// </summary>
    /// <param name="inv">inventory data</param>
    /// <param name="instance">item data</param>
    /// <returns>returns true if inventory holds this item instance</returns>
    public static bool InvHasItemInstance(InventoryData inv, ItemData instance)
    {
        bool found = false;

        for (int i=0; i <inv.items.Length; i++)
        {
            if (inv.items[i] == instance)
            {
                found = true;
                break;
            }
        }

        return found;
    }

    /// <summary>
    /// Adds an item to inventory
    /// </summary>
    /// <param name="inv">inventory data</param>
    /// <param name="item">item data</param>
    /// <returns>inventory data, with item added if empty slot was available</returns>
    public static InventoryData AddToInventory(InventoryData inv, ItemData item)
    {
        InventoryData retInv = inv;

        // validate (empty slot available)
        if (GetEmptySlotsInInv(inv) < 1)
        {
            // be noisy when this fails, no empty slot available
            UnityEngine.Debug.LogWarning("--- InventorySystem [AddToInventory] : no empty slot available. will ignore (item lost).");
            return retInv;
        }
        // add item
        ItemData[] tmp = new ItemData[retInv.items.Length + 1];
        for (int i=0; i<retInv.items.Length; i++)
        {
            tmp[i] = retInv.items[i];
        }
        tmp[retInv.items.Length] = item;
        retInv.items = tmp;

        return retInv;
    }

    /// <summary>
    /// Removes the first instance of an item from inventory, by item type
    /// </summary>
    /// <param name="inv">inventory data</param>
    /// <param name="type">item type</param>
    /// <returns>inventory data with item removed, if any item of type existed</returns>
    public static InventoryData RemoveFromInventory(InventoryData inv, ItemType type)
    {
        InventoryData retInv = inv;

        // validate (any item of type exists)
        if (!InvHasItemOfType(retInv,type))
            return retInv;
        // remove item
        int count = 0;
        bool removed = false;
        ItemData[] tmp = new ItemData[retInv.items.Length-1];
        for (int i=0;i<retInv.items.Length;i++)
        {
            if (removed || retInv.items[i].type != type)
            {
                tmp[count] = retInv.items[i];
                count++;
            }
            else
                removed = true;
        }
        retInv.items = tmp;

        return retInv;
    }

    /// <summary>
    /// Removes an instance of an item from inventory, by specific instance
    /// </summary>
    /// <param name="inv">inventory data</param>
    /// <param name="item">item data</param>
    /// <returns>inventory data, with item removed if existed in inventory</returns>
    public static InventoryData RemoveItemFromInventory(InventoryData inv, ItemData item)
    {
        InventoryData retInv = inv;

        bool found = false;
        // validate (exists in inventory)
        for (int i = 0; i < retInv.items.Length; i++)
        {
            if (retInv.items[i] == item)
            {
                found = true;
                break;
            }
        }
        if (!found)
            return retInv;
        // remove item
        int count = 0;
        ItemData[] tmp = new ItemData[retInv.items.Length - 1];
        for (int i = 0; i < retInv.items.Length; i++)
        {
            if (retInv.items[i] != item)
            {
                tmp[count] = retInv.items[i];
                count++;
            }
        }
        retInv.items = tmp;

        return retInv;
    }
}
