// REVIEW: necessary namespaces

public static class AlmanacSystem
{
    public static AlmanacEntry InitializeEntry()
    {
        AlmanacEntry retEntry = new AlmanacEntry();

        retEntry.title = "";
        retEntry.category = AlmanacCateogory.Default;
        retEntry.revealed = false;
        retEntry.icon = "";
        retEntry.subtitle = "";
        retEntry.description = "";
        retEntry.details = new string[0];

        return retEntry;
    }

    public static string ConvertToLorem(string nonLorem)
    {
        string retString = "";

        if (nonLorem == null || nonLorem.Length == 0)
        {
            UnityEngine.Debug.LogWarning("--- AlmanacSystem [ConvertToLorem] : input string invalid. will return empty string.");
            return retString;
        }

        string[] words = nonLorem.Split(char.Parse(" "));
        retString = GenerateLoremIpsum(words.Length);

        return retString;
    }

    // REVIEW: this is generating random lorem, procedural lorem instead?
    // procedural lorem would have to be based on the original string

    public static string GenerateLoremIpsum(int loremLength)
    {
        string retLorem = "";

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
        for (int i = 0; i < loremLength; i++)
        {
            int idx = UnityEngine.Mathf.RoundToInt(RandomSystem.FlatRandom01() * (float)total);
            if (i < loremLength - 4 && commaInterval > baseCommaInterval + ((baseCommaInterval * RandomSystem.FlatRandom01() * variance) - baseCommaInterval / 2f))
            {
                retLorem += words[idx] + ", ";
                commaInterval = 0;
            }
            else if (i < loremLength - 2 && periodInterval > basePeriodInterval + ((basePeriodInterval * RandomSystem.FlatRandom01() * variance) - basePeriodInterval / 2f))
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

        retEntry.title = ConvertToLorem(retEntry.title);
        retEntry.icon = "GenesisTree"; // default hidden entry icon
        retEntry.subtitle = ConvertToLorem(retEntry.subtitle);
        retEntry.description = ConvertToLorem(retEntry.description);
        if (retEntry.details != null)
        {
            for (int i = 0; i < retEntry.details.Length; i++)
            {
                retEntry.details[i] = ConvertToLorem(retEntry.details[i]);
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

        int loreEntries = 5;
        int peopleEntries = 3;
        int placeEntries = 4;
        int itemEntries = 7;
        int farmEntries = 5;
        int plantEntries = 42;
        int magicEntries = 15;
        int eventEntries = 12;
        int secretEntries = 7;

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

            if (i < loreEntries)
            {
                retData.entries[i].category = AlmanacCateogory.Lore;
                // LORE
                // (generic almanac entry format with optional fields for data)
                // set revealed to false if this should be hidden
                // use 'genesistree' as icon name for now, we need icons
                if (i == 0)
                {
                    retData.entries[i].title = "The Sample Almanac Entry";
                    retData.entries[i].revealed = true;
                    retData.entries[i].icon = "GenesisTree";
                    retData.entries[i].subtitle = "Every journey begins with a single step.";
                    retData.entries[i].description = "Once upon a time, a squirrel found a nut in the tallest tree in the forest. The nuts was so marvelous he vowed to fetch it and become king of all squirrels. So, he climbed and climbed, and he was never seen again.";
                    retData.entries[i].details = new string[3];
                    retData.entries[i].details[0] = "Nut";
                    retData.entries[i].details[1] = "Squirrel";
                    retData.entries[i].details[2] = "Tree";
                }
            }
            else if (i < peopleEntries)
            {
                retData.entries[i].category = AlmanacCateogory.People;
                // PEOPLE
            }
            else if (i < placeEntries)
            {
                retData.entries[i].category = AlmanacCateogory.Places;
                // PLACES
            }
            else if (i < itemEntries)
            {
                retData.entries[i].category = AlmanacCateogory.Items;
                // ITEMS
            }
            else if (i < farmEntries)
            {
                retData.entries[i].category = AlmanacCateogory.Farming;
                // FARMING
            }
            else if (i < plantEntries)
            {
                retData.entries[i].category = AlmanacCateogory.Plants;
                // PLANTS
            }
            else if (i < magicEntries)
            {
                retData.entries[i].category = AlmanacCateogory.Magic;
                // MAGIC
            }
            else if (i < eventEntries)
            {
                retData.entries[i].category = AlmanacCateogory.Events;
                // EVENTS
            }
            else if (i < secretEntries)
            {
                retData.entries[i].category = AlmanacCateogory.Secrets;
                // SECRETS
            }

            // temp lorem entries (will use lorem to hide entries)
            int rnd = 1 + UnityEngine.Mathf.RoundToInt( RandomSystem.FlatRandom01() * 3 );
            retData.entries[i].title = GenerateLoremIpsum(rnd).TrimEnd(char.Parse("."));
            rnd = 1 + UnityEngine.Mathf.RoundToInt(RandomSystem.FlatRandom01() * 6);
            retData.entries[i].subtitle = GenerateLoremIpsum(rnd);
            rnd = 1 + UnityEngine.Mathf.RoundToInt(RandomSystem.FlatRandom01() * 28);
            retData.entries[i].description = GenerateLoremIpsum(rnd);
            rnd = 1 +UnityEngine.Mathf.RoundToInt(RandomSystem.FlatRandom01() * 4);
            retData.entries[i].details = new string[rnd];
            for (int n = 0; n < retData.entries[i].details.Length; n++)
            {
                rnd = 1 + UnityEngine.Mathf.RoundToInt(RandomSystem.FlatRandom01() * 3);
                retData.entries[i].details[n] = GenerateLoremIpsum(rnd).TrimEnd(char.Parse("."));
            }
        }

        return retData;
    }
}
