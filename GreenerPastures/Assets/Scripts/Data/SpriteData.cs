// REVIEW: necessary namespaces

// REVIEW: if animation holds are needed, make an animation frame data, use array

[System.Serializable]
public class SpriteData
{
    public string name;
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