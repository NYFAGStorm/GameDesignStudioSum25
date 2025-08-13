// REVIEW: necessary namespaces

public static class AlmanacSystem
{
    /// <summary>
    /// Returns initialized almanac data
    /// </summary>
    /// <returns>initialized almanac data</returns>
    public static AlmanacEntry InitializeEntry()
    {
        AlmanacEntry retEntry = new AlmanacEntry();

        retEntry.title = "";
        retEntry.category = AlmanacCategory.Default;
        retEntry.revealed = false;
        retEntry.icon = "";
        retEntry.subtitle = "";
        retEntry.description = "";
        retEntry.details = new string[0];

        return retEntry;
    }

    /// <summary>
    /// Returns a string of randomly generated lorem ipsum with the same word count as the given string
    /// </summary>
    /// <param name="nonLorem">non-lorem string</param>
    /// <returns>lorem ipsum string</returns>
    public static string ConvertToRandomLorem(string nonLorem)
    {
        string retString = "";

        if (nonLorem == null || nonLorem.Length == 0)
        {
            UnityEngine.Debug.LogWarning("--- AlmanacSystem [ConvertToRandomLorem] : input string invalid. will return empty string.");
            return retString;
        }

        string[] words = nonLorem.Split(char.Parse(" "));
        retString = GenerateLoremIpsum(words.Length, "");

        return retString;
    }

    /// <summary>
    /// Returns a string of procedurally generated lorem ipsum with the same word count as the given string
    /// </summary>
    /// <param name="nonLorem">non-lorem string</param>
    /// <returns>lorem ipsum string</returns>
    public static string ConvertToProceduralLorem(string nonLorem)
    {
        string retString = "";

        if (nonLorem == null || nonLorem.Length == 0)
        {
            UnityEngine.Debug.LogWarning("--- AlmanacSystem [ConvertToProceduralLorem] : input string invalid. will return empty string.");
            return retString;
        }

        retString = GenerateLoremIpsum(0, nonLorem);

        return retString;
    }

    /// <summary>
    /// Returns a string of procedurally or randomly generated lorem ipsum with the given number of words and adding reasonable commas, periods and capitalization
    /// </summary>
    /// <param name="loremWords">number of lorem words to generate randomly (ignored if procedural)</param>
    /// <param name="proceduralSeed">non-lorem string to used as word count and seed for procedural lorem (will ignore if empty)</param>
    /// <returns>a string of lorem ipsum text in the given word count</returns>
    public static string GenerateLoremIpsum(int loremWords, string proceduralSeed)
    {
        string retLorem = "";

        bool performProcedural = (proceduralSeed != ""); // override random result
        string[] wordsProcedural = new string[0];
        if (performProcedural)
        {
            wordsProcedural = proceduralSeed.Split(char.Parse(" "));
            loremWords = wordsProcedural.Length;
        }

        int total = 68;
        int basePeriodInterval = 20;
        int baseCommaInterval = 7;
        float variance = 0.381f;
        /*
        * Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
        * Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.
        * Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.
        * Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
        */
        string[] words = new string[total];
        string longLorem = "lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore et dolore magna aliqua ut enim ad minim veniam quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur excepteur sint occaecat cupidatat non proident sunt in culpa qui officia deserunt mollit anim id est laborum";
        words = longLorem.Split(char.Parse(" "));

        int commaInterval = 1;
        int periodInterval = 1;

        for (int i = 0; i < loremWords; i++)
        {
            float variationA = 0f;
            float variationB = 0f;
            float variationC = 0f;

            int index = loremWords + i;
            index %= proceduralSeed.Length - 1;
            if (performProcedural)
            {
                // make three floats
                for (int n = 0; n < 3; n++)
                {
                    float result = 0f;
                    int factor = 1;
                    index += i * n;
                    index %= proceduralSeed.Length - 1;
                    // generate digits
                    for (int t = 0; t < 7; t++)
                    {
                        int d = (int)proceduralSeed[index] / (n+1); // letters to numbers
                        d += i + n + t;
                        result += d * factor;
                        factor *= 10;
                        index += d;
                        index %= proceduralSeed.Length - 1;
                    }
                    // normalize to 0-1
                    while (result > 1f)
                    {
                        result *= 0.1f;
                    }
                    // record result
                    if (n == 0)
                        variationA = result;
                    else if (n == 1)
                        variationB = result;
                    else
                        variationC = result;
                }
                // shuffle up
                for (int e = 0; e < loremWords - i; e++)
                {
                    float tmp = variationA;
                    variationA = variationB;
                    variationB = variationC;
                    variationC = tmp;
                }
                //UnityEngine.Debug.Log("variations A: "+variationA+" ,B: "+variationB+" ,C: "+variationC);
            }
            else
            {
                variationA = RandomSystem.FlatRandom01();
                variationB = RandomSystem.FlatRandom01();
                variationC = RandomSystem.FlatRandom01();
            }

            int idx = GameSystem.RoundedResult(variationA, total);
            if (i < loremWords - 4 && commaInterval > baseCommaInterval + ((baseCommaInterval * variationB * variance) - baseCommaInterval / 2f))
            {
                retLorem += words[idx] + ", ";
                commaInterval = 0;
            }
            else if (i < loremWords - 2 && periodInterval > basePeriodInterval + ((basePeriodInterval * variationC * variance) - basePeriodInterval / 2f))
            {
                retLorem += words[idx] + ". ";
                periodInterval = 0;
            }
            else
            {
                if (periodInterval < 2)
                {
                    string cap = words[idx].Substring(0, 1).ToUpper();
                    string rest = words[idx].Substring(1);
                    retLorem += cap + rest + " ";
                }
                else
                    retLorem += words[idx] + " ";

            }
            commaInterval++;
            periodInterval++;
        }
        retLorem = retLorem.TrimEnd(char.Parse(" "));
        retLorem += ".";

        return retLorem;
    }

    /// <summary>
    /// Creates a new almanac entry replacing given entry with lorem ipsum if revealed is false
    /// </summary>
    /// <param name="entry">almanac entry data</param>
    /// <returns>if revealed, returns original entry. otherwise, this returns almanac entry created to hide original with lorem ipsum</returns>
    public static AlmanacEntry GenerateLoremAlmanacEntry(AlmanacEntry entry)
    {
        AlmanacEntry retEntry = entry;

        if (retEntry.revealed)
            return entry;

        retEntry.title = ConvertToProceduralLorem(retEntry.title).TrimEnd(char.Parse("."));
        retEntry.icon = "GenesisTree"; // default hidden entry icon
        retEntry.subtitle = ConvertToProceduralLorem(retEntry.subtitle);
        retEntry.description = ConvertToProceduralLorem(retEntry.description);
        if (retEntry.details != null)
        {
            for (int i = 0; i < retEntry.details.Length; i++)
            {
                retEntry.details[i] = ConvertToProceduralLorem(retEntry.details[i]).TrimEnd(char.Parse("."));
            }
        }
        else
            retEntry.details = new string[0];

        return retEntry;
    }

    /// <summary>
    /// Returns the array of boolean values representing revealed entries from the given almanac data (for use as player data property, almanac)
    /// </summary>
    /// <param name="almanac">almanac data</param>
    /// <returns>the array of booleans representing each entry revealed state</returns>
    public static bool[] GetAlmanacRevealedFlags(AlmanacData almanac)
    {
        bool[] retBools = new bool[almanac.entries.Length];

        for (int i = 0; i < almanac.entries.Length; i++)
        {
            retBools[i] = almanac.entries[i].revealed;
        }

        return retBools;
    }

    /// <summary>
    /// Sets the almanac revealed flags for the given almanac data using the given array of boolean values
    /// </summary>
    /// <param name="almanac">almanac data</param>
    /// <param name="revealed">array of boolean values (from player data)</param>
    /// <returns>almanac data with revealed flags set per player data boolean array</returns>
    public static AlmanacData SetAlmanacRevealedFlags( AlmanacData almanac, bool[] revealed )
    {
        AlmanacData retAlmanac = almanac;

        if (almanac.entries.Length != revealed.Length)
        {
            UnityEngine.Debug.LogWarning("--- AlamanacSystem [SetAlmanacRevealedFlags] : mismatch length of reveal flag array and almanac entry array. will ignore.");
            return retAlmanac;
        }

        for (int i = 0; i < retAlmanac.entries.Length; i++)
        {
            retAlmanac.entries[i].revealed = revealed[i];
        }

        return retAlmanac;
    }

    /// <summary>
    /// Returns the entry index in the given almanac data that matches the given entry title
    /// </summary>
    /// <param name="almanac">almanac data</param>
    /// <param name="entryTitle">almanac entry title</param>
    /// <returns>entry index if found, -1 if not found</returns>
    public static int GetAlmanacEntryIndex( AlmanacData almanac, string entryTitle )
    {
        int retInt = -1;

        for (int i = 0; i < almanac.entries.Length; i++)
        {
            if (almanac.entries[i].title == entryTitle)
            {
                retInt = i;
                break;
            }
        }
        if (retInt == -1)
            UnityEngine.Debug.LogWarning("--- AlmanacSystem [GetAlmanacEntryIndex] : no entry with title '" + entryTitle + "' found. will return -1.");

        return retInt;
    }

    /// <summary>
    /// Initializes the biomancer's almanac data, including default reveal flags
    /// </summary>
    /// <returns>initialized almanac data</returns>
    public static AlmanacData InitializeAlmanac()
    {
        AlmanacData retData = new AlmanacData();

        int loreEntries = 3;
        int peopleEntries = 4;
        int placeEntries = 2;
        int itemEntries = 13;
        int farmEntries = 9;
        int plantEntries = 50;
        int magicEntries = 15;
        int eventEntries = 10;
        int secretEntries = 4;

        int totalEntries = loreEntries + peopleEntries + placeEntries + 
            itemEntries + farmEntries + plantEntries + magicEntries + 
            eventEntries + secretEntries;

        peopleEntries += loreEntries;
        placeEntries += peopleEntries;
        itemEntries += placeEntries;
        farmEntries += itemEntries;
        plantEntries += farmEntries;
        magicEntries += plantEntries;
        eventEntries += magicEntries;
        secretEntries += eventEntries;

        retData.entries = new AlmanacEntry[totalEntries];

        for (int i = 0; i < totalEntries; i++)
        {
            retData.entries[i] = InitializeEntry();

            string title = "";
            bool revealed = false;
            string icon = "GenesisTree";
            string subtitle = "";
            string description = "";
            string detailA = "";
            string detailB = "";
            string detailC = "";
            string detailD = "";
            string detailE = "";

            if (i < loreEntries)
            {
                retData.entries[i].category = AlmanacCategory.Lore;
                // LORE
                switch(i)
                {
                    case 0:
                        title = "The Genesis Tree";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "The Source of All Life and Magic";
                        description = "The great Genesis Tree, from where all magic in the world flows to and from. Nobody knows where it came from or how it grew into the towering plant it is today. Regardless, it is the sacred duty of the Biomancers to ensure the growth of the Genesis Tree continues to remain eternal.";
                        detailA = "Mythology";
                        detailB = "History";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 1:
                        title = "The Mystic Magistrate";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Council of Supreme Biomancers";
                        description = "Guardians of the Genesis Tree, overseers of all biomancers, and the most responsible magic users you can find in Empyrea. The Mystic Magistrate watches over all biomancers and provides them guidance when necessary. You can always rely on them to help whenever you’re feeling lost.";
                        detailA = "History";
                        detailB = "Guidance";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 2:
                        title = "Arcana";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Our Magical Currency";
                        description = "Biomancers are serving a purpose for the Genesis Tree by cultivating Arcana from sunlight, other natural resources and their magical practice. The Arcana each biomancer earns strengthens the Genesis Tree, and in turn, our community. With Arcana, a biomancer becomes more skilled and gains advanced abilities.";
                        detailA = "Mythology";
                        detailB = "Arcana Skills";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                }
            }
            else if (i < peopleEntries)
            {
                retData.entries[i].category = AlmanacCategory.People;
                // PEOPLE
                switch (i - loreEntries)
                {
                    case 0:
                        title = "Magister Eden";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Magister of Gardens";
                        description = "The most knowledgeable of the three Mystic Magistrate members, Magister Eden is a very kindhearted lady. Her knowledge of the plants that can be found in Empyrea is staggering. She enjoys helping new biomancers adjust to their positions and grants them an island to begin their life in Empyrea as well.";
                        detailA = "Magistrate";
                        detailB = "Biomancers";
                        detailC = "Plants";
                        detailD = "";
                        detailE = "";
                        break;
                    case 1:
                        title = "Magister Salesman";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Magister of Arcana";
                        description = "The ever punctual Magister Salesman. Magister Salesman is in charge of sending islands to the Genesis Tree when they are deemed refined of all their magic. He said once before that what he loves most is the smiles on his customers' faces when they see their new island.";
                        detailA = "Magistrate";
                        detailB = "Merchant";
                        detailC = "Island Upgrades";
                        detailD = "";
                        detailE = "";
                        break;
                    case 2:
                        title = "Magister Shady";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Magister of Curses";
                        description = "The Magister with perhaps the most mystery surrounding him out of them all. Magister Shady usually stays out of the way, watching Empyrea’s activities from a distance. Every now and then he’ll take interest in a biomancer and offer them some rather unique curses. For a price of course, much to Magister Eden’s dismay.";
                        detailA = "Magistrate";
                        detailB = "Curses";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 3:
                        title = "Mr. Sells Alat";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "The Most Successful Sale-Cat";
                        description = "The best merchant through all the skies of Empyrea. Perhaps even the greatest merchant in the entire world, or so he’ll try to get you to believe. Mr. Salesalot is a longstanding member of Empyrea’s community of biomancers. Though he isn’t one himself, he sells all the things necessary to Biomancer life. Just don’t question why he wears different hats at different stalls and you’ll get along great.";
                        detailA = "Merchant";
                        detailB = "Market";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                }
            }
            else if (i < placeEntries)
            {
                retData.entries[i].category = AlmanacCategory.Places;
                // PLACES
                switch (i-peopleEntries)
                {
                    case 0:
                        title = "Empyrea";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "The Biomancer Skies";
                        description = "Rather than a single designated area, Empyrea is the general name for the airspace belonging to the Biomancers. It stretches far beyond the horizon yet is never out of reach. Here, you’ve made your home and will continue to grow it as a member of the Empyrea community.";
                        detailA = "The Skies";
                        detailB = "Biomancers";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 1:
                        title = "The Skyport";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "The Central Marketplace";
                        description = "Where most of the activity in Empyrea occurs. The Skyport is where biomancers come to converse amongst each other, purchase goods from the market, and other important daily activities. If you ever need something, the Skyport is where you can find it.";
                        detailA = "The Skies";
                        detailB = "Market";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                }
            }
            else if (i < itemEntries)
            {
                retData.entries[i].category = AlmanacCategory.Items;
                // ITEMS
                switch (i-placeEntries)
                {
                    case 0:
                        title = "Seed";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "A small package for big things";
                        description = "All seeds appear similar before growing into their own. Seeds can be planted manually by holding them and working the land. Seeds can also be planted by broadcasting, dropping them on tilled plots.";
                        detailA = "Plant";
                        detailB = "Growing";
                        detailC = "Crafting";
                        detailD = "";
                        detailE = "";
                        break;
                    case 1:
                        title = "Plant";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "A singular piece of life";
                        description = "A grown plant from seed. Plants may be harvested for their fruit, and some plants will continue to grow, to re-fruit. Most plants need sunlight, but some (Dark Plants) need moonlight. All plants need water and good soil quality to grow healthy. Healthy plants produce better harvested fruit quality.";
                        detailA = "Plant";
                        detailB = "Harvesting";
                        detailC = "Crafting";
                        detailD = "";
                        detailE = "";
                        break;
                    case 2:
                        title = "Stalk";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Supportive structure";
                        description = "A stalk is left over after a plant is harvested, but plants that re-fruit do not have left-over stalks. A stalk will continue to drain soil quality and water from a plot after harvesting. A stalk in a plot can be grafted to a fruit of another plant, creating a new, more rare, plant type.";
                        detailA = "Plant";
                        detailB = "Grafting";
                        detailC = "Crafting";
                        detailD = "";
                        detailE = "";
                        break;
                    case 3:
                        title = "Fruit";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Flowering beauty and bounty";
                        description = "A fruit or flower is the product of a harvested plant. Quality of a fruit translates to a higher value when selling at the market. A fruit in hand can be grafted to a stalk of another plant, creating a new, more rare, plant type.";
                        detailA = "Plant";
                        detailB = "Grafting";
                        detailC = "Selling";
                        detailD = "Crafting";
                        detailE = "";
                        break;
                    case 4:
                        title = "Fertilizer";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Grounding element";
                        description = "A necessary part of gardening, the product of the compost bin after ‘cooking’ plant material. It improves soil quality dramatically when placed in plots that have been uprooted.";
                        detailA = "Compost";
                        detailB = "Soil Quality";
                        detailC = "Crafting";
                        detailD = "";
                        detailE = "";
                        break;
                    case 5:
                        title = "Rock";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Simple and solid";
                        description = "A small rock. ‘Funny thing is, sometimes it’s there and sometimes it isn’t.";
                        detailA = "Crafting";
                        detailB = "Secret life";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 6:
                        title = "Gold Coin";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "All that glitters";
                        description = "A single gold piece, and the basis for currency at the market and with island upgrades. Gold may be spent on goods at the market or with the traveling salesman, or one may bet gold on chicken races near the market. There are other means to acquire gold coins.";
                        detailA = "Currency";
                        detailB = "Market";
                        detailC = "Island upgrades";
                        detailD = "";
                        detailE = "";
                        break;
                    case 7:
                        title = "Gold Pouch";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Hold the purse strings";
                        description = "A gold pouch appears when many gold coins are dropped at once. Picking up a gold pouch immediately transfers all the gold within to one's purse.";
                        detailA = "Currency";
                        detailB = "Market";
                        detailC = "Island upgrades";
                        detailD = "Consumable";
                        detailE = "";
                        break;
                    case 8:
                        title = "Package";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Special delivery";
                        description = "Packages hold many items, and when picking up a package, all the items within appear. If one purchases more items at the market than their inventory can hold, the market will deliver a package to their mailbox with those items inside.";
                        detailA = "Mail";
                        detailB = "Consumable";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 9:
                        title = "Letter";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Greetings and well wishes";
                        description = "Letters are small personal messages from others. They often contain helpful ideas or reminders.";
                        detailA = "Mail";
                        detailB = "Consumable";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 10:
                        title = "Coupon";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Valuable discount";
                        description = "A coupon can be held while purchasing at the market for a discount on that item. The amount of discount can be as high as 100%. Upon buying an item at the market, the coupon is exchanged for that item.";
                        detailA = "Market";
                        detailB = "Trade";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 11:
                        title = "Scroll";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Magical powerup";
                        description = "A magic scroll contains a single spell charge, to be transferred directly to one's spell book without having to craft. One reads the scroll by picking it up. Upon reading the scroll, the charge is added to a spell book and the scroll disappears.";
                        detailA = "Magic";
                        detailB = "Spell Charge";
                        detailC = "Consumable";
                        detailD = "";
                        detailE = "";
                        break;
                    case 12:
                        title = "Potion";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Counter cooldowns";
                        description = "A magic potion clears spell cooldowns. Grey potions will clear one spell charge at random within a spell book. White potions will clear all spell charges in a spell book. One drinks the potion by picking it up. Upon drinking the potion, it will disappear.";
                        detailA = "Magic";
                        detailB = "Spell Cooldown";
                        detailC = "Consumable";
                        detailD = "";
                        detailE = "";
                        break;
                }
            }
            else if (i < farmEntries)
            {
                retData.entries[i].category = AlmanacCategory.Farming;
                // FARMING
                switch (i- itemEntries)
                {
                    case 0:
                        title = "Plots of Land";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Foundation for growth";
                        description = "A square of ground that is able to hold a plant for growing, harvesting and grafting. Plots of land begin as wild, and must be worked to be tilled and ready for planting. Digging a hole in the plot uproots plants and allows fertilizer to be dropped in.";
                        detailA = "Planting";
                        detailB = "Harvesting";
                        detailC = "Grafting";
                        detailD = "";
                        detailE = "";
                        break;
                    case 1:
                        title = "Working the Land";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Good work pays off";
                        description = "Plots are able to be worked from wild to dirt, to tilled and planted, and after harvest, to be uprooted, so it can be cycled back to dirt and tilled again. Working the land improves the soil quality. Neglecting a plot with a plant or stalk in it will drain plot resources.";
                        detailA = "Tilling";
                        detailB = "Digging";
                        detailC = "Enriching";
                        detailD = "";
                        detailE = "";
                        break;
                    case 2:
                        title = "Sun and Moon";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Light of day and night";
                        description = "A key resource for plants is light. Most plants need sunlight to grow, but some (Dark Plants) need moonlight. While sunlight is available during each day, moonlight can be much dimmer during the new moon in the middle of each month. All light is dimmed with clouds.";
                        detailA = "Resource";
                        detailB = "Plants";
                        detailC = "Dark Plants";
                        detailD = "Moon Phases";
                        detailE = "";
                        break;
                    case 3:
                        title = "Water";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Source of growth";
                        description = "Plants need water to grow as well. When watering a plot, it will immediately be full, but begin to drain. A plot with a plant in it will drain water faster. Rain will automatically add to each plot’s water resource.";
                        detailA = "Resource";
                        detailB = "Plants";
                        detailC = "Watering";
                        detailD = "";
                        detailE = "";
                        break;
                    case 4:
                        title = "Soil";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Rich nutrients make rich farms";
                        description = "Soil quality is improved by working the land or adding fertilizer to a plot that has been uprooted. Fertilizer improves the soil quality dramatically. Soil quality is drained over time when a plot has a growing plant in it.";
                        detailA = "Resource";
                        detailB = "Plants";
                        detailC = "Fertilizer";
                        detailD = "";
                        detailE = "";
                        break;
                    case 5:
                        title = "Planting";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Begin at the beginning";
                        description = "Seeds can be planted manually by holding them and working the land when the plot is tilled. Seeds can also be planted by broadcasting, dropping them on tilled plots.";
                        detailA = "Seed";
                        detailB = "Plants";
                        detailC = "Working the Land";
                        detailD = "";
                        detailE = "";
                        break;
                    case 6:
                        title = "Harvesting";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "What you sow";
                        description = "Taking the fruit or flower of a fully grown plant produces a harvest. The quality of the harvested fruit is dependent on the conditions the plant grew under before harvest. To maximize harvest quality, biomancers care for the plant by maintaining its necessary resources, like sun, water and soil quality.";
                        detailA = "Plants";
                        detailB = "Fruit";
                        detailC = "Stalk";
                        detailD = "";
                        detailE = "";
                        break;
                    case 7:
                        title = "Transplanting";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Moving day";
                        description = "By digging up a plot with a plant, you extract the plant and leave a hole in the plot. The uprooted plant can be moved to another plot with a hole, and dropped into the hole. The plant will continue to grow in its new plot. Biomancers also use this technique to fertilize a plot with a re-fruiting plant.";
                        detailA = "Plants";
                        detailB = "Digging";
                        detailC = "Enriching";
                        detailD = "";
                        detailE = "";
                        break;
                    case 8:
                        title = "Grafting";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "More than the sum";
                        description = "A biomancer may combine the stalk of one plant and the fruit of another to form a more rare plant variety, but the two must be compatible. To graft, the biomancer holds a fruit and approaches a stalk that is currently growing in a plot. The resulting new plant will then grow.";
                        detailA = "Stalk";
                        detailB = "Fruit";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                }
            }
            else if (i < plantEntries)
            {
                retData.entries[i].category = AlmanacCategory.Plants;
                // PLANTS
                switch (i - farmEntries)
                {
                    // COMMON (10)
                    case 0:
                        title = "Corn";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Common Plant";
                        description = "Looks pretty corny to me!";
                        detailA = "";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 1:
                        title = "Tomato";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Common Plant";
                        description = "Very useful bad joke repellent.";
                        detailA = "";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 2:
                        title = "Carrot";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Common Plant";
                        description = "Be careful, one of these could be a rabbit hole trap!";
                        detailA = "";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 3:
                        title = "Poppy";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Common Plant";
                        description = "Despite Magister Eden’s best efforts, beginner biomancers are constantly trying to use these for explosion spells.";
                        detailA = "";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 4:
                        title = "Rose";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Common Plant";
                        description = "They’re shockingly flame resistant and are often used in fire spells. For some reason, while burning, they give off a sweet scent.";
                        detailA = "";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 5:
                        title = "Sunflower";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Common Plant";
                        description = "Filled with the might of the sun! For some reason, a few biomancers often call these Superflowers.";
                        detailA = "";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 6:
                        title = "Moonflower";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Common Plant";
                        description = "Filled with the might of the moon! Which is technically the might of the sun, but don’t bring that up in front of Magister Shady.";
                        detailA = "Dark Plant";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 7:
                        title = "Apple";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Common Plant";
                        description = "Land dwellers eat these to ward off doctors. It’s a strange tradition that Magisters have been studying for years.";
                        detailA = "Re-fruit";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 8:
                        title = "Orange";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Common Plant";
                        description = "If you keep them in the sun too long, these plants are said to change into an entirely different fruit. This has been proven false, but some biomancers try it anyway.";
                        detailA = "Re-fruit";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 9:
                        title = "Lemon";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Common Plant";
                        description = "This nefarious plant has the power to turn the face of any who eats it inside out. And shockingly, it’s not even magical!";
                        detailA = "Re-fruit";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    // UNCOMMON (11)
                    case 10:
                        title = "Lotus";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Uncommon Plant";
                        description = "A plant that blooms beautifully and glows with Arcana. An ancient biomancer brought this flower from another world.";
                        detailA = "";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 11:
                        title = "Marigold";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Uncommon Plant";
                        description = "The gold flower of the celestial trio. It’s been used in many get rich quick schemes because many amateur biomancers think it’s made of actual gold.";
                        detailA = "";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 12:
                        title = "Magnolia";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Uncommon Plant";
                        description = "The black flower of the celestial trio. Nobody really knows where it came from and everybody is too scared of learning the answer to find out.";
                        detailA = "Dark Plant";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 13:
                        title = "Myosotis";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Uncommon Plant";
                        description = "The blue flower of the celestial trio. These are often used in mourning ceremonies to honor passing biomancers and dying lands.";
                        detailA = "Dark Plant";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 14:
                        title = "Chrystalia";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Uncommon Plant";
                        description = "A sacred flower once found at the peak of a mountain so high it rose above the skies of Empyrea! Not really, a biomancer got bored and made a flower out of glass. But the other idea was much cooler, right?";
                        detailA = "";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 15:
                        title = "Pumpkin";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Uncommon Plant";
                        description = "A big orange plant with a face on it. A lot of biomancers enjoy hollowing these out and using them as decorations. Some, however, swear the plants whisper to them in the dark.";
                        detailA = "Dark Plant";
                        detailB = "Re-fruit";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 16:
                        title = "Underbloom";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Uncommon Plant";
                        description = "Plants from a darker world. They look strikingly similar to sunflowers yet are far darker and only grow in the dark. Their use in curses has earned them a negative reputation.";
                        detailA = "Dark Plant";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 17:
                        title = "Water Lily";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Uncommon Plant";
                        description = "Named after a magister of the past, these plants are easy to grow in water and make excellent dye. Growing these for a significant other has become a popular practice in Empyrea.";
                        detailA = "Re-fruit";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 18:
                        title = "Snowgrace";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Uncommon Plant";
                        description = "These flowers look similar to snowflakes and grow in great amounts. Unlike snowflakes, every one of them looks the same. However, they all have different magic patterns, so they’re still close enough.";
                        detailA = "Dark Plant";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 19:
                        title = "Popcorn";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Uncommon Plant";
                        description = "Delicious popcorn on the cob. One brave biomancer from many years ago decided to magically create this tasty treat. Other biomancers worshipped them as a deity for a few days for their achievement.";
                        detailA = "Grafted";
                        detailB = "Corn";
                        detailC = "Poppy";
                        detailD = "";
                        detailE = "";
                        break;
                    case 20:
                        title = "Eclipse Flower";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Uncommon Plant";
                        description = "Flowers of night and day, glowing brightly like an eclipse. Each flower reflects a different type of eclipse depending on the time of day.";
                        detailA = "Grafted";
                        detailB = "Sunflower";
                        detailC = "Moonflower";
                        detailD = "";
                        detailE = "";
                        break;
                    // RARE (10)
                    case 21:
                        title = "Golden Apple";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Rare Plant";
                        description = "An apple that gleams as if it were made of gold. And technically, they are! These apples contain real gold in them, yet are still edible and taste delicious. Quite the expensive snack.";
                        detailA = "Grafted";
                        detailB = "Apple";
                        detailC = "Marigold";
                        detailD = "";
                        detailE = "";
                        break;
                    case 22:
                        title = "Hollowbloom";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Rare Plant";
                        description = "These flowers are completely hollow and dark inside. Yet, when touched, they feel soft and almost squishy. This is because the shadows inside the flower’s hollow spaces become tangible. This has baffled biomancers for ages.";
                        detailA = "Dark Plant";
                        detailB = "Grafted";
                        detailC = "Underbloom";
                        detailD = "Pumpkin";
                        detailE = "";
                        break;
                    case 23:
                        title = "Mandrake";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Rare Plant";
                        description = "A legendary plant of old. Well, not quite. These plants were called mystical for looking like humans and all sorts of stories began circulating around them. But they aren’t actually that magical, just a bit hard to find.";
                        detailA = "";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 24:
                        title = "Frost Lily";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Rare Plant";
                        description = "An evolved species of the Lily flower line that’s cold as ice. Touching one with your tongue will cause it to stick, so be careful when messing with one. These plants glimmer brightly in the light.";
                        detailA = "Re-fruit";
                        detailB = "Grafted";
                        detailC = "Water Lily";
                        detailD = "Snowgrace";
                        detailE = "";
                        break;
                    case 25:
                        title = "Banana";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Rare Plant";
                        description = "A classic and tasty treat that many biomancers love to enjoy. Bananas are very healthy and recommended to be eaten as a morning snack. But be careful to not let them become cursed, or they’ll wreck havoc on your stomach.";
                        detailA = "Re-fruit";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 26:
                        title = "Coconut";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Rare Plant";
                        description = "An incredibly hard fruit that contains tons of magic inside. Biomancers have discovered that putting more magic into one and throwing it can cause it to violently explode. Perfect for pranks. (Note from Magister Eden: 'Do NOT use these as a prank!')";
                        detailA = "Re-fruit";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 27:
                        title = "Mysteria";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Rare Plant";
                        description = "The mysterious Mysteria, which come in a multitude of mesmerizing and mystical colors so magical they might make many marvel at their majesty.";
                        detailA = "Re-fruit";
                        detailB = "Grafted";
                        detailC = "Marigold";
                        detailD = "Magnolia or";
                        detailE = "Myosotis";
                        break;
                    case 28:
                        title = "Nightshade";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Rare Plant";
                        description = "Dangerous and magical plants that contain a magic poison. Anybody who ingests one is unable to use magic for a while, so it’s recommended that only experienced biomancers handle them. Thankfully, most biomancers are immune to the poison thanks to the Genesis Tree’s blessings.";
                        detailA = "Dark Plant";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 29:
                        title = "Crystal Rose";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Rare Plant";
                        description = "A rose that has come into contact with a certain type of magic. It is still being researched what this special magic is, but its effect on the rose is evident. The entire inner structure of the rose crystalizes and is yet still able to grow as a plant. This special magic can be found in Chrystalia plants, so biomancers use the two for research.";
                        detailA = "Grafted";
                        detailB = "Rose";
                        detailC = "Chrystalia";
                        detailD = "";
                        detailE = "";
                        break;
                    case 30:
                        title = "Yarrow";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Rare Plant";
                        description = "An intriguing plant, not for its appearance but because of its special magic. Yarrow plants are said to be able to heal anything with their special magic. So far this has proven true, but Magister Shady has taken on the task of finding this plant’s limits.";
                        detailA = "";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    // SPECIAL (10)
                    case 31:
                        title = "Dragonroot";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Special Plant";
                        description = "This root is said to be born at the same time as a dragon. In truth, they just look like a dragon curled up in sleep, but the story has become so popular that the plant is celebrated on Dragon Day. Maybe that itself is a form of magic.";
                        detailA = "Grafted";
                        detailB = "Mandrake";
                        detailC = "Any rare fruit";
                        detailD = "";
                        detailE = "";
                        break;
                    case 32:
                        title = "Winter Rose";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Special Plant";
                        description = "Containing the beauty and mysticism of winter itself, these special roses are the masterpiece of the late Magister Lily. She once said that they are a tribute to the comfort one can find in the cold.";
                        detailA = "Dark Plant";
                        detailB = "Re-fruit";
                        detailC = "Grafted";
                        detailD = "Frost Lily";
                        detailE = "Crystal Rose";
                        break;
                    case 33:
                        title = "Fleur-De-Lis";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Special Plant";
                        description = "A rather strange plant that was once used as the symbol of an ancient nation. Many biomancers theorize that it was created by the biomancers of that era, so they may forever be immortalized in the world.";
                        detailA = "";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 34:
                        title = "Tropicus";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Special Plant";
                        description = "A favorite for many biomancers, this special fruit is used as edible decorations in the summer for their incredible resilience to heat. There was once an overflow of these in the market because everybody wanted to grow them. Now, they are actually quite rare to find.";
                        detailA = "Re-fruit";
                        detailB = "Grafted";
                        detailC = "Orange, Banana";
                        detailD = "Coconut, Lemon";
                        detailE = "";
                        break;
                    case 35:
                        title = "Mourning Nyx";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Special Plant";
                        description = "Despite its name, the Mourning Nyx does not inherently cause sorrow or demise. Rather, it’s an incredible ingredient in making curses, so the curses it is used to make are responsible for the tears of hundreds of biomancers.";
                        detailA = "Dark Plant";
                        detailB = "Grafted";
                        detailC = "Eclipse Flower";
                        detailD = "Hollowbloom";
                        detailE = "";
                        break;
                    case 36:
                        title = "Blast Apple";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Special Plant";
                        description = "An explosive apple.That’s pretty much it. Handle with absolute care!";
                        detailA = "Re-fruit";
                        detailB = "Grafted";
                        detailC = "Popcorn";
                        detailD = "Golden Apple";
                        detailE = "";
                        break;
                    case 37:
                        title = "Pixie Plumeria";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Special Plant";
                        description = "These plants are called as such because they are so popular among the pixie community. They are used for almost everything. Decoration, consumption, medicine, and even magic conduction. A plant of all trades indeed.";
                        detailA = "";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 38:
                        title = "Fae Foxglove";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Special Plant";
                        description = "A royal plant that was first cultivated by the royal fae of old. They don’t actually do much, but are very nice to look at and can be used in some spells as ingredients.";
                        detailA = "Dark Plant";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 39:
                        title = "Druid’s Lotus";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Special Plant";
                        description = "These plants are very popular among biomancers because they can be grown in almost any conditions. They are said to contain the resilience of the forest itself.";
                        detailA = "Re-fruit";
                        detailB = "Grafted";
                        detailC = "Mysteria";
                        detailD = "Lotus";
                        detailE = "";
                        break;
                    case 40:
                        title = "Splat Berry";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Special Plant";
                        description = "Younger biomancers used to love throwing these around because they burst and spread a very thin layer of magical juice. However, this juice is incredibly sticky, so it’s a pain to get off of anything without magic.";
                        detailA = "Re-fruit";
                        detailB = "Graffted";
                        detailC = "Tomato";
                        detailD = "Pumpkin";
                        detailE = "";
                        break;
                    // UNIQUE (9)
                    case 41:
                        title = "Jazzmyne";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Unique Plant";
                        description = "These plants are unexpected, but unstoppable. They just keep surprising us.";
                        detailA = "Re-fruit";
                        detailB = "Blue Notes";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 42:
                        title = "Mashroom";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Unique Plant";
                        description = "This grand fungus springs from the remains of an old stump to dominate the forest nights.";
                        detailA = "Dark Plant";
                        detailB = "King of the Fungi";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 43:
                        title = "Herbal Pert";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Unique Plant";
                        description = "This one-of-a-kind plant has a way of making us move.";
                        detailA = "Trumpet Flower";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 44:
                        title = "Firefly Trap";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Unique Plant";
                        description = "Inviting and open until it is not. This plant harvests more than just the flower.";
                        detailA = "Dark Plant";
                        detailB = "Re-fruit";
                        detailC = "Fairy";
                        detailD = "";
                        detailE = "";
                        break;
                    case 45:
                        title = "Betting Hedge";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Unique Plant";
                        description = "This plant is timeless and mesmerizing. It may be difficult to walk away from it.";
                        detailA = "Re-fruit";
                        detailB = "Black";
                        detailC = "Red";
                        detailD = "";
                        detailE = "";
                        break;
                    case 46:
                        title = "Bawn Sigh";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Unique Plant";
                        description = "There is a subtle punishment about this plant. Guard against it.";
                        detailA = "Small";
                        detailB = "Wall";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 47:
                        title = "Willow Wisp";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Unique Plant";
                        description = "It comes once in a blue moon, and when you see it, you know it was special.";
                        detailA = "Dark Plant";
                        detailB = "Glow-in-the-dark";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 48:
                        title = "Walking Stick";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Unique Plant";
                        description = "This plant grows a face and eyes that seem to follow you. A wise biomancer once enchanted their staff to become this form plant. Its magical fruit is very valuable.";
                        detailA = "Re-fruit";
                        detailB = "Scroll";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 49:
                        title = "Genesis Sapling";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Unique Plant";
                        description = "Renewal is a part of life, and so it is with the Genesis Tree. If you are lucky enough to witness the birth of a Genesis Sapling, consider yourself blessed.";
                        detailA = "Sprout";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                }
            }
            else if (i < magicEntries)
            {
                retData.entries[i].category = AlmanacCategory.Magic;
                // MAGIC
                switch (i - plantEntries)
                {
                    case 0:
                        title = "Crafting Magic Spells";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Biomancer skills";
                        description = "A biomancer uses magic to enrich the world, from their farm to the community around them. Every biomancer uses their spell library and crafting table, found in their wizard tower. To craft magic spells involves collection of ingredients and careful thinking.";
                        detailA = "Magic Spells";
                        detailB = "Crafting";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 1:
                        title = "The Grimoire";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "The book of knowledge";
                        description = "When biomancers need to craft new spells, they consult the Grimoire, their collection of spell recipes. As one gains experience, the Grimoire will fill with new recipes. One needs to follow the recipes in order to craft its spell charge.";
                        detailA = "Magic Spells";
                        detailB = "Crafting";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 2:
                        title = "Spell Recipes";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Quality ingredients";
                        description = "Each spell requires special ingredients, carefully selected and brought to the crafting table. Specific types of ingredients are listed in each spell recipe. Once a biomancer has the ingredients in hand, the spell may be crafted using the Magic Cauldron.";
                        detailA = "Crafting";
                        detailB = "Ingredients";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 3:
                        title = "The Magic Cauldron";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Bubble, bubble";
                        description = "Ingredients for spells must be combined in unique ways for the spell charge to be created. The Magic Cauldron has the necessary heat and magic to place all the ingredients in the order required. Once all ingredients are in, a spell charge can be crafted.";
                        detailA = "Crafting";
                        detailB = "Ingredients";
                        detailC = "Spell Charges";
                        detailD = "";
                        detailE = "";
                        break;
                    case 4:
                        title = "The Magic Book";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Personal spell storage";
                        description = "Every biomancer carries with them a Magic Book, a collection of the spell charges they have crafted. Every charge in the spell book represents a single magical cast the biomancer can invoke as long as that spell type is not on cooldown.";
                        detailA = "Magic Casting";
                        detailB = "Spell Charges";
                        detailC = "Arcana Magic Skills";
                        detailD = "";
                        detailE = "";
                        break;
                    case 5:
                        title = "Spell Charges";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "One can never have too many";
                        description = "The crafting of magic creates a spell charge that is stored in the biomancer’s Magic Book. The biomancer may cast the spell at any time. Once cast, the charge is spent and that spell type will be on cooldown.";
                        detailA = "Magic Casting";
                        detailB = "Spell Cooldowns";
                        detailC = "Scrolls";
                        detailD = "";
                        detailE = "";
                        break;
                    case 6:
                        title = "Casting Magic Spells";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Abracadabra and hocus pocus";
                        description = "When a biomancer opens their Magic Book, and they read a spell charge from it, they are able to cast the spell out into the world. Their ability to direct the spell cast to any point means they can control what plot, plant, person or object is affected by the spell.";
                        detailA = "Spell Book";
                        detailB = "Spell Charges";
                        detailC = "Spell Cooldowns";
                        detailD = "";
                        detailE = "";
                        break;
                    case 7:
                        title = "Spell Casts";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Magical life";
                        description = "After a spell is cast, it lives in the world on its own. Some spells are instant, while others last a very long time. Some spells are even permanent. When a spell cast expires, its effect is undone.";
                        detailA = "Magic Casting";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 8:
                        title = "Spell Cooldowns";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Gather your strength";
                        description = "After a biomancer has cast a spell of a particular type, it is unavailable for a time, while the biomancer regains their strength. This cooldown time can be different for each spell. A cooldown can be eliminated with a potion.";
                        detailA = "Magic Casting";
                        detailB = "Spell Book";
                        detailC = "Potions";
                        detailD = "";
                        detailE = "";
                        break;
                    case 9:
                        title = "Plot Spells";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Enchanted ground";
                        description = "Spells that affect plots are able to increase soil quality, to keep the ground saturated with water, or even to shine sunlight upon it through a portal. Some spells are able to work the land, harvest or uproot plots.";
                        detailA = "Magic Spells";
                        detailB = "Magic Casting";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 10:
                        title = "Plant Spells";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Charmed life";
                        description = "Plant magic includes the ability to force plants to grow faster or slower, to cause their harvest quality to be greater or lesser, or even to cause a plant to produce more fruit. Some spells can force plants to re-fruit or guarantee seeds will also drop with harvest.";
                        detailA = "Magic Spells";
                        detailB = "Magic Casting";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 11:
                        title = "People Spells";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "There’s something about you";
                        description = "Care must be taken when casting spells on others. Typically, a spell designed to affect people is used by the biomancer on themselves. These spells include the ability to change appearance, leave a color trail or shine a light overhead.";
                        detailA = "Magic Spells";
                        detailB = "Magic Casting";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 12:
                        title = "Place Spells";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Sacred structure";
                        description = "Rare magic can affect larger structures, and often this comes in the form of cosmetic decoration for more festive times. These spells include displays in the sky and changing the color of a biomancer’s tower.";
                        detailA = "Magic Spells";
                        detailB = "Magic Casting";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 13:
                        title = "Item Spells";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Transformation and evolution";
                        description = "A biomancer will commonly use magic to affect items that need attention. Some spells can gather plant material and deposit it into the compost bin, or gather harvest items and transport them to market.";
                        detailA = "Magic Spells";
                        detailB = "Magic Casting";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 14:
                        title = "Active Magic Skills";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "Biomancer cantrips";
                        description = "Some advanced Arcana skills include active magic. These act like spells in a biomancer’s Magic Book, but their charges are never depleted and they have no cooldown time.";
                        detailA = "Arcana Skills";
                        detailB = "Magic Casting";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                }
            }
            else if (i < eventEntries)
            {
                retData.entries[i].category = AlmanacCategory.Events;
                // EVENTS
                switch (i - magicEntries)
                {
                    case 0:
                        title = "Morning";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Rise and shine";
                        description = "When the sun rises on the horizon, the start of a new day is at hand. The morning time may include special events for those keen enough to listen. It is the time when one may have a sense that a wild plant is growing nearby.";
                        detailA = "Dawn";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 1:
                        title = "Noon";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Heat of the day";
                        description = "Most biomancers find this time of day to be the most productive. It is also the time of day when the traveling salesman pays a visit, but only on the first of each month.";
                        detailA = "Day";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 2:
                        title = "Mail Delivery";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Neither snow, nor rain,..";
                        description = "A biomancer’s mailbox springs to life at this time of day, and you may be lucky enough to receive well wishes and greetings in the form of letters or even care packages. Remember to check your mailbox every day.";
                        detailA = "Package";
                        detailB = "Letter";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 3:
                        title = "Evening";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Pretty sunsets";
                        description = "As the sun goes down and day turns to night, one may be lucky enough to get a visit to their farm from the seed fairy. At this time of day, it is common for winds to blow.";
                        detailA = "Dusk";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 4:
                        title = "Midnight";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "The dark of night";
                        description = "In the middle of the night, while no one is watching, some things move that normally do not move. Also, there is a rare chance of a farm visit from the gold fairy. The market will also change their daily specials at the stroke of midnight.";
                        detailA = "Night";
                        detailB = "";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 5:
                        title = "Daily";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Every day is a new beginning";
                        description = "To make the most of each day, a biomancer surveys the bounty of the land, takes note of the opportunities at the market, and follows their intuition. A biomancer finds their way each day by deciding to start and not deciding to stop.";
                        detailA = "Market";
                        detailB = "Visitors";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 6:
                        title = "Weather";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Unpredictable";
                        description = "Some days and nights are clear and calm. Others are filled with blustering winds or covered with dark clouds and sheets of rain. Some biomancers enjoy the variable weather, even when it is dramatic. Others prefer to find activities indoors, like at the chicken races, crafting magic or just cozying up to a warm fireplace.";
                        detailA = "Wind";
                        detailB = "Clouds";
                        detailC = "Rain";
                        detailD = "";
                        detailE = "";
                        break;
                    case 7:
                        title = "New Moon";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Extra dark";
                        description = "The moon can be full and bright, lighting up the night sky and shining lots of moonlight on dark plants. But, in the middle of each month, the moon is covered in shadow and shines almost no moonlight at all. Dark plants grow very slowly during the new moon.";
                        detailA = "Market";
                        detailB = "Moon Phases";
                        detailC = "Dark Plant";
                        detailD = "";
                        detailE = "";
                        break;
                    case 8:
                        title = "Monthly";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Mark your calendar";
                        description = "Each month brings a full moon, a new moon and a fresh new set of days to work with. In a single month, a biomancer can get a lot done, and amass quite a lot of gold. On the first day of each month, the traveling salesman comes to visit at noon.";
                        detailA = "Island Upgrades";
                        detailB = "Full Moon";
                        detailC = "Dark Plant";
                        detailD = "";
                        detailE = "";
                        break;
                    case 9:
                        title = "Seasons";
                        revealed = true;
                        icon = "GenesisTree";
                        subtitle = "Turn, turn, turn";
                        description = "Some plants grow better at night, or during the day. Some plants grow better during particular seasons. As the seasons change, the weather may get warmer or cooler, and your plants will change as well. The market is likely to offer specials based on season.";
                        detailA = "Plants";
                        detailB = "Market";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                }
            }
            else if (i < secretEntries)
            {
                retData.entries[i].category = AlmanacCategory.Secrets;
                // SECRETS
                switch (i - eventEntries)
                {
                    case 0:
                        title = "No Stone Unturned";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "The secret life of rocks";
                        description = "This rock is just a rock. Or, is it? Wasn’t this rock over there? Wait, there’s another rock. Where did that other rock go? Maybe this rock will make a good pet.";
                        detailA = "Rock";
                        detailB = "Midnight";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 1:
                        title = "Poor Biomancer";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "A small helping hand";
                        description = "Accidents happen. It’s okay, and we can grow from setbacks as easily as we grow any other time. When one needs help, they need only look to those who enjoy helping others.";
                        detailA = "Fairy";
                        detailB = "Gold";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 2:
                        title = "Uniquely Yours";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "More rare than rare";
                        description = "The unique mysteries in life are endless. Wonder springs everywhere, if one simply looks for it. Enjoy the rarest of gifts, including that of enjoyment itself.";
                        detailA = "Plants";
                        detailB = "Unique";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                    case 3:
                        title = "Pumpkin Eater";
                        revealed = false;
                        icon = "GenesisTree";
                        subtitle = "The secret of cheats";
                        description = "Tsk, tsk, tsk.";
                        detailA = "Cheat";
                        detailB = "Codes";
                        detailC = "";
                        detailD = "";
                        detailE = "";
                        break;
                }
            }

            retData.entries[i].title = title;
            retData.entries[i].revealed = revealed;
            retData.entries[i].icon = icon;
            retData.entries[i].subtitle = subtitle;
            retData.entries[i].description = description;
            int numDetails = 0;
            if (detailA != "")
                numDetails++;
            if (detailB != "")
                numDetails++;
            if (detailC != "")
                numDetails++;
            if (detailD != "")
                numDetails++;
            if (detailE != "")
                numDetails++;
            retData.entries[i].details = new string[numDetails];
            if (numDetails > 0)
                retData.entries[i].details[0] = detailA;
            if (numDetails > 1)
                retData.entries[i].details[1] = detailB;
            if (numDetails > 2)
                retData.entries[i].details[2] = detailC;
            if (numDetails > 3)
                retData.entries[i].details[3] = detailD;
            if (numDetails > 4)
                retData.entries[i].details[4] = detailE;

            
            /*
            if (i > 0 && (!retData.entries[i].revealed || retData.entries[i].title == ""))
            {
                // temp lorem entries for debug (will use lorem to hide entries)
                int rnd = 1 + UnityEngine.Mathf.RoundToInt(RandomSystem.FlatRandom01() * 3);
                retData.entries[i].title = GenerateLoremIpsum(rnd, "").TrimEnd(char.Parse("."));
                rnd = 1 + UnityEngine.Mathf.RoundToInt(RandomSystem.FlatRandom01() * 6);
                retData.entries[i].subtitle = GenerateLoremIpsum(rnd, "");
                rnd = 1 + UnityEngine.Mathf.RoundToInt(RandomSystem.FlatRandom01() * 28);
                retData.entries[i].description = GenerateLoremIpsum(rnd, "");
                rnd = 1 + UnityEngine.Mathf.RoundToInt(RandomSystem.FlatRandom01() * 4);
                retData.entries[i].details = new string[rnd];
                for (int n = 0; n < retData.entries[i].details.Length; n++)
                {
                    rnd = 1 + UnityEngine.Mathf.RoundToInt(RandomSystem.FlatRandom01() * 3);
                    retData.entries[i].details[n] = GenerateLoremIpsum(rnd, "").TrimEnd(char.Parse("."));
                }
            }
            */
            
        }

        return retData;
    }
}
