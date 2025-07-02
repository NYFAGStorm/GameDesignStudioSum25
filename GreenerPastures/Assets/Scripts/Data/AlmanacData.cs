// REVIEW: necessary namespaces

public enum AlmanacCateogory
{
    Default,
    Lore,
    People,
    Places,
    Items,
    Farming,
    Plants,
    Magic,
    Events,
    Secrets
}

[System.Serializable]
public struct AlmanacEntry
{
    public string title;
    public AlmanacCateogory category;
    public bool revealed;
    public string icon; // art reference
    public string subtitle;
    public string description;
    public string[] details;
}

[System.Serializable]
public class AlmanacData
{
    public AlmanacEntry[] entries;
}
