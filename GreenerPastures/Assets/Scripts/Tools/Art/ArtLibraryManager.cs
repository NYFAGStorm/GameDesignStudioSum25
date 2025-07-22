using UnityEngine;

public class ArtLibraryManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles multiple libraries of art image arrays, including animation sequences

    // REFACTOR: art library absolutely needs to be re-designed and refactored from data to system to tool
    // (we can put up with it for now as a simple static item art libraray, nothing more)

    public ArtLibraryData itemArtData;
    public Texture2D[] itemImages;

    const int GENERALITEMS = 13;
    const int COMMONPLANTS = 10;
    const int UNCOMMONPLANTS = 11;
    const int RAREPLANTS = 10;
    const int SPECIALPLANTS = 10;
    const int UNIQUEPLANTS = 9;


    void Start()
    {
        // validate
        if (itemArtData == null )
        {
            Debug.LogError("--- ArtLibraryManager [Start] : no item art data found. aborting.");
            enabled = false;
        }
        else if (itemArtData.images == null || itemArtData.images.Length == 0 )
        {
            Debug.LogError("--- ArtLibraryManager [Start] : empty item art data. aborting.");
            enabled = false;
        }
        if (itemImages == null || itemImages.Length == 0)
        {
            Debug.LogError("--- ArtLibraryManager [Start] : no item images found. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            itemArtData = new ArtLibraryData();
            itemArtData.images = new ArtData[0];
            itemImages = new Texture2D[0];

            InitializeGeneralItemArt();
            InitializePlantArt();
        }
    }

    void Update()
    {
        
    }

    /*
    /// <summary>
    /// Gets art and animation data by item type (uses item type as name)
    /// </summary>
    /// <param name="itemType">item type</param>
    /// <returns>art data (empty data if failed)</returns>
    public ArtData GetArtData( ItemType itemType )
    {
        ArtData retData = new ArtData();

        // validate
        bool found = false;
        int index = -1;
        for (int i=0; i<itemArtData.images.Length; i++)
        {
            if (itemArtData.images[i].type == itemType)
            {
                found = true;
                index = i;
                break;
            }
        }
        if (!found)
        {
            Debug.LogWarning("--- ArtLibraryManager [GetArtData] : no data found for type " + itemType.ToString()+". will return null data.");
            return retData;
        }

        retData = itemArtData.images[index];

        return retData;
    }
    */

    /// <summary>
    /// Gets art and animation data by item type (uses item type as name) and plant type
    /// </summary>
    /// <param name="itemType">item type</param>
    /// <param name="plantType">plant type</param>
    /// <returns>art data (empty data if failed)</returns>
    public ArtData GetArtData(ItemType itemType, PlantType plantType)
    {
        ArtData retData = new ArtData();

        // validate
        bool found = false;
        int index = -1;
        for (int i = 0; i < itemArtData.images.Length; i++)
        {
            if (itemArtData.images[i].type == itemType &&
                itemArtData.images[i].plant == plantType)
            {
                found = true;
                index = i;
                break;
            }
        }
        // if not found particular plant type item, get default type
        if (!found)
        {
            for (int i = 0; i < itemArtData.images.Length; i++)
            {
                if (itemArtData.images[i].type == itemType &&
                    itemArtData.images[i].plant == PlantType.Default)
                {
                    found = true;
                    index = i;
                    break;
                }
            }
        }
        if (!found)
        {
            Debug.LogWarning("--- ArtLibraryManager [GetArtData] : no data found for item type " + itemType.ToString() + " and plant type " + plantType.ToString() + ". will return null data.");
            return retData;
        }

        retData = itemArtData.images[index];

        return retData;
    }

    /// <summary>
    /// Gets array of images referenced by art data, base and anim sequence
    /// </summary>
    /// <param name="data">art data</param>
    /// <returns>array of images, one or more in length (unless failed)</returns>
    public Texture2D[] GetImageList( ArtData data )
    {
        // validate
        if (data == null || data.artIndexBase < 0 || 
            (data.artIndexBase + data.artAnimLength) > itemImages.Length )
        {
            Debug.LogError("--- ArtLibraryManager [GetImageList] : data null or index out of range. will return null data.");
            return new Texture2D[0];
        }

        Texture2D[] retImages = new Texture2D[data.artAnimLength + 1];

        for (int i=0; i< retImages.Length; i++)
        {
            retImages[i] = itemImages[data.artIndexBase + i];
        }

        return retImages;
    }

    void AddToArtData( ArtData[] data )
    {
        if (data == null || data.Length == 0)
            return;

        ArtData[] tmp = new ArtData[itemArtData.images.Length + data.Length];
        for (int i = 0; i < itemArtData.images.Length; i++)
        {
            tmp[i] = itemArtData.images[i];
        }
        int prevLength = 0;
        int indexExtent = 0;
        if (itemArtData != null && itemArtData.images != null && 
            itemArtData.images.Length > 0)
        {
            prevLength = itemArtData.images.Length;
            indexExtent = itemArtData.images[itemArtData.images.Length - 1].artIndexBase +
                itemArtData.images[itemArtData.images.Length - 1].artAnimLength;
            indexExtent++;
        }
        for (int i = 0; i < data.Length; i++)
        {
            data[i].artIndexBase += indexExtent;
            tmp[i + prevLength] = data[i];
        }
        itemArtData.images = tmp;
    }

    void AddToImages( Texture2D newImage )
    {
        if (newImage == null)
            return;

        Texture2D[] tmp = new Texture2D[itemImages.Length + 1];
        for (int i = 0; i < itemImages.Length; i++)
        {
            tmp[i] = itemImages[i];
        }
        tmp[itemImages.Length] = newImage;
        itemImages = tmp;
    }

    ArtData ConfigItemArtData( string iName, ItemType iType, PlantType pType, int artIdx )
    {
        ArtData retArtData = new ArtData();

        retArtData.name = iName;
        retArtData.type = iType;
        retArtData.plant = pType;
        retArtData.artIndexBase = artIdx;

        return retArtData;
    }

    void InitializeGeneralItemArt()
    {
        // GENERAL ITEMS
        ArtData[] newArtData = new ArtData[GENERALITEMS];

        string itemName = "";
        int idx = 0;

        // fertilizer
        itemName = "Fertilizer";
        newArtData[idx] = ConfigItemArtData(itemName, ItemType.Fertilizer, PlantType.Default, idx);
        AddToImages((Texture2D)Resources.Load("Item_" + itemName));
        idx++;

        // default plant items (no actual art available)
        itemName = "Seed";
        newArtData[idx] = ConfigItemArtData(itemName, ItemType.Seed, PlantType.Default, idx);
        AddToImages((Texture2D)Resources.Load("Item_" + itemName));
        idx++;
        itemName = "Plant";
        newArtData[idx] = ConfigItemArtData(itemName, ItemType.Plant, PlantType.Default, idx);
        AddToImages((Texture2D)Resources.Load("Item_" + itemName));
        idx++;
        itemName = "Stalk";
        newArtData[idx] = ConfigItemArtData(itemName, ItemType.Stalk, PlantType.Default, idx);
        AddToImages((Texture2D)Resources.Load("Item_" + itemName));
        idx++;
        itemName = "Fruit";
        newArtData[idx] = ConfigItemArtData(itemName, ItemType.Fruit, PlantType.Default, idx);
        AddToImages((Texture2D)Resources.Load("Item_" + itemName));
        idx++;

        // rock
        itemName = "Rock";
        newArtData[idx] = ConfigItemArtData(itemName, ItemType.Rock, PlantType.Default, idx);
        AddToImages((Texture2D)Resources.Load("Item_" + itemName));
        idx++;

        // gold coin
        itemName = "Gold Coin";
        newArtData[idx] = ConfigItemArtData(itemName, ItemType.GoldCoin, PlantType.Default, idx);
        AddToImages((Texture2D)Resources.Load("Item_" + itemName));
        idx++;
        // gold sack
        itemName = "Gold Sack";
        newArtData[idx] = ConfigItemArtData(itemName, ItemType.GoldSack, PlantType.Default, idx);
        AddToImages((Texture2D)Resources.Load("Item_" + itemName));
        idx++;

        // package
        itemName = "Package";
        newArtData[idx] = ConfigItemArtData(itemName, ItemType.Package, PlantType.Default, idx);
        AddToImages((Texture2D)Resources.Load("Item_" + itemName));
        idx++;

        // letter
        itemName = "Letter";
        newArtData[idx] = ConfigItemArtData(itemName, ItemType.Letter, PlantType.Default, idx);
        AddToImages((Texture2D)Resources.Load("Item_" + itemName));
        idx++;
        // coupon
        itemName = "Coupon";
        newArtData[idx] = ConfigItemArtData(itemName, ItemType.Coupon, PlantType.Default, idx);
        AddToImages((Texture2D)Resources.Load("Item_" + itemName));
        idx++;

        // scroll
        itemName = "Scroll";
        newArtData[idx] = ConfigItemArtData(itemName, ItemType.Scroll, PlantType.Default, idx);
        AddToImages((Texture2D)Resources.Load("Item_" + itemName));
        idx++;
        // potion
        itemName = "Potion";
        newArtData[idx] = ConfigItemArtData(itemName, ItemType.Potion, PlantType.Default, idx);
        AddToImages((Texture2D)Resources.Load("Item_" + itemName));
        idx++;

        AddToArtData(newArtData);
    }

    void InitializePlantArt()
    {
        // PLANT ITEMS
        ArtData[] newArtData =
            new ArtData[3 * (COMMONPLANTS + UNCOMMONPLANTS + RAREPLANTS + SPECIALPLANTS + UNIQUEPLANTS)];

        string itemName;
        string plant;
        int idx;

        // REFACTOR: single function maybe?

        // common
        string rarity = "Common";
        int min = 0;
        int max = COMMONPLANTS;
        for (int i = min; i < max; i++)
        {
            idx = i * 3;
            plant = ((PlantType)i + 1).ToString(); // skip 'default' type
            itemName = "Item_" + rarity + "_" + plant;
            newArtData[idx] = ConfigItemArtData(plant, ItemType.Fruit, (PlantType)(i + 1), idx);
            AddToImages((Texture2D)Resources.Load(itemName));
            itemName = "Stalk_" + rarity + "_" + plant;
            newArtData[idx + 1] = ConfigItemArtData(plant, ItemType.Stalk, (PlantType)(i + 1), idx + 1);
            AddToImages((Texture2D)Resources.Load(itemName));
            itemName = "Plant_" + rarity + "_" + plant;
            newArtData[idx + 2] = ConfigItemArtData(plant, ItemType.Plant, (PlantType)(i + 1), idx + 2);
            AddToImages((Texture2D)Resources.Load(itemName));
        }

        // uncommon
        rarity = "Uncommon";
        min += COMMONPLANTS;
        max += UNCOMMONPLANTS;
        for (int i = min; i < max; i++)
        {
            idx = (i * 3);
            plant = ((PlantType)i + 1).ToString();
            itemName = "Item_" + rarity + "_" + plant;
            newArtData[idx] = ConfigItemArtData(plant, ItemType.Fruit, (PlantType)(i + 1), idx);
            AddToImages((Texture2D)Resources.Load(itemName));
            itemName = "Stalk_" + rarity + "_" + plant;
            newArtData[idx + 1] = ConfigItemArtData(plant, ItemType.Stalk, (PlantType)(i + 1), idx + 1);
            AddToImages((Texture2D)Resources.Load(itemName));
            itemName = "Plant_" + rarity + "_" + plant;
            newArtData[idx + 2] = ConfigItemArtData(plant, ItemType.Plant, (PlantType)(i + 1), idx + 2);
            AddToImages((Texture2D)Resources.Load(itemName));
        }

        // rare
        rarity = "Rare";
        min += UNCOMMONPLANTS;
        max += RAREPLANTS;
        for (int i = min; i < max; i++)
        {
            idx = (i * 3);
            plant = ((PlantType)i + 1).ToString();
            itemName = "Item_" + rarity + "_" + plant;
            newArtData[idx] = ConfigItemArtData(plant, ItemType.Fruit, (PlantType)(i + 1), idx);
            AddToImages((Texture2D)Resources.Load(itemName));
            itemName = "Stalk_" + rarity + "_" + plant;
            newArtData[idx + 1] = ConfigItemArtData(plant, ItemType.Stalk, (PlantType)(i + 1), idx + 1);
            AddToImages((Texture2D)Resources.Load(itemName));
            itemName = "Plant_" + rarity + "_" + plant;
            newArtData[idx + 2] = ConfigItemArtData(plant, ItemType.Plant, (PlantType)(i + 1), idx + 2);
            AddToImages((Texture2D)Resources.Load(itemName));
        }

        // special
        rarity = "Special";
        min += RAREPLANTS;
        max += SPECIALPLANTS;
        for (int i = min; i < max; i++)
        {
            idx = (i * 3);
            plant = ((PlantType)i + 1).ToString();
            itemName = "Item_" + rarity + "_" + plant;
            newArtData[idx] = ConfigItemArtData(plant, ItemType.Fruit, (PlantType)(i + 1), idx);
            AddToImages((Texture2D)Resources.Load(itemName));
            itemName = "Stalk_" + rarity + "_" + plant;
            newArtData[idx + 1] = ConfigItemArtData(plant, ItemType.Stalk, (PlantType)(i + 1), idx + 1);
            AddToImages((Texture2D)Resources.Load(itemName));
            itemName = "Plant_" + rarity + "_" + plant;
            newArtData[idx + 2] = ConfigItemArtData(plant, ItemType.Plant, (PlantType)(i + 1), idx + 2);
            AddToImages((Texture2D)Resources.Load(itemName));
        }
        
        // unique
        rarity = "Unique";
        min += SPECIALPLANTS;
        max += UNIQUEPLANTS;
        for (int i = min; i < max; i++)
        {
            idx = (i * 3);
            plant = ((PlantType)i + 1).ToString();
            itemName = "Item_" + rarity + "_" + plant;
            newArtData[idx] = ConfigItemArtData(plant, ItemType.Fruit, (PlantType)(i + 1), idx);
            AddToImages((Texture2D)Resources.Load(itemName));
            itemName = "Stalk_" + rarity + "_" + plant;
            newArtData[idx + 1] = ConfigItemArtData(plant, ItemType.Stalk, (PlantType)(i + 1), idx + 1);
            AddToImages((Texture2D)Resources.Load(itemName));
            itemName = "Plant_" + rarity + "_" + plant;
            newArtData[idx + 2] = ConfigItemArtData(plant, ItemType.Plant, (PlantType)(i + 1), idx + 2);
            AddToImages((Texture2D)Resources.Load(itemName));
        }

        AddToArtData(newArtData);
    }
}
