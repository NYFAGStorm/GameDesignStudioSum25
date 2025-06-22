using UnityEngine;

public class ArtLibraryManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles multiple libraries of art image arrays, including animation sequences

    // NOTE: 
    // may have multiple data and art arrays per type
    // (bg,char,item,vfx and ui)
    // or by any other category division necessary to maintain images easily

    public ArtLibraryData itemArtData;
    public Texture2D[] itemImages;


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

        }
    }

    void Update()
    {
        
    }

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
        if (!found)
        {
            // TEMP: suppressed this warning until we have art for plant types?
            //Debug.LogWarning("--- ArtLibraryManager [GetArtData] : no data found for item type " + itemType.ToString() + " and plant type " + plantType.ToString() + ". will return null data.");
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
}
