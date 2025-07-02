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
