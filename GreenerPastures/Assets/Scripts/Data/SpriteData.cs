// REVIEW: necessary namespaces

// REVIEW: if animation holds are needed, make an animation frame data, use array
// REVIEW: perhaps a dedicated set of data and images should be used for items

[System.Serializable]
public class SpriteData
{
    public string name;
    public ItemType type;
    public int spriteIndexBase;
    public int spriteAnimLength;
    public float animFrameTime;
    public bool animLoop;
}

[System.Serializable]
public class SpriteLibraryData
{
    public SpriteData[] sprites;
}