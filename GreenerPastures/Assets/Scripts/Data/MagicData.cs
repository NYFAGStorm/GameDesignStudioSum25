// REVIEW: necessary namespaces

// VOCABULARY
// Magic system results in SPELLS the player has CAST into the world
// Each spell cast into the world takes a single CHARGE of that spell
// Charges of spells are stored in a player's SPELL BOOK
// One or more charges of a spell can be stored in a player's spell book
// To acquire a new charge of a spell in their spell book, players CRAFT the spell
// Crafting the spell requires the player work the MAGIC TABLE and spell INGREDIENTS
// Ingredients are items the player has in their inventory
// The spell RECIPE, listing the ingredients needed to craft charges of a spell are
//    in a player's GRIMOIRE, which is like an encyclopedia reference of spells
// The player reveals (learns) new spell recipes in their grimoire when leveling up
// The magic table interface displays the grimoire recipe they have chosen to craft
// Along with the name of the spell, the recipe displays ingredient inventory items
// The player can drag and drop ingredients from the recipe to the crafting CAULDRON
// If the player fits the ingredient pieces into the cauldron puzzle space, 
//    the spell charge is crafted and stored in the spell book, and the items disappear

// as we develop, continue to add statistics to keep regarding magic
[System.Serializable]
public class MagicStats
{
    public float totalMagicTime;
    public int totalRecipesLearned;
    public int totalIngredientsUsed;
    public int totalChargesCrafted;
    public int totalChargesCast;
}

// master spell list
public enum SpellType
{
    Default,
    SpellA,
    SpellB,
    SpellC,
    SpellD
}

// a grimiore holds a player's list of learned spells available to make charges
[System.Serializable]
public class GrimioreData
{
    public string name;
    public string description;
    public SpellType type;
    public ItemType[] ingredients; // REVIEW: only need one item of each ingredient?
}

// spell books hold the spell charges a player has crafted and are available to cast
[System.Serializable]
public class SpellBookData
{
    public string name;
    public SpellType type;
    public int chargesAvailable;
    public float cooldownDuration;
    public long cooldownTimestamp; // tracked cooldown max is a timestamp (future game time)
    public float cooldown; // tracked cooldown max is cooldown duration
    public float castDuration;
    public float castAOE; // range (radius) of area of effect
}

// the spell library is both the grimiore and spell book for a single player
// a grimiore is a list of the spells a player has learned the recipe for
// a spell book is a collection of crafted spells charges for a player
[System.Serializable]
public class SpellLibrary
{
    public GrimioreData[] grimiore;
    public SpellBookData[] spellBook;
}

// casts hold the details of a single spell charge a player has cast into the world
[System.Serializable]
public class CastData
{
    public SpellType type;
    public long lifeTimestamp; // saved lifetime max is a timestamp (future game time)
    public float lifetime; // tracked lifetime max is cast duration
    public float posX;
    public float posY;
    public float posZ;
    public float rangeAOE;
}

// magic is one player's current magic, including learned spells and casts in effect
[System.Serializable]
public class MagicData
{
    public MagicStats stats;
    public SpellLibrary library;
}
