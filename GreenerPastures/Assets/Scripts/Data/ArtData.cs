// REVIEW: necessary namespaces

// REVIEW: if animation holds are needed, make an animation frame data, use array
// REVIEW: perhaps a dedicated set of data and images should be used for items

[System.Serializable]
public class ArtData
{
    public string name;
    public ItemType type;
    public int artIndexBase;
    public int artAnimLength;
    public float animFrameTime;
    public bool animLoop;
}

[System.Serializable]
public class ArtLibraryData
{
    public ArtData[] images;
}