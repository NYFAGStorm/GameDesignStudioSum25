// REVIEW: necessary namespaces

// REVIEW:
public enum ProfileState
{
    Default,
    Creating,
    Offline,
    Connecting,
    Playing,
    Disconnecting
}

[System.Serializable]
public class ProfileData
{
    public string loginName;
    public string loginPass;
    public ProfileState state;
    public ProfileOptionsData options;
}
