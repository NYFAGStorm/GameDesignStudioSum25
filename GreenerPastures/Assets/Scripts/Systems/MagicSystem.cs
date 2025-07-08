// REVIEW: necessary namespaces

public static class MagicSystem
{
    /// <summary>
    /// Creates new magic data for one player, including their spell library
    /// </summary>
    /// <returns>initialized magic data</returns>
    public static MagicData IntializeMagic()
    {
        MagicData retMagic = new MagicData();

        // initialize
        retMagic.stats = new MagicStats();
        retMagic.library = new SpellLibrary();
        retMagic.library.grimiore = new GrimioreData[0];
        retMagic.library.spellBook = new SpellBookData[0];

        return retMagic;
    }

    /// <summary>
    /// Creates a cast from the spell data in a player's spell book to the world
    /// </summary>
    /// <param name="spell">spell book data from a player's spell libray</param>
    /// /// <param name="position">the game world position this cast is centered</param>
    /// <returns>initialized cast data</returns>
    public static CastData InitializeCast(SpellBookData spell, UnityEngine.Vector3 position)
    {
        CastData retCast = new CastData();

        retCast.type = spell.type;
        retCast.lifetime = spell.castDuration;
        retCast.posX = position.x;
        retCast.posY = position.y;
        retCast.posZ = position.z;
        retCast.rangeAOE = spell.castAOE;

        return retCast;
    }

    /// <summary>
    /// Returns true if spell book has at least one charge of the given spell type
    /// </summary>
    /// <param name="spell">spell type</param>
    /// <param name="library">player's spell library</param>
    /// <returns>true if at least one charge exists, false if not</returns>
    public static bool SpellBookHasCharge( SpellType spell, SpellLibrary library )
    {
        bool retBool = false;

        if (library.spellBook == null || library.spellBook.Length == 0)
            return retBool;

        for (int i = 0; i < library.spellBook.Length; i++)
        {
            if (library.spellBook[i].type == spell)
            {
                retBool = (library.spellBook[i].chargesAvailable > 0);
                break;
            }
        }

        return retBool;
    }

    /// <summary>
    /// Attempts to cast a spell from a player's spell book 
    /// </summary>
    /// <param name="spell">the spell type to cast</param>
    /// <param name="fromLibrary">the player spell library</param>
    /// <param name="library">the resulting spell library data</param>
    /// <returns>true if cast was successful, false if cast failed</returns>
    public static bool CastSpellFromBook(SpellType spell, SpellLibrary fromLibrary, out SpellLibrary library)
    {
        bool retBool = false;
        SpellLibrary retLibrary = fromLibrary;

        // validate charge exists in spell book
        bool foundInBook = false;
        bool chargefound = false;
        if (retLibrary.spellBook != null && retLibrary.spellBook.Length > 0)
        {
            for ( int i=0; i<retLibrary.spellBook.Length; i++ )
            {
                if ( retLibrary.spellBook[i].type == spell )
                {
                    if (retLibrary.spellBook[i].chargesAvailable > 0)
                    {
                        retLibrary.spellBook[i].chargesAvailable--;
                        chargefound = true;
                        retBool = true; // spell cast from book
                    }
                    foundInBook = true;
                }
            }
            if (!foundInBook)
                UnityEngine.Debug.LogWarning("--- MagicSystem [CastSpellFromBook] : no spell of type "+spell.ToString()+" found in spell book. will ignore.");
            else if (!chargefound)
                UnityEngine.Debug.LogWarning("--- MagicSystem [CastSpellFromBook] : no charges of " + spell.ToString() + " spell available in spell book. will ignore.");
        }
        else
            UnityEngine.Debug.LogWarning("--- MagicSystem [CastSpellFromBook] : spell book or spell book entries do not exist. will ignore.");

        library = retLibrary;

        return retBool;
    }

    /// <summary>
    /// Creates a new spell book entry based on spell type
    /// </summary>
    /// <param name="spell">spell type</param>
    /// <returns>initialized spell book data</returns>
    public static SpellBookData InitializeSpellBookEntry( SpellType spell )
    {
        SpellBookData retEntry = new SpellBookData();

        retEntry.name = spell.ToString(); // to be configured
        retEntry.type = spell;
        retEntry.chargesAvailable = 0;
        retEntry.cooldownDuration = 0f;
        retEntry.cooldown = 0f;
        retEntry.castDuration = 0f;
        retEntry.castAOE = 0f; // range (radius) of area of effect

        retEntry = ConfigureSpellBookEntry(retEntry);

        return retEntry;
    }

    static SpellBookData ConfigureSpellBookEntry( SpellBookData entry )
    {
        SpellBookData retSpell = entry;

        // configure by spell type
        switch (entry.type)
        {
            case SpellType.Default:
                // should never be here
                break;
            case SpellType.FastGrowI:
                retSpell.name = "Fast Grow I";
                retSpell.cooldownDuration = 1440f; // one day
                retSpell.castDuration = 1440f;
                retSpell.castAOE = 3f; // 4x4 plot range
                break;
            case SpellType.SummonWaterI:
                retSpell.name = "Summon Water I";
                retSpell.cooldownDuration = 1440f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            case SpellType.BlessI:
                retSpell.name = "Bless I";
                retSpell.cooldownDuration = 1440f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = 3f; // 4x4 plot range
                break;
            case SpellType.MalnutritionI:
                retSpell.name = "Malnutrition I";
                retSpell.cooldownDuration = 1440f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = 3f; // 4x4 plot range
                break;
            case SpellType.ProsperousI:
                retSpell.name = "Prosperous I";
                retSpell.cooldownDuration = 1440f;
                retSpell.castDuration = 720f; // half day
                retSpell.castAOE = 3f; // 4x4 plot range
                break;
            case SpellType.LesionI:
                retSpell.name = "Lesion I";
                retSpell.cooldownDuration = 1440f;
                retSpell.castDuration = 720f; // half day
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            case SpellType.EclipseI:
                retSpell.name = "Eclipse I";
                retSpell.cooldownDuration = 1440f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = 3f; // 4x4 plot range
                break;
            case SpellType.GoldenThumbI:
                retSpell.name = "Golden Thumb I";
                retSpell.cooldownDuration = 1440f;
                retSpell.castDuration = 720f; // half day
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            default:
                UnityEngine.Debug.LogWarning("--- MagicSystem [ConfigureSpellBookEntry] : spell type undefined. will ignore.");
                break;
        }

        return retSpell;
    }

    /// <summary>
    /// Gets spell book data for a spell entry, if it exists in the spell book
    /// </summary>
    /// <param name="spell">spell type</param>
    /// <param name="library">player spell library data</param>
    /// <returns>spell book data, or null if entry does not exist in spell book</returns>
    public static SpellBookData GetSpellBookEntry(SpellType spell, SpellLibrary library)
    {
        SpellBookData retData = null;

        for (int i = 0; i < library.spellBook.Length; i++)
        {
            if (library.spellBook[i].type == spell)
            {
                retData = library.spellBook[i];
                break;
            }
        }

        return retData;
    }

    /// <summary>
    /// Adds one charge of a spell type to the spell book in a player's library
    /// </summary>
    /// <param name="spell">spell type</param>
    /// <param name="library">player spell library data</param>
    /// <returns>spell library data with added charge</returns>
    public static SpellLibrary AddChargeToSpellBook(SpellType spell, SpellLibrary library)
    {
        SpellLibrary retLibrary = library;

        // if spell listing exists in spell book, increment available charges
        bool found = false;
        if (retLibrary.spellBook == null || retLibrary.spellBook.Length == 0)
        {
            // no spell data yet, create
            // add listing to spell book, configure one charge
            retLibrary.spellBook = new SpellBookData[1];
            retLibrary.spellBook[0] = InitializeSpellBookEntry(spell);
            retLibrary.spellBook[0].chargesAvailable = 1;
            found = true;
        }
        else
        {
            for (int i = 0; i < retLibrary.spellBook.Length; i++)
            {
                if (retLibrary.spellBook[i].type == spell)
                {
                    retLibrary.spellBook[i].chargesAvailable++; // increment charges
                    found = true;
                    break;
                }
            }
        }
        if (!found)
        {
            // add listing to spell book, configure one charge
            SpellBookData[] tmp = new SpellBookData[retLibrary.spellBook.Length + 1];
            for (int i = 0; i < retLibrary.spellBook.Length; i++)
            {
                tmp[i] = retLibrary.spellBook[i];
            }
            tmp[retLibrary.spellBook.Length] = InitializeSpellBookEntry(spell);
            tmp[retLibrary.spellBook.Length].chargesAvailable = 1;
            retLibrary.spellBook = tmp;
        }

        return retLibrary;
    }

    /// <summary>
    /// Returns true if grimoire contains the given spell type
    /// </summary>
    /// <param name="spell">spell type</param>
    /// <param name="library">the player's spell library</param>
    /// <returns>true if spell entry exists in grimoire, false if not</returns>
    public static bool GrimoireHasSpell( SpellType spell, SpellLibrary library )
    {
        bool retBool = false;

        if (library.grimiore == null || library.grimiore.Length == 0)
            return retBool;

        for (int i=0; i<library.grimiore.Length; i++)
        {
            if (library.grimiore[i].type == spell)
            {
                retBool = true;
                break;
            }
        }

        return retBool;
    }

    /// <summary>
    /// Creates a new grimoire entry based on spell type
    /// </summary>
    /// <param name="spell">spell type</param>
    /// <returns>initialized grimoire data</returns>
    public static GrimioreData InitializeGrimoireEntry( SpellType spell )
    {
        GrimioreData retEntry = new GrimioreData();

        retEntry.name = spell.ToString(); // to be configured
        retEntry.description = "";
        retEntry.type = spell;
        retEntry.ingredients = new ItemType[0];

        retEntry = ConfigureGrimoireEntry(retEntry);

        return retEntry;
    }

    static GrimioreData ConfigureGrimoireEntry( GrimioreData entry )
    {
        GrimioreData retSpell = entry;

        // configure by spell type
        switch (entry.type)
        {
            // REVIEW: all ingredient lists
            
            case SpellType.Default:
                // should never be here
                break;
            case SpellType.FastGrowI:
                retSpell.name = "Fast Grow I";
                retSpell.description = "Plants grow faster for one day. (5%)";
                retSpell.ingredients = new ItemType[2];
                retSpell.ingredients[0] = ItemType.Fertilizer;
                retSpell.ingredients[1] = ItemType.Stalk;
                break;
            case SpellType.SummonWaterI:
                retSpell.name = "Summon Water I";
                retSpell.description = "Waters a 2x2 area that stays hydrated for one day.";
                retSpell.ingredients = new ItemType[2];
                retSpell.ingredients[0] = ItemType.Seed;
                retSpell.ingredients[1] = ItemType.Fruit;
                break;
            case SpellType.BlessI:
                retSpell.name = "Bless I";
                retSpell.description = "Make plants immune to all hazards for one day.";
                retSpell.ingredients = new ItemType[3];
                retSpell.ingredients[0] = ItemType.Fertilizer;
                retSpell.ingredients[1] = ItemType.Stalk;
                retSpell.ingredients[2] = ItemType.Seed;
                break;
            case SpellType.MalnutritionI:
                retSpell.name = "Malnutrition I";
                retSpell.description = "Plants grow speed decreases for 1 day. (10%)";
                retSpell.ingredients = new ItemType[3];
                retSpell.ingredients[0] = ItemType.Stalk;
                retSpell.ingredients[1] = ItemType.Rock;
                retSpell.ingredients[2] = ItemType.Seed;
                break;
            case SpellType.ProsperousI:
                retSpell.name = "Prosperous I";
                retSpell.description = "Have a chance of harvesting x2 from each plant. (10%)";
                retSpell.ingredients = new ItemType[4];
                retSpell.ingredients[0] = ItemType.Fruit;
                retSpell.ingredients[1] = ItemType.Seed;
                retSpell.ingredients[2] = ItemType.Seed;
                retSpell.ingredients[3] = ItemType.Seed;
                break;
            case SpellType.LesionI:
                retSpell.name = "Lesion I";
                retSpell.description = "Curse plots and decrease harvest quality. (-5%)";
                retSpell.ingredients = new ItemType[4];
                retSpell.ingredients[0] = ItemType.Fertilizer;
                retSpell.ingredients[1] = ItemType.Stalk;
                retSpell.ingredients[2] = ItemType.Stalk;
                retSpell.ingredients[3] = ItemType.Seed;
                break;
            case SpellType.EclipseI:
                retSpell.name = "Eclipse I";
                retSpell.description = "Obscure sunlight from plots for 1 day.";
                retSpell.ingredients = new ItemType[4];
                retSpell.ingredients[0] = ItemType.Rock;
                retSpell.ingredients[1] = ItemType.Stalk;
                retSpell.ingredients[2] = ItemType.Seed;
                retSpell.ingredients[3] = ItemType.Seed;
                break;
            case SpellType.GoldenThumbI:
                retSpell.name = "Golden Thumb I";
                retSpell.description = "Bless plots and increase harvest quality. (10%)";
                retSpell.ingredients = new ItemType[5];
                retSpell.ingredients[0] = ItemType.Fertilizer;
                retSpell.ingredients[1] = ItemType.Seed;
                retSpell.ingredients[2] = ItemType.Seed;
                retSpell.ingredients[3] = ItemType.Seed;
                retSpell.ingredients[4] = ItemType.Seed;
                break;
            default:
                UnityEngine.Debug.LogWarning("--- MagicSystem [ConfigureGrimoireEntry] : spell type undefined. will ignore.");
                break;
        }

        return retSpell;
    }

    /// <summary>
    /// Adds one entry of spell recipe to the grimoire of a player's library
    /// </summary>
    /// <param name="spell">spell type</param>
    /// <param name="library">player spell library data</param>
    /// <returns>spell library data with added entry in grimoire</returns>
    public static SpellLibrary AddSpellToGrimoire( SpellType spell, SpellLibrary library )
    {
        SpellLibrary retLibrary = library;

        // validate does not yet exist in grimoire
        bool found = false;
        if (retLibrary.grimiore == null || retLibrary.grimiore.Length == 0)
        {
            // no spell data yet, create
            retLibrary.grimiore = new GrimioreData[0];
        }
        else
        {
            for (int i = 0; i < retLibrary.grimiore.Length; i++)
            {
                if (retLibrary.grimiore[i].type == spell)
                {
                    found = true;
                    break;
                }
            }
        }
        if (!found)
        {
            // add listing to grimoire
            GrimioreData[] tmp = new GrimioreData[retLibrary.grimiore.Length + 1];
            for (int i = 0; i < retLibrary.grimiore.Length; i++)
            {
                tmp[i] = retLibrary.grimiore[i];
            }
            tmp[retLibrary.grimiore.Length] = InitializeGrimoireEntry(spell);
            retLibrary.grimiore = tmp;
        }
        else
            UnityEngine.Debug.LogWarning("--- MagicSystem [AddSpellToGrimoire] : spell type " + spell.ToString() + " already exists in grimoire. will ignore.");

        return retLibrary;
    }
}
