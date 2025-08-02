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
        retMagic.library.grimoire = new GrimoireData[0];
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
                        // ARCANA SKILLS : active magic skills never lose their charge
                        if (spell < SpellType.SkillWasteManagement || spell > SpellType.SkillTakeMeHome)
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
            // level one
            case SpellType.FastGrowI:
                retSpell.name = "Fast Grow I";
                retSpell.cooldownDuration = 60f; // one hour
                retSpell.castDuration = 1440f;
                retSpell.castAOE = .5f; // 1x1 plot range
                break;
            case SpellType.SummonWaterI:
                retSpell.name = "Summon Water I";
                retSpell.cooldownDuration = 60f; // five hours
                retSpell.castDuration = 1440f;
                retSpell.castAOE = .5f; // 1x1 plot range
                break;
            case SpellType.SoiledItI: // TODO: revise all new spell config
                retSpell.name = "Soiled It I";
                retSpell.cooldownDuration = 60f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            case SpellType.MirrorMirror:
                retSpell.name = "Mirror Mirror";
                retSpell.cooldownDuration = 1400f;
                retSpell.castDuration = .1f; // instant
                retSpell.castAOE = .5f; // self
                break;
            // level two
            case SpellType.BlessI:
                retSpell.name = "Bless I";
                retSpell.cooldownDuration = 180f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            case SpellType.DaylightI:
                retSpell.name = "Daylight I";
                retSpell.cooldownDuration = 60f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = .5f; // 1x1 plot range
                break;
            case SpellType.GildedWordsI:
                retSpell.name = "Gilded Words I";
                retSpell.cooldownDuration = 300f;
                retSpell.castDuration = 300f;
                retSpell.castAOE = .5f; // self
                break;
            case SpellType.SeedingEcho:
                retSpell.name = "Seeding Echo";
                retSpell.cooldownDuration = 300f;
                retSpell.castDuration = .1f; // instant plant effect
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            case SpellType.ColorTrailI:
                retSpell.name = "Color Trail I";
                retSpell.cooldownDuration = 300f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = .5f; // self
                break;
            case SpellType.ColorTrailII:
                retSpell.name = "Color Trail II";
                retSpell.cooldownDuration = 300f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = .5f; // self
                break;
            case SpellType.ColorTrailIII:
                retSpell.name = "Color Trail III";
                retSpell.cooldownDuration = 300f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = .5f; // self
                break;
            // level 3
            case SpellType.MalnutritionI:
                retSpell.name = "Malnutrition I";
                retSpell.cooldownDuration = 180f; // three hours
                retSpell.castDuration = 1440f;
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            case SpellType.ProsperousI:
                retSpell.name = "Prosperous I";
                retSpell.cooldownDuration = 180f;
                retSpell.castDuration = 1440f; // half day
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            case SpellType.TheGreatHarvest:
                retSpell.name = "The Great Harvest";
                retSpell.cooldownDuration = 180f;
                retSpell.castDuration = .1f; // instant plot auto-harvests
                retSpell.castAOE = 2f; // 3x3 plot range
                break;
            case SpellType.FastGrowII:
                retSpell.name = "Fast Grow II";
                retSpell.cooldownDuration = 180f; // three hours
                retSpell.castDuration = 2880f;
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            case SpellType.Splaturn:
                retSpell.name = "Splaturn";
                retSpell.cooldownDuration = 1440f;
                retSpell.castDuration = .1f; // instant permanent structure effect (clear others of same type)
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            // level 4
            case SpellType.LesionI:
                retSpell.name = "Lesion I";
                retSpell.cooldownDuration = 180f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            case SpellType.TheReaper:
                retSpell.name = "The Reaper";
                retSpell.cooldownDuration = 180f;
                retSpell.castDuration = .1f; // instant plots auto-uproot
                retSpell.castAOE = 2f; // 3x3 plot range
                break;
            case SpellType.Swiftness:
                retSpell.name = "Swiftness";
                retSpell.cooldownDuration = 300f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = .5f; // self
                break;
            case SpellType.LightWork:
                retSpell.name = "Light Work";
                retSpell.cooldownDuration = 300f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = .5f; // self
                break;
            case SpellType.SummonWaterII:
                retSpell.name = "Summon Water II";
                retSpell.cooldownDuration = 180f;
                retSpell.castDuration = 2880f;
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            case SpellType.StarbloomBurst:
                retSpell.name = "Starbloom Burst";
                retSpell.cooldownDuration = 60f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = 2.5f; // 4x4 plot range
                break;
            // level 5
            case SpellType.EclipseI:
                retSpell.name = "Eclipse I";
                retSpell.cooldownDuration = 180f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            case SpellType.GoldenThumbI:
                retSpell.name = "Golden Thumb I";
                retSpell.cooldownDuration = 180f;
                retSpell.castDuration = 1440f; // half day
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            case SpellType.GildedWordsII:
                retSpell.name = "Gilded Words II";
                retSpell.cooldownDuration = 720f;
                retSpell.castDuration = 300f;
                retSpell.castAOE = .5f; // self
                break;
            case SpellType.DullEarth:
                retSpell.name = "Dull Earth";
                retSpell.cooldownDuration = 300f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            case SpellType.FogOfWar:
                retSpell.name = "Fog Of War";
                retSpell.cooldownDuration = 60f;
                retSpell.castDuration = 720f;
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            case SpellType.SoiledItII:
                retSpell.name = "Soiled It II";
                retSpell.cooldownDuration = 180f;
                retSpell.castDuration = .1f; // instant plot stat change
                retSpell.castAOE = 2f; // 3x3 plot range
                break;
            case SpellType.BlessedSpring:
                retSpell.name = "Blessed Spring";
                retSpell.cooldownDuration = 300f;
                retSpell.castDuration = 720f;
                retSpell.castAOE = 1.5f; // 2x2 plot range
                break;
            // level 6
            case SpellType.ProsperousII:
                retSpell.name = "Prosperous II";
                retSpell.cooldownDuration = 180f;
                retSpell.castDuration = 720f;
                retSpell.castAOE = 2f; // 3x3 plot range
                break;
            case SpellType.MalnutritionII:
                retSpell.name = "Malnutrition II";
                retSpell.cooldownDuration = 180f;
                retSpell.castDuration = 2880f;
                retSpell.castAOE = 2f; // 3x3 plot range
                break;
            case SpellType.BlessII:
                retSpell.name = "Bless II";
                retSpell.cooldownDuration = 300f;
                retSpell.castDuration = 4320f;
                retSpell.castAOE = 2f; // 3x3 plot range
                break;
            // level 7
            case SpellType.DaylightII:
                retSpell.name = "Daylight II";
                retSpell.cooldownDuration = 180f;
                retSpell.castDuration = 4320f;
                retSpell.castAOE = 2f; // 3x3 plot range
                break;
            case SpellType.RabbitHole:
                retSpell.name = "Rabbit Hole";
                retSpell.cooldownDuration = 720f;
                retSpell.castDuration = 1440f;
                retSpell.castAOE = .5f; // other player
                break;
            case SpellType.SkillWasteManagement: // ARCANA SKILLS
                retSpell.name = "Waste Management";
                retSpell.cooldownDuration = 0.1f;
                retSpell.castDuration = .1f;
                retSpell.castAOE = .5f; // 1x1 plot range
                break;
            case SpellType.SkillCleanUp:
                retSpell.name = "Clean Up";
                retSpell.cooldownDuration = 0.1f;
                retSpell.castDuration = 1f;
                retSpell.castAOE = 2f; // 3x3 plot range
                break;
            case SpellType.SkillLightenUp:
                retSpell.name = "Lighten Up";
                retSpell.cooldownDuration = 0.1f;
                retSpell.castDuration = 1f;
                retSpell.castAOE = 2f; // 3x3 plot range
                break;
            case SpellType.SkillCashIn:
                retSpell.name = "Cash In";
                retSpell.cooldownDuration = 0.1f;
                retSpell.castDuration = .1f;
                retSpell.castAOE = 2f; // 3x3 plot range
                break;
            case SpellType.SkillTakeMeHome:
                retSpell.name = "Take Me Home";
                retSpell.cooldownDuration = 0.1f;
                retSpell.castDuration = .1f;
                retSpell.castAOE = .5f; // self
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

        if (library.grimoire == null || library.grimoire.Length == 0)
            return retBool;

        for (int i=0; i<library.grimoire.Length; i++)
        {
            if (library.grimoire[i].type == spell)
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
    public static GrimoireData InitializeGrimoireEntry( SpellType spell )
    {
        GrimoireData retEntry = new GrimoireData();

        retEntry.name = spell.ToString(); // to be configured
        retEntry.description = "";
        retEntry.type = spell;
        retEntry.ingredients = new IngredientData[0];

        retEntry = ConfigureGrimoireEntry(retEntry);

        return retEntry;
    }

    static IngredientData InitializeIngredient( ItemType iType, PlantType pType )
    {
        IngredientData retIngredient = new IngredientData();

        string plant = "";
        if ((iType == ItemType.Seed || iType == ItemType.Plant ||
            iType == ItemType.Stalk || iType == ItemType.Fruit) && pType == PlantType.Default)
            plant = "Any";
        else
        {
            PlantData pData = PlantSystem.InitializePlant(pType);
            plant = pData.plantName;
        }
        // REVIEW: need to do the same 'name getting' for items?
        retIngredient.name = iType.ToString() + " (" + plant + ")";
        retIngredient.item = iType;
        retIngredient.plant = pType;

        return retIngredient;
    }

    static GrimoireData ConfigureGrimoireEntry( GrimoireData entry )
    {
        GrimoireData retSpell = entry;

        // configure by spell type
        switch (entry.type)
        {
            // REVIEW: all ingredient lists
            
            case SpellType.Default:
                // should never be here
                break;
            case SpellType.FastGrowI:
                retSpell.name = "Fast Grow I";
                retSpell.description = "Plants grow faster for one day (+33%)";
                retSpell.ingredients = new IngredientData[2];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fertilizer, PlantType.Default);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Stalk, PlantType.Corn);
                break;
            case SpellType.SummonWaterI:
                retSpell.name = "Summon Water I";
                retSpell.description = "Plot of land stays hydrated for one day";
                retSpell.ingredients = new IngredientData[2];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Seed, PlantType.Default);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fruit, PlantType.Carrot);
                break;
            case SpellType.SoiledItI: // TODO: revise all new spell config
                retSpell.name = "Soiled It I";
                retSpell.description = "Instantly increase soil quality (+50%)";
                retSpell.ingredients = new IngredientData[2];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fertilizer, PlantType.Default);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fertilizer, PlantType.Default);
                break;
            case SpellType.MirrorMirror:
                retSpell.name = "Mirror Mirror";
                retSpell.description = "Change your appearance";
                retSpell.ingredients = new IngredientData[2];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Stalk, PlantType.Tomato);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Seed, PlantType.Default);
                break;
            case SpellType.BlessI:
                retSpell.name = "Bless I";
                retSpell.description = "Plots of land immune to hazards for one day";
                retSpell.ingredients = new IngredientData[2];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fruit, PlantType.Lotus);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Stalk, PlantType.Sunflower);
                break;
            case SpellType.DaylightI:
                retSpell.name = "Daylight I";
                retSpell.description = "Summon sunlight on a plot for one day";
                retSpell.ingredients = new IngredientData[2];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Stalk, PlantType.Sunflower);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Plant, PlantType.Marigold);
                break;
            case SpellType.GildedWordsI:
                retSpell.name = "Gilded Words I";
                retSpell.description = "Make yourself charming (-25% off market prices)";
                retSpell.ingredients = new IngredientData[3];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fruit, PlantType.Snowgrace);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fruit, PlantType.Snowgrace);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Plant, PlantType.Popcorn);
                break;
            case SpellType.SeedingEcho:
                retSpell.name = "Seeding Echo";
                retSpell.description = "Harvest from plants guaranteed to drop seed";
                retSpell.ingredients = new IngredientData[3];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Stalk, PlantType.Carrot);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Stalk, PlantType.Carrot);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Fruit, PlantType.Lemon);
                break;
            case SpellType.ColorTrailI:
                retSpell.name = "Color Trail I";
                retSpell.description = "Leave a trail of your primary color behind you";
                retSpell.ingredients = new IngredientData[3];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Stalk, PlantType.Moonflower);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fertilizer, PlantType.Default);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Rock, PlantType.Default);
                break;
            case SpellType.ColorTrailII:
                retSpell.name = "Color Trail II";
                retSpell.description = "Leave a trail of your secondary color behind you";
                retSpell.ingredients = new IngredientData[3];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Stalk, PlantType.Moonflower);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fertilizer, PlantType.Default);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Rock, PlantType.Default);
                break;
            case SpellType.ColorTrailIII:
                retSpell.name = "Color Trail III";
                retSpell.description = "Leave a trail of your accent color behind you";
                retSpell.ingredients = new IngredientData[3];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Stalk, PlantType.Moonflower);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fertilizer, PlantType.Default);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Rock, PlantType.Default);
                break;
            case SpellType.MalnutritionI:
                retSpell.name = "Malnutrition I";
                retSpell.description = "Plants grow slower for one day (-33%)";
                retSpell.ingredients = new IngredientData[3];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fruit, PlantType.Chrystalia);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Plant, PlantType.EclipseFlower);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Seed, PlantType.Apple);
                break;
            case SpellType.ProsperousI:
                retSpell.name = "Prosperous I";
                retSpell.description = "Plant harvest yields twice as much";
                retSpell.ingredients = new IngredientData[4];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fruit, PlantType.Underbloom);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Plant, PlantType.Rose);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Fertilizer, PlantType.Default);
                retSpell.ingredients[3] = InitializeIngredient(ItemType.Fertilizer, PlantType.Default);
                break;
            case SpellType.TheGreatHarvest:
                retSpell.name = "The Great Harvest";
                retSpell.description = "Harvest several plots immediately";
                retSpell.ingredients = new IngredientData[3];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Plant, PlantType.Pumpkin);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fruit, PlantType.Orange);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Fruit, PlantType.Corn);
                break;
            case SpellType.FastGrowII:
                retSpell.name = "Fast Grow II";
                retSpell.description = "Plants grow faster for two days (+67%)";
                retSpell.ingredients = new IngredientData[3];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fertilizer, PlantType.Default);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Stalk, PlantType.Corn);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Fruit, PlantType.Popcorn);
                break;
            case SpellType.Splaturn:
                retSpell.name = "Splaturn";
                retSpell.description = "Change the color of your tower";
                retSpell.ingredients = new IngredientData[2];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fruit, PlantType.Apple);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Plant, PlantType.Lotus);
                break;
            case SpellType.LesionI:
                retSpell.name = "Lesion I";
                retSpell.description = "Decrease harvest quality (-50%)";
                retSpell.ingredients = new IngredientData[3];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fruit, PlantType.Magnolia);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Stalk, PlantType.Myosotis);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Plant, PlantType.Tomato);
                break;
            case SpellType.BlessedSpring:
                retSpell.name = "Blessed Spring";
                retSpell.description = "Force multiple plants to re-fruit";
                retSpell.ingredients = new IngredientData[4];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Stalk, PlantType.CrystalRose);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Plant, PlantType.Yarrow);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Plant, PlantType.Popcorn);
                retSpell.ingredients[3] = InitializeIngredient(ItemType.Plant, PlantType.Lemon);
                break;
            case SpellType.TheReaper:
                retSpell.name = "The Reaper";
                retSpell.description = "Dig up several plots, leaving holes and destroying seedlings";
                retSpell.ingredients = new IngredientData[3];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Plant, PlantType.WaterLily);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Stalk, PlantType.Snowgrace);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Plant, PlantType.Poppy);
                break;
            case SpellType.Swiftness:
                retSpell.name = "Swiftness";
                retSpell.description = "Move faster (150%)";
                retSpell.ingredients = new IngredientData[3];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Stalk, PlantType.Magnolia);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fruit, PlantType.Sunflower);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Plant, PlantType.Chrystalia);
                break;
            case SpellType.LightWork:
                retSpell.name = "Light Work";
                retSpell.description = "Farm work goes faster (200%)";
                retSpell.ingredients = new IngredientData[3];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fruit, PlantType.Marigold);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fruit, PlantType.EclipseFlower);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Plant, PlantType.Carrot);
                break;
            case SpellType.SummonWaterII:
                retSpell.name = "Summon Water II";
                retSpell.description = "Plots of land stay hydrated for two days";
                retSpell.ingredients = new IngredientData[3];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fruit, PlantType.Pumpkin);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fruit, PlantType.Carrot);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Plant, PlantType.Chrystalia);
                break;
            case SpellType.StarbloomBurst:
                retSpell.name = "Starbloom Burst";
                retSpell.description = "Cast continuous fireworks into the sky";
                retSpell.ingredients = new IngredientData[4];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Plant, PlantType.Underbloom);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Plant, PlantType.Corn);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Fruit, PlantType.Rose);
                retSpell.ingredients[3] = InitializeIngredient(ItemType.Fruit, PlantType.Rose);
                break;
            case SpellType.EclipseI:
                retSpell.name = "Eclipse I";
                retSpell.description = "Block sunlight from plots for one day";
                retSpell.ingredients = new IngredientData[4];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Stalk, PlantType.Nightshade);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Plant, PlantType.CrystalRose);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Plant, PlantType.Hollowbloom);
                retSpell.ingredients[3] = InitializeIngredient(ItemType.Plant, PlantType.Pumpkin);
                break;
            case SpellType.GoldenThumbI:
                retSpell.name = "Golden Thumb I";
                retSpell.description = "Increase harvest quality (+50%)";
                retSpell.ingredients = new IngredientData[4];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Plant, PlantType.Mysteria);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Plant, PlantType.Coconut);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Fruit, PlantType.Myosotis);
                retSpell.ingredients[3] = InitializeIngredient(ItemType.Fruit, PlantType.Tomato);
                break;
            case SpellType.GildedWordsII:
                retSpell.name = "Gilded Words II";
                retSpell.description = "Make yourself charming (-50% off market prices)";
                retSpell.ingredients = new IngredientData[5];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fruit, PlantType.Snowgrace);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fruit, PlantType.Snowgrace);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Fruit, PlantType.FrostLily);
                retSpell.ingredients[3] = InitializeIngredient(ItemType.Stalk, PlantType.Mandrake);
                retSpell.ingredients[4] = InitializeIngredient(ItemType.Plant, PlantType.Myosotis);
                break;
            case SpellType.DullEarth:
                retSpell.name = "Dull Earth";
                retSpell.description = "Decrease soil quality of several plots (-50%)";
                retSpell.ingredients = new IngredientData[4];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fruit, PlantType.Banana);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fruit, PlantType.Yarrow);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Plant, PlantType.Nightshade);
                retSpell.ingredients[3] = InitializeIngredient(ItemType.Plant, PlantType.Marigold);
                break;
            case SpellType.FogOfWar:
                retSpell.name = "Fog Of War";
                retSpell.description = "Summon cloud over an area";
                retSpell.ingredients = new IngredientData[4];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fruit, PlantType.GoldenApple);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fruit, PlantType.Mandrake);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Fruit, PlantType.Mysteria);
                retSpell.ingredients[3] = InitializeIngredient(ItemType.Plant, PlantType.Snowgrace);
                break;
            case SpellType.SoiledItII:
                retSpell.name = "Soiled It II";
                retSpell.description = "Instantly maximize soil quality (100%)";
                retSpell.ingredients = new IngredientData[5];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Plant, PlantType.Banana);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fruit, PlantType.Nightshade);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Plant, PlantType.Lotus);
                retSpell.ingredients[3] = InitializeIngredient(ItemType.Fertilizer, PlantType.Default);
                retSpell.ingredients[4] = InitializeIngredient(ItemType.Fertilizer, PlantType.Default);
                break;
            case SpellType.ProsperousII:
                retSpell.name = "Prosperous II";
                retSpell.description = "Plant harvest yields three times as much";
                retSpell.ingredients = new IngredientData[4];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fruit, PlantType.Coconut);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Stalk, PlantType.Mandrake);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Stalk, PlantType.Popcorn);
                retSpell.ingredients[3] = InitializeIngredient(ItemType.Plant, PlantType.Moonflower);
                break;
            case SpellType.MalnutritionII:
                retSpell.name = "Malnutrition II";
                retSpell.description = "Plants grow slower for two days (-67%)";
                retSpell.ingredients = new IngredientData[5];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fruit, PlantType.CrystalRose);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fruit, PlantType.Hollowbloom);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Stalk, PlantType.Nightshade);
                retSpell.ingredients[3] = InitializeIngredient(ItemType.Stalk, PlantType.Magnolia);
                retSpell.ingredients[4] = InitializeIngredient(ItemType.Seed, PlantType.Coconut);
                break;
            case SpellType.BlessII:
                retSpell.name = "Bless II";
                retSpell.description = "Plots of land immune to hazards for three days";
                retSpell.ingredients = new IngredientData[5];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Plant, PlantType.FrostLily);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Stalk, PlantType.Yarrow);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Fruit, PlantType.GoldenApple);
                retSpell.ingredients[3] = InitializeIngredient(ItemType.Fruit, PlantType.Lotus);
                retSpell.ingredients[4] = InitializeIngredient(ItemType.Seed, PlantType.Popcorn);
                break;
            case SpellType.DaylightII:
                retSpell.name = "Daylight II";
                retSpell.description = "Summon sunlight on plots for three days";
                retSpell.ingredients = new IngredientData[5];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Plant, PlantType.Coconut);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fruit, PlantType.Mandrake);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Fruit, PlantType.Banana);
                retSpell.ingredients[3] = InitializeIngredient(ItemType.Fruit, PlantType.Marigold);
                retSpell.ingredients[4] = InitializeIngredient(ItemType.Plant, PlantType.FrostLily);
                break;
            case SpellType.RabbitHole:
                retSpell.name = "Rabbit Hole";
                retSpell.description = "Another player is trapped for one day";
                retSpell.ingredients = new IngredientData[6];
                retSpell.ingredients[0] = InitializeIngredient(ItemType.Fruit, PlantType.CrystalRose);
                retSpell.ingredients[1] = InitializeIngredient(ItemType.Fruit, PlantType.Nightshade);
                retSpell.ingredients[2] = InitializeIngredient(ItemType.Stalk, PlantType.Yarrow);
                retSpell.ingredients[3] = InitializeIngredient(ItemType.Plant, PlantType.Lotus);
                retSpell.ingredients[4] = InitializeIngredient(ItemType.Plant, PlantType.Chrystalia);
                retSpell.ingredients[5] = InitializeIngredient(ItemType.Plant, PlantType.WaterLily);
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
        if (retLibrary.grimoire == null || retLibrary.grimoire.Length == 0)
        {
            // no spell data yet, create
            retLibrary.grimoire = new GrimoireData[0];
        }
        else
        {
            for (int i = 0; i < retLibrary.grimoire.Length; i++)
            {
                if (retLibrary.grimoire[i].type == spell)
                {
                    found = true;
                    break;
                }
            }
        }
        if (!found)
        {
            // add listing to grimoire
            GrimoireData[] tmp = new GrimoireData[retLibrary.grimoire.Length + 1];
            for (int i = 0; i < retLibrary.grimoire.Length; i++)
            {
                tmp[i] = retLibrary.grimoire[i];
            }
            tmp[retLibrary.grimoire.Length] = InitializeGrimoireEntry(spell);
            retLibrary.grimoire = tmp;
        }
        else
            UnityEngine.Debug.LogWarning("--- MagicSystem [AddSpellToGrimoire] : spell type " + spell.ToString() + " already exists in grimoire. will ignore.");

        return retLibrary;
    }
}
