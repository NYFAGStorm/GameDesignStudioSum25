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

        retEntry.name = spell.ToString(); // TODO: configure
        retEntry.type = spell;
        retEntry.chargesAvailable = 0;
        retEntry.cooldownDuration = 0f;
        retEntry.cooldown = 0f;
        retEntry.castDuration = 0f;
        retEntry.castAOE = 0f; // range (radius) of area of effect

        // TODO: configure by spell type
        switch (spell)
        {
            case SpellType.Default:
                // should never be here
                break;
            case SpellType.SpellA:
                break;
            case SpellType.SpellB:
                break;
            case SpellType.SpellC:
                break;
            case SpellType.SpellD:
                break;
            default:
                UnityEngine.Debug.LogWarning("--- MagicSystem [InitializeSpellBookEntry] : spell type undefined. will ignore.");
                break;
        }

        return retEntry;
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

        retEntry.name = spell.ToString(); // TODO: configure
        retEntry.description = "";
        retEntry.type = spell;
        retEntry.ingredients = new ItemType[0];

        // TODO: configure by spell type
        switch (spell)
        {
            case SpellType.Default:
                // should never be here
                break;
            case SpellType.SpellA:
                break;
            case SpellType.SpellB:
                break;
            case SpellType.SpellC:
                break;
            case SpellType.SpellD:
                break;
            default:
                UnityEngine.Debug.LogWarning("--- MagicSystem [InitializeGrimoireEntry] : spell type undefined. will ignore.");
                break;
        }

        return retEntry;
    }

    /// <summary>
    /// Adds one entry of spell recipe to the grimoire of a player's library
    /// </summary>
    /// <param name="spell">spell type</param>
    /// <param name="library">player spell library data</param>
    /// <returns>spell library data with added entry in grimoire</returns>
    public static SpellLibrary AddSpellToGrimoire( SpellType spell, SpellLibrary library )
    {
        SpellLibrary retLibrary = new SpellLibrary();

        // validate does not yet exist in grimoire
        bool found = false;
        if (retLibrary.grimiore == null || retLibrary.grimiore.Length == 0)
        {
            // no spell data yet, create
            // add first listing to grimoire
            retLibrary.grimiore = new GrimioreData[1];
            retLibrary.grimiore[0] = InitializeGrimoireEntry(spell);
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
            UnityEngine.Debug.LogWarning("--- MagicSystem [AddSpellToGrimoire] : spell type "+spell.ToString()+" already exists in grimoire. will ignore.");

        return retLibrary;
    }
}
