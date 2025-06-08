using UnityEngine;

public class SpriteLibraryManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles multiple libraries of sprite arrays, including animation sequences

    // NOTE:
    // may have multiple data and sprite arrays per type
    // (bg,char,item,vfx and ui)
    // or by any other category division necessary to maintain sprites easily

    public SpriteLibraryData itemSpriteData;
    public Sprite[] itemSprites;


    void Start()
    {
        // validate
        if ( itemSpriteData == null )
        {
            Debug.LogError("--- SpriteLibraryManager [Start] : no item sprite data found. aborting.");
            enabled = false;
        }
        else if ( itemSpriteData.sprites == null || itemSpriteData.sprites.Length == 0 )
        {
            Debug.LogError("--- SpriteLibraryManager [Start] : empty item sprite data. aborting.");
            enabled = false;
        }
        if (itemSprites == null || itemSprites.Length == 0)
        {
            Debug.LogError("--- SpriteLibraryManager [Start] : no item sprites found. aborting.");
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
    /// Gets sprite and animation data by item type (uses item type as name)
    /// </summary>
    /// <param name="itemType">item type</param>
    /// <returns>sprite data (empty data if failed)</returns>
    public SpriteData GetSpriteData( ItemType itemType )
    {
        SpriteData retData = new SpriteData();

        // validate
        bool found = false;
        int index = -1;
        for (int i=0; i<itemSpriteData.sprites.Length; i++)
        {
            if (itemSpriteData.sprites[i].type == itemType)
            {
                found = true;
                index = i;
                break;
            }
        }
        if (!found)
        {
            Debug.LogWarning("--- SpriteLibraryManager [GetSpriteData] : no data found for type "+itemType.ToString()+". will return null data.");
            return retData;
        }

        retData = itemSpriteData.sprites[index];

        return retData;
    }

    /// <summary>
    /// Gets array of sprites referenced by sprite data, base and anim sequence
    /// </summary>
    /// <param name="data">sprite data</param>
    /// <returns>array of sprites, one or more in length (unless failed)</returns>
    public Sprite[] GetSpriteList( SpriteData data )
    {
        // validate
        if (data == null || data.spriteIndexBase < 0 || 
            (data.spriteIndexBase + data.spriteAnimLength) > itemSprites.Length )
        {
            Debug.LogError("--- SpriteLibraryManager [GetSpriteList] : data null or index out of range. will return null data.");
            return new Sprite[0];
        }

        Sprite[] retSprites = new Sprite[data.spriteAnimLength + 1];

        for (int i=0; i<retSprites.Length; i++)
        {
            retSprites[i] = itemSprites[data.spriteIndexBase + i];
        }

        return retSprites;
    }
}
